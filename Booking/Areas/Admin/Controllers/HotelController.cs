using AutoMapper;
using Booking.Areas.Admin.Models.Hotel;
using Booking.Controllers;
using Booking.EF;
using Booking.Models;
using Booking.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Booking.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("/admin/hotel/")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.ServiceProvider)]
    public class HotelController : Controller
    {
        private readonly ILogger<HotelController> _logger;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly CloudinaryController _cloudinary;

        private readonly int ITEM_PER_PAGE = 10;

        public HotelController
            (
                ILogger<HotelController> logger,
                AppDbContext context,
                IMapper mapper,
                CloudinaryController cloudinary
            )
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _cloudinary = cloudinary;
        }

        #region CategoryHotel
        [HttpGet("cate-hotel/")]
        public async Task<IActionResult> CateHotelIndex()
        {
            var cateHotel = await _context.CategoryHotels
                                .Where(ch => ch.IdParent == null)
                                .Include(ch => ch.CateHotelChildren)
                                .ThenInclude(ch => ch.CateHotelChildren)
                                .ToListAsync();

            return View(cateHotel);
        }

        [HttpGet("cate-hotel/create")]
        public async Task<IActionResult> CateHotelCreate()
        {
            await RenderSelectedCateHotel();

            return View();
        }

        [HttpPost("cate-hotel/create")]
        public async Task<IActionResult> CateHotelCreate(CategoryHotelDTO model)
        {
            try
            {
                await RenderSelectedCateHotel();

                if (ModelState.IsValid)
                {
                    var cateHotel = new CategoryHotel
                    {
                        Id = Guid.NewGuid(),
                        Name = model.Name,
                        Slug = AppUtilities.GenerateSlug(model.Name),
                        IsOutstanding = model.IsOutstanding,
                        IdParent = model.IdParent,
                    };

                    if (model.Avatar != null)
                    {
                        var files = new List<IFormFile>();
                        files.Add(model.Avatar);

                        var result = await _cloudinary.UploadMediaAsync(files) as JsonResult;
                        var data = result?.Value as IEnumerable<dynamic>;
                        var id = string.Empty;
                        foreach (var item in data)
                        {
                            id = item.Id.ToString();
                        }

                        if (!Guid.TryParse(id, out Guid guidId))
                        {
                            TempData["Message"] = "Error: Không tìm thấy id hình ảnh";
                            return View(model);
                        }

                        var media = await _context.Medias.FindAsync(guidId);
                        if (media == null)
                        {
                            TempData["Message"] = "Error: Không tìm thấy hình ảnh";
                            return View(model);
                        }

                        media.AuthorType = "CategoryHotel";
                        media.AuthorId = cateHotel.Id;
                        media.IsUsed = true;
                        media.IsAvatar = true;
                        media.UpdateAt = DateTime.UtcNow;

                        _context.Medias.Update(media);

                        cateHotel.Avatar = media.MediaUrl;
                    }

                    _context.CategoryHotels.Add(cateHotel);

                    await _context.SaveChangesAsync();

                    TempData["Message"] = "Success: Thêm thành công";
                    return RedirectToAction(nameof(CateHotelIndex));
                }

                TempData["Message"] = "Error: Có lỗi xảy ra";

                return View(model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx)
                {
                    // 2601: Cannot insert duplicate key row
                    // 2627: Violation of UNIQUE KEY constraint
                    if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                    {
                        TempData["Message"] = "Error: Tên danh mục đã được sử dụng";
                    }
                }

                _logger.LogError($"Error: {ex.InnerException}");
                return View(model);
            }
        }

        [HttpDelete("cate-hotel/delete")]
        public async Task<IActionResult> CateHotelDelete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new
                    {
                        Success = false,
                        Message = "Id không có dữ liệu"
                    });

                if (!Guid.TryParse(id, out var guidId))
                    return Json(new
                    {
                        Success = false,
                        Message = "Kiểu dữ liệu id không đúng"
                    });

                var cateHotel = await _context.CategoryHotels.FindAsync(guidId);
                if (cateHotel == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Không tìm thấy danh mục hotel"
                    });
                }

                _context.CategoryHotels.Remove(cateHotel);

                // Xóa tất cả ảnh theo cate hotel
                var listPublicId = await _context.Medias
                                .Where(m => m.AuthorType == "CategoryHotel"
                                && m.AuthorId == cateHotel.Id)
                                .Select(m => m.PublicId)
                                .ToListAsync();

                if (listPublicId.Count > 0)
                {
                    // Xóa ảnh trên cloundinary
                    await _cloudinary.DeleteMediaAsync(listPublicId);
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    Success = true,
                    Message = "Success"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        #endregion

        #region Utility
        [HttpGet("utility")]
        public async Task<IActionResult> UtilityIndex()
        {
            return View(await _context.Utilities.ToListAsync());
        }

        [HttpGet("utility/details/{id}")]
        public async Task<IActionResult> UtilityDetails(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var utility = await _context.Utilities
                .FirstOrDefaultAsync(m => m.Id == id);
            if (utility == null)
            {
                return NotFound();
            }

            return View(utility);
        }

        [HttpGet("utility/create")]
        public IActionResult UtilityCreate()
        {
            return View();
        }

        [HttpPost("utility/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UtilityCreate([Bind("Id,Icon,UtilityName")] Utility model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.Id = Guid.NewGuid();
                    _context.Add(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(UtilityIndex));
                }
                return View(model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx)
                {
                    // 2601: Cannot insert duplicate key row
                    // 2627: Violation of UNIQUE KEY constraint
                    if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                    {
                        TempData["Message"] = "Error: Tên tiện ích đã được sử dụng";
                    }
                }

                _logger.LogError($"Error: {ex.InnerException}");
                return View(model);
            }
        }

        [HttpGet("utility/edit/{id}")]
        public async Task<IActionResult> UtilityEdit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var utility = await _context.Utilities.FindAsync(id);
            if (utility == null)
            {
                return NotFound();
            }
            return View(utility);
        }

        [HttpPost("utility/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UtilityEdit(Guid id, [Bind("Id,Icon,UtilityName")] Utility model)
        {
            if (id != model.Id)
            {
                TempData["Message"] = "Error: Không tìm thấy dữ liệu";
                return View(model);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!UtilityExists(model.Id))
                    {
                        TempData["Message"] = "Error: Không tìm thấy dữ liệu";
                        return View(model);
                    }

                    model.UpdateAt = DateTime.UtcNow;

                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    if (ex.InnerException is SqlException sqlEx)
                    {
                        // 2601: Cannot insert duplicate key row
                        // 2627: Violation of UNIQUE KEY constraint
                        if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                        {
                            TempData["Message"] = "Error: Tên tiện ích đã được sử dụng";
                        }
                    }

                    _logger.LogError($"Error: {ex.InnerException}");
                    return View(model);
                }
                return RedirectToAction(nameof(UtilityIndex));
            }
            return View(model);
        }

        [HttpGet("utility/delete/{id}")]
        public async Task<IActionResult> UtilityDelete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var utility = await _context.Utilities
                .FirstOrDefaultAsync(m => m.Id == id);
            if (utility == null)
            {
                return NotFound();
            }

            return View(utility);
        }

        [HttpPost("utility/delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UtilityDeleteConfirmed(Guid id)
        {
            var utility = await _context.Utilities.FindAsync(id);
            if (utility != null)
            {
                _context.Utilities.Remove(utility);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(UtilityIndex));
        }

        private bool UtilityExists(Guid id)
        {
            return _context.Utilities.Any(e => e.Id == id);
        }
        #endregion

        #region Hotel
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            var hotels = await _context.Hotels.ToListAsync();
            var roles = HttpContext.User.Claims
                            .Where(c => c.Type == ClaimTypes.Role)
                            .Select(c => c.Value)
                            .ToList();
            var test = HttpContext.User.FindAll(ClaimTypes.Role);

            List<HotelDTO> listHotelDTO = new List<HotelDTO>();

            foreach (var hotel in hotels)
            {
                var hotelDTO = _mapper.Map<HotelDTO>(hotel);
                var media = await _context.Medias
                    .FirstOrDefaultAsync(m => m.AuthorType == "Hotel" && m.AuthorId == hotel.Id && m.IsAvatar);
                hotelDTO.Images = media?.MediaUrl;
                listHotelDTO.Add(hotelDTO);
            }

            var total = listHotelDTO.Count();
            var countPage = (int)Math.Ceiling((double)total / ITEM_PER_PAGE);
            page = page > countPage ? countPage : page;

            listHotelDTO = listHotelDTO.Skip((page - 1) * ITEM_PER_PAGE).Take(ITEM_PER_PAGE).ToList();

            ViewBag.CountPage = countPage;

            return View(listHotelDTO);
        }

        [HttpGet("details")]
        public async Task<IActionResult> Details(Guid id)
        {
            var hotel = await _context.Hotels.Include(h => h.Rooms)
                                .FirstOrDefaultAsync(m => m.Id == id);
            if (hotel == null)
            {
                TempData["Message"] = "Error: Không tìm thấy khách sạn";
                return View();
            }

            var hotelDTO = _mapper.Map<HotelDTO>(hotel);
            var listImage = await _context.Medias
                                    .Where(m => m.AuthorType == "Hotel"
                                    && m.AuthorId == hotel.Id)
                                    .ToListAsync();
            string images = string.Empty;
            foreach (var image in listImage)
            {
                images = string.IsNullOrEmpty(images)
                    ? $"{image.MediaType}:{image.MediaUrl}"
                    : images + $";{image.MediaType}:{image.MediaUrl}";
            }
            hotelDTO.Images = images;

            var utilities = await (from u in _context.Utilities
                                   join hu in _context.HotelUtilities on u.Id equals hu.IdUtility
                                   where hu.IdHotel == hotel.Id
                                   select u).ToListAsync();
            hotelDTO.Utilities = utilities;

            return View(hotelDTO);
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            var hotelDTO = new HotelDTO
            {
                Utilities = await _context.Utilities.ToListAsync(),
            };
            await RenderSelectedCateHotel();
            return View(hotelDTO);
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HotelDTO model, string avatarId)
        {
            try
            {
                await RenderViewHotel(model, true);
                if (!ModelState.IsValid)
                {
                    TempData["Message"] = "Error: Dữ liệu không đúng";
                    return View(model);
                }

                if (string.IsNullOrEmpty(model.Images))
                {
                    TempData["Message"] = "Error: Vui lòng thêm ảnh";
                    return View(model);
                }

                #region Hotel
                var hotel = new Hotel
                {
                    Id = Guid.NewGuid(),
                    HotelName = model.HotelName,
                    Address = model.Address,
                    ResidenceType = model.ResidenceType,
                    Star = model.Star,
                    Tag = model.Tag,
                    Slug = AppUtilities.GenerateSlug(model.HotelName),
                    Outstanding = model.Outstanding,
                    IdCateHotel = model.IdCateHotel,
                };
                #endregion

                #region HotelUtility
                model.SelectedUtilityIds.ForEach(idUtility =>
                {
                    var hotelUtility = new HotelUtility
                    {
                        IdHotel = hotel.Id,
                        IdUtility = idUtility
                    };

                    _context.HotelUtilities.Add(hotelUtility);
                });
                #endregion

                #region Image
                var listIdImage = model.Images.Split(";").ToList();

                foreach (var id in listIdImage)
                {
                    if (!Guid.TryParse(id, out var guidId)) // Convert string to Guid
                    {
                        TempData["Message"] = "Error: Dữ liệu không đúng";
                        return View(model);
                    }
                    var media = await _context.Medias.FindAsync(guidId);
                    if (media == null)
                    {
                        TempData["Message"] = "Error: Không tìm thấy hình ảnh";
                        return View(model);
                    }

                    if (avatarId == media.Id.ToString())
                        hotel.Avatar = media.MediaUrl;

                    media.IsAvatar = avatarId == media.Id.ToString();
                    media.IsUsed = true;
                    media.AuthorType = "Hotel";
                    media.AuthorId = hotel.Id;

                    _context.Medias.Update(media);
                }

                _context.Hotels.Add(hotel);
                #endregion

                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Thêm mới khách sạn thành công";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx)
                {
                    // 2601: Cannot insert duplicate key row
                    // 2627: Violation of UNIQUE KEY constraint
                    if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                        TempData["Message"] = "Error: Tên khách sạn đã được sử dụng";
                }

                _logger.LogError($"Error: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet("edit")]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (!CheckHotelId(id))
            {
                TempData["Message"] = "Error: Dữ liệu không hợp lệ";
                return RedirectToAction(nameof(Index));
            }

            var hotel = await _context.Hotels.FindAsync(id);

            var hotelDTO = _mapper.Map<HotelDTO>(hotel);

            await RenderViewHotel(hotelDTO);

            return View(hotelDTO);
        }

        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] HotelDTO model, List<Guid> selectedId, string avatarId)
        {
            try
            {
                await RenderViewHotel(model);
                if (!ModelState.IsValid)
                {
                    TempData["Message"] = "Error: Dữ liệu không đúng";
                    return View(model);
                }

                if (string.IsNullOrEmpty(model.Images))
                {
                    TempData["Message"] = "Error: Vui lòng thêm ảnh";
                    return View(model);
                }

                #region Hotel
                var hotel = await _context.Hotels.FindAsync(model.Id);
                if (!CheckHotelId(model.Id) || hotel == null)
                {
                    TempData["Message"] = "Error: Không tìm thấy khách sạn";
                    return View(model);
                }

                hotel.HotelName = model.HotelName;
                hotel.Address = model.Address;
                hotel.IdCateHotel = model.IdCateHotel;
                hotel.ResidenceType = model.ResidenceType;
                hotel.Star = model.Star;
                hotel.Tag = model.Tag;
                hotel.Slug = AppUtilities.GenerateSlug(model.HotelName);
                hotel.Outstanding = model.Outstanding;
                hotel.UpdateAt = DateTime.UtcNow;
                #endregion

                #region HotelUtilities
                // Lấy các guid tiện ích của khách sạn
                var utilities = await _context.HotelUtilities
                                        .Where(hu => hu.IdHotel == hotel.Id)
                                        .Select(hu => hu.IdUtility)
                                        .ToListAsync();
                // Xóa các tiện ích được bỏ chọn
                foreach (var utility in utilities)
                {
                    // Nếu các guid cũ không có trong guid mới thì xóa
                    // Kiểm tra guid mới có chứa guid cũ không
                    // Nếu ko chứa guid cũ thì xóa guid cũ
                    if (!selectedId.Contains(utility))
                    {
                        var hotelUtility = await _context.HotelUtilities
                                    .FirstOrDefaultAsync(hu => hu.IdUtility == utility);

                        _context.HotelUtilities.Remove(hotelUtility!);
                    }
                }

                // Thêm các tiện ích mới
                foreach (var selectedUti in selectedId)
                {
                    // Nếu các guid mới ko có trong guid cũ thì thêm mới
                    if (!utilities.Contains(selectedUti))
                    {
                        var hotelUtility = new HotelUtility
                        {
                            IdHotel = hotel.Id,
                            IdUtility = selectedUti,
                        };

                        _context.HotelUtilities.Add(hotelUtility);
                    }
                }
                #endregion

                #region Image
                var listIdImage = model.Images.Split(";").ToList();
                foreach (var id in listIdImage)
                {
                    if (!Guid.TryParse(id, out var guidId)) // Convert string to Guid
                    {
                        TempData["Message"] = "Error: Dữ liệu không đúng";
                        return View(model);
                    }

                    var media = await _context.Medias.FindAsync(guidId);
                    if (media == null)
                    {
                        TempData["Message"] = "Error: Không tìm thấy hình ảnh";
                        return View(model);
                    }

                    if (avatarId == media.Id.ToString())
                        hotel.Avatar = media.MediaUrl;

                    media.IsAvatar = avatarId == media.Id.ToString();
                    media.IsUsed = true;
                    media.AuthorType = "Hotel";
                    media.AuthorId = hotel.Id;
                    media.UpdateAt = DateTime.UtcNow;

                    _context.Medias.Update(media);
                }

                _context.Hotels.Update(hotel);
                #endregion

                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Cập nhật khách sạn thành công";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx)
                {
                    // 2601: Cannot insert duplicate key row
                    // 2627: Violation of UNIQUE KEY constraint
                    if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                        TempData["Message"] = "Error: Tên khách sạn đã được sử dụng";
                }

                _logger.LogError($"Error: {ex.InnerException}");
                return View(model);
            }
        }

        [HttpDelete("delete")]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new
                    {
                        Success = false,
                        Message = "Id không có dữ liệu"
                    });

                if (!Guid.TryParse(id, out var guidId))
                    return Json(new
                    {
                        Success = false,
                        Message = "Kiểu dữ liệu id không đúng"
                    });

                var hotel = await _context.Hotels.FindAsync(guidId);
                if (hotel == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Không tìm thấy khách sạn"
                    });
                }

                _context.Hotels.Remove(hotel);

                // Xóa tất cả ảnh theo hotel
                var listPublicId = await _context.Medias
                                .Where(m => m.AuthorType == "Hotel"
                                && m.AuthorId == hotel.Id)
                                .Select(m => m.PublicId)
                                .ToListAsync();

                if (listPublicId.Count > 0)
                {
                    // Xóa ảnh trên cloundinary
                    await _cloudinary.DeleteMediaAsync(listPublicId);
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    Success = true,
                    Message = "Success"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        private bool CheckHotelId(Guid id)
        {
            return _context.Hotels.Any(h => h.Id == id);
        }

        private async Task RenderSelectedCateHotel()
        {
            var listCateHotel = await _context.CategoryHotels
                                .Where(ch => ch.IdParent == null)
                                .Include(ch => ch.CateHotelChildren)
                                .ThenInclude(ch => ch.CateHotelChildren)
                                .ToListAsync();

            var items = new List<CategoryHotel>();
            CreateSelectItems(listCateHotel, items, 0);
            var selectList = new SelectList(items, "Id", "Name");

            ViewBag.CateHotel = selectList;
        }

        private async Task RenderViewHotel(HotelDTO hotelDTO, bool newHotel = false)
        {
            hotelDTO.Utilities = await _context.Utilities.ToListAsync();

            if (!newHotel)
            {
                hotelDTO.SelectedUtilityIds = await _context.HotelUtilities
                                            .Where(hu => hu.IdHotel == hotelDTO.Id)
                                            .Select(hu => hu.IdUtility)
                                            .ToListAsync();
            }


            var images = await _context.Medias
                                    .Where(m => m.AuthorType == "Hotel"
                                            && m.AuthorId == hotelDTO.Id)
                                    .ToListAsync();

            ViewBag.Images = images;

            var avatar = await _context.Medias
                                    .Where(m => m.AuthorType == "Hotel"
                                            && m.AuthorId == hotelDTO.Id
                                            && m.IsAvatar)
                                    .Select(m => m.Id)
                                    .FirstOrDefaultAsync();

            hotelDTO.AvatarId = avatar.ToString();
        }
        #endregion

        #region Room
        [HttpGet("room")]
        public async Task<IActionResult> RoomIndex(int page = 1)
        {
            var rooms = await _context.Rooms.ToListAsync();

            List<RoomDTO> listRoomDTO = new List<RoomDTO>();

            foreach (var room in rooms)
            {
                var roomDTO = _mapper.Map<RoomDTO>(room);
                var media = await _context.Medias
                    .FirstOrDefaultAsync(m => m.AuthorType == "Room" && m.AuthorId == room.Id && m.IsAvatar);
                roomDTO.Images = media?.MediaUrl;
                listRoomDTO.Add(roomDTO);
            }

            var total = listRoomDTO.Count();
            var countPage = (int)Math.Ceiling((double)total / ITEM_PER_PAGE);
            page = page > countPage ? countPage : page;

            listRoomDTO = listRoomDTO.Skip((page - 1) * ITEM_PER_PAGE).Take(ITEM_PER_PAGE).ToList();

            ViewBag.CountPage = countPage;

            return View(listRoomDTO);
        }

        [HttpGet("room/details")]
        public async Task<IActionResult> RoomDetails(Guid id)
        {
            if (!_context.Rooms.Any(r => r.Id == id))
            {
                TempData["Message"] = "Error: Dữ liệu không đúng";
                return RedirectToAction(nameof(Index));
            }

            var room = await _context.Rooms.FindAsync(id);

            var hotel = await _context.Hotels.FindAsync(room.IdHotel);
            
            var roomDTO = _mapper.Map<RoomDTO>(room);
            roomDTO.HotelName = hotel?.HotelName;

            var media = await _context.Medias
                .FirstOrDefaultAsync(m => m.AuthorType == "Room" && m.AuthorId == room.Id && m.IsAvatar);
            roomDTO.Images = media?.MediaUrl;
            return View(roomDTO);
        }

        [HttpGet("room/create")]
        public async Task<IActionResult> RoomCreate()
        {
            var hotels = await _context.Hotels.ToListAsync();

            ViewBag.Hotels = hotels;

            return View();
        }

        [HttpPost("room/create")]
        public async Task<IActionResult> RoomCreate(RoomDTO model)
        {
            try
            {
                await RenderViewRoom(model, true);

                if (!ModelState.IsValid)
                {
                    TempData["Message"] = "Error: Dữ liệu không đúng";
                    return View(model);
                }

                if (string.IsNullOrEmpty(model.Images))
                {
                    TempData["Message"] = "Error: Vui lòng chọn ảnh";
                    return View(model);
                }

                #region Room
                var room = new Room
                {
                    Id = Guid.NewGuid(),
                    RoomName = model.RoomName,
                    Style = model.Style,
                    Price = model.Price,
                    Discount = model.Discount,
                    PriceShow = model.PriceShow,
                    Quantity = model.Quantity,
                    Area = model.Area,
                    AmountPeople = model.AmountPeople,
                    Direction = model.Direction,
                    Bed = model.Bed,
                    BedMore = model.BedMore,
                    IdHotel = model.IdHotel,
                    PercentDeposit = model.PercentDeposit
                };
                #endregion

                #region Image
                var listIdImage = model.Images.Split(";").ToList();

                foreach (var id in listIdImage)
                {
                    if (!Guid.TryParse(id, out var guidId)) // Convert string to Guid
                    {
                        TempData["Message"] = "Error: Dữ liệu không đúng";
                        return View(model);
                    }
                    var media = await _context.Medias.FindAsync(guidId);
                    if (media == null)
                    {
                        TempData["Message"] = "Error: Không tìm thấy hình ảnh";
                        return View(model);
                    }

                    if (model.AvatarId == media.Id.ToString())
                        room.Avatar = media.MediaUrl;

                    media.IsAvatar = model.AvatarId == media.Id.ToString();
                    media.IsUsed = true;
                    media.AuthorType = "Room";
                    media.AuthorId = room.Id;

                    _context.Medias.Update(media);
                }

                _context.Rooms.Add(room);
                #endregion

                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Tạo phòng thành công";

                return RedirectToAction(nameof(RoomIndex));
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx)
                {
                    // 2601: Cannot insert duplicate key row
                    // 2627: Violation of UNIQUE KEY constraint
                    if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                        TempData["Message"] = "Error: Tên phòng đã được sử dụng";
                }

                _logger.LogError($"Error: {ex.InnerException}");
                return View(model);
            }
        }

        [HttpGet("room/edit")]
        public async Task<IActionResult> RoomEdit(Guid id)
        {
            if (!_context.Rooms.Any(r => r.Id == id))
            {
                TempData["Message"] = "Error: Dữ liệu không đúng";
                return RedirectToAction(nameof(RoomIndex));
            }

            var room = await _context.Rooms.FindAsync(id);
            var roomDTO = _mapper.Map<RoomDTO>(room);

            await RenderViewRoom(roomDTO);

            return View(roomDTO);
        }

        [HttpPost("room/edit")]
        public async Task<IActionResult> RoomEdit(RoomDTO model)
        {
            try
            {
                await RenderViewRoom(model);

                if (!ModelState.IsValid)
                {
                    TempData["Message"] = "Error: Dữ liệu không đúng";
                    return View(model);
                }

                if (string.IsNullOrEmpty(model.Images))
                {
                    TempData["Message"] = "Error: Vui lòng chọn ảnh";
                    return View(model);
                }

                #region Room
                var room = await _context.Rooms.FindAsync(model.Id);
                if (room == null)
                {
                    TempData["Message"] = "Error: Không tìm thấy phòng";
                    return View(model);
                }

                room.RoomName = model.RoomName;
                room.Style = model.Style;
                room.Price = model.Price;
                room.Discount = model.Discount;
                room.PriceShow = model.PriceShow;
                room.Area = model.Area;
                room.Quantity = model.Quantity;
                room.AmountPeople = model.AmountPeople;
                room.Direction = model.Direction;
                room.Bed = model.Bed;
                room.BedMore = model.BedMore;
                room.IdHotel = model.IdHotel;
                room.PercentDeposit = model.PercentDeposit;
                room.UpdateAt = DateTime.UtcNow;
                #endregion

                #region Image
                var listIdImage = model.Images.Split(";").ToList();

                foreach (var id in listIdImage)
                {
                    if (!Guid.TryParse(id, out var guidId)) // Convert string to Guid
                    {
                        TempData["Message"] = "Error: Dữ liệu không đúng";
                        return View(model);
                    }
                    var media = await _context.Medias.FindAsync(guidId);
                    if (media == null)
                    {
                        TempData["Message"] = "Error: Không tìm thấy hình ảnh";
                        return View(model);
                    }

                    if (model.AvatarId == media.Id.ToString())
                        room.Avatar = media.MediaUrl;

                    media.IsAvatar = model.AvatarId == media.Id.ToString();
                    media.IsUsed = true;
                    media.AuthorType = "Room";
                    media.AuthorId = room.Id;
                    media.UpdateAt = DateTime.UtcNow;

                    _context.Medias.Update(media);
                }

                _context.Rooms.Update(room);
                #endregion

                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Sửa phòng thành công";

                return RedirectToAction(nameof(RoomIndex));
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx)
                {
                    // 2601: Cannot insert duplicate key row
                    // 2627: Violation of UNIQUE KEY constraint
                    if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                        TempData["Message"] = "Error: Tên phòng đã được sử dụng";
                }

                _logger.LogError($"Error: {ex.InnerException}");
                return View(model);
            }
        }

        [HttpDelete("room/delete")]
        public async Task<IActionResult> RoomDelete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new
                    {
                        Success = false,
                        Message = "Id không có dữ liệu"
                    });

                if (!Guid.TryParse(id, out var guidId))
                    return Json(new
                    {
                        Success = false,
                        Message = "Kiểu dữ liệu id không đúng"
                    });

                var room = await _context.Rooms.FindAsync(guidId);
                if (room == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Không tìm thấy khách sạn"
                    });
                }

                _context.Rooms.Remove(room);

                // Xóa tất cả ảnh theo room
                var listPublicId = await _context.Medias
                                .Where(m => m.AuthorType == "Room"
                                && m.AuthorId == room.Id)
                                .Select(m => m.PublicId)
                                .ToListAsync();

                if (listPublicId.Count > 0)
                {
                    // Xóa ảnh trên cloundinary
                    await _cloudinary.DeleteMediaAsync(listPublicId);
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    Success = true,
                    Message = "Success"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        private async Task RenderViewRoom(RoomDTO model, bool newHotel = false)
        {

            var hotels = await _context.Hotels.ToListAsync();

            if (!newHotel)
            {
                var images = await _context.Medias
                                .Where(m => m.AuthorType == "Room" && m.AuthorId == model.Id)
                                .ToListAsync();

                ViewBag.Images = images;
            }

            ViewBag.Hotels = hotels;
        }
        #endregion

        [HttpPost("upload-media")]
        public async Task<IActionResult> UploadMedia(List<IFormFile> files)
        {
            var result = await _cloudinary.UploadMediaAsync(files);

            return result;
        }

        [HttpDelete("delete-media")]
        public async Task<IActionResult> DeleteMediaById(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Json(new
                {
                    Success = false,
                    Message = "Id không có dữ liệu"
                });

            if (!Guid.TryParse(id, out var guidId))
                return Json(new
                {
                    Success = false,
                    Message = "Kiểu dữ liệu id không đúng"
                });
            var media = await _context.Medias.FindAsync(guidId);

            if (media == null)
                return Json(new
                {
                    Success = false,
                    Message = "Không tìm thấy hình ảnh"
                });

            if (media.IsAvatar)
                return Json(new
                {
                    Success = false,
                    Message = "Ảnh đại diện chưa được lưu"
                });

            List<string> publicIds = new List<string>();
            publicIds.Add(media.PublicId);

            var result = await _cloudinary.DeleteMediaAsync(publicIds);

            return Json(new
            {
                Success = true,
                Message = "Success"
            });
        }

        private void CreateSelectItems(List<CategoryHotel> source, List<CategoryHotel> des, int level)
        {
            string prefix = string.Concat(Enumerable.Repeat("--", level));
            foreach (var category in source)
            {
                // category.Title = prefix + " " + category.Title;
                des.Add(new CategoryHotel()
                {
                    Id = category.Id,
                    Name = prefix + " " + category.Name
                });
                if (category.CateHotelChildren?.Count > 0)
                {
                    CreateSelectItems(category.CateHotelChildren.ToList(), des, level + 1);
                }
            }
        }
    }
}

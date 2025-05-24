using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Booking.Areas.Admin.Models.Tour;
using Booking.EF;
using AutoMapper;
using Microsoft.Data.SqlClient;
using Booking.Utilities;
using Booking.Areas.Admin.Models.Hotel;
using Booking.Controllers;
using Newtonsoft.Json.Linq;
using Booking.Models;
using Microsoft.AspNetCore.Authorization;

namespace Booking.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("/admin/tour/")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.ServiceProvider)]
    public class TourController : Controller
    {
        private readonly ILogger<TourController> _logger;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;
        private readonly CloudinaryController _cloudinary;

        private readonly int ITEM_PER_PAGE = 10;

        public TourController
            (
                ILogger<TourController> logger,
                IMapper mapper,
                AppDbContext context,
                CloudinaryController cloudinary
            )
        {
            _logger = logger;
            _mapper = mapper;
            _context = context;
            _cloudinary = cloudinary;
        }

        #region CategoryTour
        [HttpGet("cate-tour/")]
        public async Task<IActionResult> CateTourIndex()
        {
            var cateTour = await _context.CategoryTours
                                .Where(ct => ct.IdParent == null)
                                .Include(ct => ct.CateTourChildren)
                                .ThenInclude(ct => ct.CateTourChildren)
                                .ToListAsync();

            return View(cateTour);
        }

        [HttpGet("cate-tour/create")]
        public async Task<IActionResult> CateTourCreate()
        {
            await RenderViewTour();

            return View();
        }

        [HttpPost("cate-tour/create")]
        public async Task<IActionResult> CateTourCreate(CategoryTourDTO model)
        {
            try
            {
                await RenderViewTour();

                if (ModelState.IsValid)
                {
                    var cateTour = new CategoryTour
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

                        media.AuthorType = "CategoryTour";
                        media.AuthorId = cateTour.Id;
                        media.IsUsed = true;
                        media.IsAvatar = true;
                        media.UpdateAt = DateTime.UtcNow;

                        _context.Medias.Update(media);

                        cateTour.Avatar = media.MediaUrl;
                    }

                    _context.CategoryTours.Add(cateTour);

                    await _context.SaveChangesAsync();

                    TempData["Message"] = "Success: Thêm thành công";
                    return RedirectToAction(nameof(CateTourIndex));
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

        [HttpDelete("cate-tour/delete")]
        public async Task<IActionResult> CateTourDelete(string id)
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

                var cateTour = await _context.CategoryTours.FindAsync(guidId);
                if (cateTour == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Không tìm thấy danh mục tour"
                    });
                }

                _context.CategoryTours.Remove(cateTour);

                // Xóa tất cả ảnh theo cate tour
                var listPublicId = await _context.Medias
                                .Where(m => m.AuthorType == "CategoryTour"
                                && m.AuthorId == cateTour.Id)
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

        #region Tour
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            var tours = await _context.Tours.ToListAsync();

            List<TourDTO> listTourDTO = new List<TourDTO>();

            foreach (var tour in tours)
            {
                var tourDTO = _mapper.Map<TourDTO>(tour);
                var media = await _context.Medias
                    .FirstOrDefaultAsync(m => m.AuthorType == "Tour" && m.AuthorId == tour.Id && m.IsAvatar);
                tourDTO.Images = media?.MediaUrl;
                listTourDTO.Add(tourDTO);
            }

            // Xóa travel schedule chưa có id tour và quá thời quan tạo 30m
            var oneTimeAgo = DateTime.UtcNow.AddMinutes(-30);
            var travelSche = await _context.TravelSchedules
                                    .Where(ts => ts.IdTour == null
                                    && ts.CreateAt < oneTimeAgo)
                                    .ToListAsync();

            var taskTravel = travelSche.Select(async travel =>
            {
                await DeleteTravelSchedule(travel.Id.ToString());
            });

            await Task.WhenAll(taskTravel);

            var total = listTourDTO.Count();
            var countPage = (int)Math.Ceiling((double)total / ITEM_PER_PAGE);
            page = page > countPage ? countPage : page;

            listTourDTO = listTourDTO.Skip((page - 1) * ITEM_PER_PAGE).Take(ITEM_PER_PAGE).ToList();

            ViewBag.CountPage = countPage;


            return View(listTourDTO);
        }

        [HttpGet("details")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tour = await _context.Tours
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tour == null)
            {
                return NotFound();
            }

            var tourDTO = _mapper.Map<TourDTO>(tour);

            return View(tourDTO);
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            await RenderViewTour();

            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TourDTO model)
        {
            try
            {
                await RenderViewTour();

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

                if (string.IsNullOrEmpty(model.TravelSchedule))
                {
                    TempData["Message"] = "Error: Vui lòng thêm lịch trình tour";
                    return View(model);
                }

                #region Tour
                var tour = new Tour
                {
                    Id = Guid.NewGuid(),
                    TourName = model.TourName,
                    Duration = model.Duration,
                    Departure = model.Departure,
                    Description = model.Description,
                    Sightseeing = model.Sightseeing,
                    Vehicle = model.Vehicle,
                    Slug = AppUtilities.GenerateSlug(model.TourName),
                    Tag = model.Tag,
                    Price = model.Price,
                    Discount = model.Discount,
                    Destination = model.Destination,
                    Overview = model.Overview,
                    TravelSchedule = model.TravelSchedule,
                    ServiceInclude = model.ServiceInclude,
                    ServiceNotInclude = model.ServiceNotInclude,
                    PercentDeposit = model.PercentDeposit,
                    IdCateTour = model.IdCateTour
                };
                #endregion

                #region Image
                var listIdImage = model.Images?.Split(";").ToList() ?? new List<string>();
                foreach (var id in listIdImage)
                {
                    if (!Guid.TryParse(id, out var guidId))
                    {
                        TempData["Message"] = "Error: Kiểu dữ liệu ảnh không đúng";
                        return View(model);
                    }
                    var media = await _context.Medias.FindAsync(guidId);
                    if (media == null)
                    {
                        TempData["Message"] = "Error: Không tìm thấy hình ảnh";
                        return View(model);
                    }

                    if (model.AvatarId == media.Id.ToString())
                        tour.Avatar = media.MediaUrl;

                    media.IsAvatar = model.AvatarId == media.Id.ToString();
                    media.IsUsed = true;
                    media.AuthorType = "Tour";
                    media.AuthorId = tour.Id;
                    media.UpdateAt = DateTime.UtcNow;

                    _context.Medias.Update(media);
                }
                _context.Tours.Add(tour);
                #endregion

                #region TravelSchedule
                var listTravelSche = model.TravelSchedule.Split(";").ToList();

                foreach (var id in listTravelSche)
                {
                    if (!Guid.TryParse(id, out var guidId))
                    {
                        TempData["Message"] = "Error: Kiểu dữ liệu lịch trình không đúng";
                        return View(model);
                    }
                    var travelSche = await _context.TravelSchedules.FindAsync(guidId);
                    if (travelSche == null)
                    {
                        TempData["Message"] = "Error: Không tìm thấy lịch trình";
                        return View(model);
                    }

                    travelSche.IdTour = tour.Id;
                    travelSche.UpdateAt = DateTime.UtcNow;

                    _context.TravelSchedules.Update(travelSche);
                }
                #endregion

                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Thêm mới tour thành công";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx)
                {
                    if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                        TempData["Message"] = "Error: Tên khách sạn đã được sử dụng";
                }

                _logger.LogError("Error: " + ex.Message);

                return View(model);
            }
        }

        [HttpGet("edit")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null)
            {
                TempData["Message"] = "Error: Không tìm thấy tour";
                return RedirectToAction(nameof(Index));
            }

            var tourDTO = _mapper.Map<TourDTO>(tour);

            await RenderViewTour();

            var images = await _context.Medias
                                    .Where(m => m.AuthorType == "Tour"
                                            && m.AuthorId == tourDTO.Id)
                                    .ToListAsync();

            ViewBag.Images = images;

            var avatar = await _context.Medias
                                    .Where(m => m.AuthorType == "Tour"
                                            && m.AuthorId == tourDTO.Id
                                            && m.IsAvatar)
                                    .Select(m => m.Id)
                                    .FirstOrDefaultAsync();

            tourDTO.AvatarId = avatar.ToString();

            var listIdTravelSche = tour.TravelSchedule.Split(";");

            tourDTO.TravelSchedule = string.Empty;

            foreach (var idString in listIdTravelSche)
            {
                if (!Guid.TryParse(idString, out var guidId))
                    break;
                var travelSche = await _context.TravelSchedules.FindAsync(guidId);
                tourDTO.TravelSchedule += travelSche?.Title + travelSche?.Content;
            }

            return View(tourDTO);
        }

        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TourDTO model)
        {
            await RenderViewTour();
            var images = await _context.Medias.Where(m => m.MediaType == "Tour"
                                                && m.AuthorId == model.Id).ToListAsync();
            ViewBag.Images = images;

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

            try
            {
                #region Tour
                var tour = await _context.Tours.FindAsync(model.Id);
                if (tour == null)
                {
                    TempData["Message"] = "Error: Không tìm thấy tour";
                    return View(model);
                }

                tour.TourName = model.TourName;
                tour.Duration = model.Duration;
                tour.Departure = model.Departure;
                tour.Destination = model.Destination;
                tour.Sightseeing = model.Sightseeing;
                tour.Vehicle = model.Vehicle;
                tour.Slug = AppUtilities.GenerateSlug(model.TourName);
                tour.Tag = model.Tag;
                tour.Price = model.Price;
                tour.Discount = model.Discount;
                tour.Description = model.Description;
                tour.Overview = model.Overview;
                tour.TravelSchedule = model.TravelSchedule ?? string.Empty;
                tour.ServiceInclude = model.ServiceInclude;
                tour.ServiceNotInclude = model.ServiceNotInclude;
                tour.PercentDeposit = model.PercentDeposit;
                tour.IdCateTour = model.IdCateTour;
                tour.UpdateAt = DateTime.UtcNow;
                #endregion

                #region Image
                var listIdImage = model.Images.Split(";").ToList();

                foreach (var id in listIdImage)
                {
                    if (!Guid.TryParse(id, out var guidId))
                    {
                        TempData["Message"] = "Error: Dữ liệu không đúng";
                        return View(model);
                    }
                    var media = await _context.Medias.FindAsync(guidId);
                    if (media == null)
                    {
                        TempData["Message"] = "Error: Không tìm thấy ảnh";
                        return View(model);
                    }

                    if (model.AvatarId == media.Id.ToString())
                        tour.Avatar = media.MediaUrl;

                    media.IsAvatar = model.AvatarId == media.Id.ToString();
                    media.AuthorId = model.Id;
                    media.AuthorType = "Tour";
                    media.IsUsed = true;
                    media.UpdateAt = DateTime.UtcNow;

                    _context.Medias.Update(media);
                }
                _context.Tours.Update(tour);
                #endregion

                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Cập nhật thành công";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx)
                {
                    if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                    {
                        TempData["Message"] = "Error: Tên phòng đã được sử dụng";
                    }
                }

                _logger.LogError("Error: " + ex.Message);
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

                var tour = await _context.Tours.FindAsync(guidId);
                if (tour == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Không tìm thấy khách sạn"
                    });
                }

                _context.Tours.Remove(tour);

                // Xóa tất cả ảnh theo tour
                var listPublicId = await _context.Medias
                                .Where(m => m.AuthorType == "Tour"
                                && m.AuthorId == tour.Id)
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

        private bool TourDTOExists(Guid id)
        {
            return _context.Tours.Any(e => e.Id == id);
        }

        private void CreateSelectItems(List<CategoryTour> source, List<CategoryTour> des, int level)
        {
            string prefix = string.Concat(Enumerable.Repeat("--", level));
            foreach (var category in source)
            {
                // category.Title = prefix + " " + category.Title;
                des.Add(new CategoryTour()
                {
                    Id = category.Id,
                    Name = prefix + " " + category.Name
                });
                if (category.CateTourChildren?.Count > 0)
                {
                    CreateSelectItems(category.CateTourChildren.ToList(), des, level + 1);
                }
            }
        }

        private async Task RenderViewTour()
        {
            var listCateTour = await _context.CategoryTours
                                .Where(ct => ct.IdParent == null)
                                .Include(ct => ct.CateTourChildren)
                                .ThenInclude(ct => ct.CateTourChildren)
                                .ToListAsync();

            var items = new List<CategoryTour>();
            CreateSelectItems(listCateTour, items, 0);
            var selectList = new SelectList(items, "Id", "Name");

            ViewBag.CateTour = selectList;
        }
        #endregion

        [HttpPost("/travel-schedule/create")]
        public async Task<IActionResult> CreateTravelSchedule(TravelScheduleDTO model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new
                    {
                        Success = false,
                        Message = "Dữ liệu không đúng"
                    });

                var travelSchedule = new TravelSchedule
                {
                    Id = Guid.NewGuid(),
                    Title = model.Title,
                    Content = model.Content,
                };

                _context.TravelSchedules.Add(travelSchedule);

                var listIdImage = model.Images?.Split(";").ToList() ?? new List<string>();
                foreach (var id in listIdImage)
                {
                    if (!Guid.TryParse(id, out var guidId))
                    {
                        TempData["Message"] = "Error: Kiểu dữ liệu ảnh không đúng";
                        return View(model);
                    }
                    var media = await _context.Medias.FindAsync(guidId);
                    if (media == null)
                    {
                        TempData["Message"] = "Error: Không tìm thấy hình ảnh";
                        return View(model);
                    }

                    media.IsUsed = true;
                    media.AuthorType = "TravelSchedule";
                    media.AuthorId = travelSchedule.Id;
                    media.UpdateAt = DateTime.UtcNow;

                    _context.Medias.Update(media);
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    Success = true,
                    Message = "Success",
                    Data = new
                    {
                        Id = travelSchedule.Id,
                        Title = travelSchedule.Title
                    }
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

        [HttpDelete("/travel-schedule/delete")]
        public async Task<IActionResult> DeleteTravelSchedule(string id)
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

                var travelSche = await _context.TravelSchedules.FindAsync(guidId);
                if (travelSche == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Không tìm thấy khách sạn"
                    });
                }

                _context.TravelSchedules.Remove(travelSche);

                // Xóa tất cả ảnh theo travel schedule
                var listPublicId = await _context.Medias
                                .Where(m => m.AuthorType == "TravelSchedule"
                                && m.AuthorId == travelSche.Id)
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

        [HttpPost("froala/upload")]
        public async Task<IActionResult> FroalaUpload()
        {
            try
            {
                var files = Request.Form.Files.ToList();
                if (files.Count <= 0)
                    return Json(new
                    {
                        Success = false,
                        Message = "Không tìm thấy ảnh",
                    });

                var result = await _cloudinary.UploadMediaAsync(files) as JsonResult;
                var data = result?.Value as IEnumerable<dynamic>;
                var link = string.Empty;
                var id = string.Empty;
                foreach (var item in data)
                {
                    id = item.Id.ToString();
                    link = item.Url;
                }

                return Json(new
                {
                    Success = true,
                    Message = "Success",
                    Id = id,
                    Link = link
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = ex.Message,
                });
            }
        }
    }
}

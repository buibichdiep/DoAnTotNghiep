using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using Booking.Areas.Admin.Models.Flight;
using Booking.EF;
using Booking.Models;
using Booking.Models.Hotel;
using Booking.Models.Tour;
using CloudinaryDotNet.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Booking.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private CloudinaryController _cloudinary;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _conf;
        private readonly PaymentController _payment;

        public HomeController
            (
                ILogger<HomeController> logger,
                AppDbContext context,
                UserManager<User> userManager,
                CloudinaryController cloudinary,
                IHttpClientFactory httpClientFactory,
                IConfiguration conf,
                PaymentController payment
            )
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _cloudinary = cloudinary;
            _httpClientFactory = httpClientFactory;
            _conf = conf;
            _payment = payment;
        }

        public async Task<IActionResult> Index()
        {
            var codeIATA = await _context.CityCodes
                            .Include(c => c.CityCodeIATAChildren)
                            .Where(c => c.IdParent == null)
                            .ToListAsync();

            ViewBag.CodeIATA = codeIATA;

            var listCateHotel = await _context.CategoryHotels.Include(ch => ch.Hotels).ToListAsync();
            ViewBag.ListCateHotel = listCateHotel;

            var listCateTour = await _context.CategoryTours
                                    .Include(ct => ct.CateTourChildren)
                                    .Include(ct => ct.Tours)
                                    .ToListAsync();


            ViewBag.ListCateTour = listCateTour;

            await RemoveMediaIsNotUsed();

            return View();
        }

        #region Flight
        [HttpGet("/ve-may-bay")]
        public async Task<IActionResult> Flight()
        {
            var codeIATA = await _context.CityCodes
                            .Include(c => c.CityCodeIATAChildren)
                            .Where(c => c.IdParent == null)
                            .ToListAsync();

            ViewBag.CodeIATA = codeIATA;

            return View();
        }

        [HttpGet("/ve-may-bay/tim-kiem-ve")]
        public async Task<IActionResult> FlightSearch(FlightDTO model)
        {
            var listFlightOutbound = await GetInfoFlight(model);

            if (listFlightOutbound.Count <= 0)
            {
                TempData["Message"] = "Error: Không có chuyến bay nào";
                return RedirectToAction(nameof(Flight));
            }

            List<FlightVM> listFlightReturn = new List<FlightVM>();

            if (model.ReturnDate != null)
            {
                var temp = model.From;
                model.From = model.To;
                model.To = temp;
                model.DepartDate = model.ReturnDate;
                listFlightReturn = await GetInfoFlight(model);

                if (listFlightReturn.Count <= 0)
                {
                    TempData["Message"] = "Error: Không có chuyến bay khứ hồi";
                    return RedirectToAction(nameof(Flight));
                }
            }

            var flightInfo = new FlightInfo
            {
                OutboundFlight = listFlightOutbound,
                ReturnFlight = listFlightReturn
            };

            return View(flightInfo);
        }

        [HttpGet("/ve-may-bay/order")]
        public IActionResult FlightOrder(int ADT, int CHD, int INF)
        {
            var sessionOutboundFlight = HttpContext.Session.GetString("OutboundFlight");
            var sessionReturnFlight = HttpContext.Session.GetString("ReturnFlight");

            if (sessionOutboundFlight == null || sessionReturnFlight == null)
            {
                TempData["Message"] = "Error: Không tìm thấy thông tin chuyến bay";
                return RedirectToAction(nameof(Flight));
            }

            var outboundFlight = JsonConvert.DeserializeObject<FlightVM>(sessionOutboundFlight);
            var returnFlight = JsonConvert.DeserializeObject<FlightVM>(sessionReturnFlight);

            ViewBag.OutboundFlight = outboundFlight;
            ViewBag.ReturnFlight = returnFlight;

            return View();
        }

        [HttpPost("/ve-may-bay/order")]
        public async Task<IActionResult> FlightOrder(string id, FlightOrder model)
        {
            try
            {
                decimal price = model.Price;
                decimal tax = model.Tax;
                decimal totalPrice = model.TotalPrice;
                DateTime departureDate = DateTime.ParseExact(model.DepartureTime, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime destinationDate = DateTime.ParseExact(model.ArrivalTime, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                var user = await _userManager.FindByEmailAsync(model.Email);

                var bill = new Bill
                {
                    Id = Guid.NewGuid(),
                    NumberAdult = model.NumberAdult,
                    NumberChildren = model.NumberChildren,
                    NumberInfant = model.NumberInfant,
                    UserName = model.UserName,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    ContentRequest = model.ContentRequest,
                    ServiceType = "Flight",
                    ServiceName = $"Đặt vé máy bay từ {model.Departure} - {model.Arrival}",
                    Price = price,
                    Tax = tax,
                    TotalPrice = totalPrice,
                    StatusBill = "Chờ thanh toán",
                    CheckInTime = departureDate,
                    CheckOutTime = destinationDate,
                    ServiceId = new Guid(),
                    IsActive = false
                };

                if (user is not null) bill.UserId = Guid.Parse(user.Id);

                _context.Bills.Add(bill);
                await _context.SaveChangesAsync();

                var createLink = await _payment.CreateLinkPayment(bill.Id);
                dynamic result = createLink.Value ?? new ExpandoObject();

                return Json(new
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data
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

        [HttpPost("/ve-may-bay/save-session")]
        public JsonResult SaveFlightSession([FromBody] FlightVM model, string field)
        {
            if (model == null)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Error: Không tìm thấy chuyến bay"
                });
            }
            if (field == "OutboundFlight")
            {
                HttpContext.Session.SetString("OutboundFlight", JsonConvert.SerializeObject(model));
            }
            else
            {
                HttpContext.Session.SetString("ReturnFlight", JsonConvert.SerializeObject(model));
            }

            return Json(new
            {
                Success = true,
                Message = "Success",
                Data = model.Id
            });
        }

        private async Task<List<FlightVM>> GetInfoFlight(FlightDTO model)
        {
            var client = _httpClientFactory.CreateClient();

            var departureFull = _context.CityCodes.FirstOrDefault(c => c.CodeIATA == model.From);
            var arrivalFull = _context.CityCodes.FirstOrDefault(c => c.CodeIATA == model.To);
            DateTime dateOutbound = DateTime.ParseExact(model.DepartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            string outboundDate = dateOutbound.ToString("yyyy-MM-dd");
            var numberChildren = model.CHD is null ? "" : $"&children={model.CHD}";
            var numberInfants = model.INF is null ? "" : $"&infants_in_seat={model.INF}";
            var apiKey = _conf["SerpApi:ApiKey"] ?? string.Empty;

            string urlApi = "https://serpapi.com/search.json?engine=google_flights" +
                $"&departure_id={model.From}&arrival_id={model.To}&gl=vn&hl=vi&currency=VND&type=2" +
                $"&outbound_date={outboundDate}" +
                $"&travel_class={model.TravelClass}" +
                $"&adults={model.ADT}{numberChildren}{numberInfants}" +
                $"&api_key={apiKey}";

            var listFlight = new List<FlightVM>();

            HttpResponseMessage response = await client.GetAsync(urlApi);
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                // Deserialize JSON thành JObject
                var jsonObject = JObject.Parse(jsonData);

                if (jsonObject["error"] != null)
                {
                    TempData["Message"] = "Error: Không tìm thấy chuyến bay";
                    return listFlight;
                }

                // Lấy phần `best_flights`
                var bestFlights = jsonObject["best_flights"];

                foreach (var flight in bestFlights ?? new JArray())
                {
                    decimal price = Math.Round(decimal.Parse(flight["price"]?.ToString() ?? "0") / 1000) * 1000;
                    decimal tax = Math.Round(price * 50 / 100 / 1000) * 1000;
                    decimal totalPrice = price + tax;

                    var flightData = new FlightVM
                    {
                        Id = Guid.NewGuid(),
                        Airplane = flight["flights"]?[0]?["airplane"]?.ToString() ?? string.Empty,
                        Airline = flight["flights"]?[0]?["airline"]?.ToString() ?? string.Empty,
                        Departure = $"{departureFull?.CityName} ({departureFull?.CodeIATA})",
                        Arrival = $"{arrivalFull?.CityName} ({arrivalFull?.CodeIATA})",
                        Price = price,
                        Tax = tax,
                        TotalPrice = totalPrice,
                        DepartureTime = ConvertModel.ConvertStringToDatetime(flight["flights"]?[0]?["departure_airport"]?["time"]?.ToString() ?? string.Empty),
                        ArrivalTime = ConvertModel.ConvertStringToDatetime(flight["flights"]?[0]?["arrival_airport"]?["time"]?.ToString() ?? string.Empty),
                        FlightNumber = flight["flights"]?[0]?["flight_number"]?.ToString() ?? string.Empty,
                        FlightLogo = flight["airline_logo"]?.ToString() ?? string.Empty
                    };

                    listFlight.Add(flightData);
                }

                return listFlight;
            }
            else
            {
                TempData["Message"] = "Error: Lỗi khi tìm chuyến bay";
                return listFlight;
            }
        }
        #endregion

        #region Tour
        [HttpGet("/tour")]
        public async Task<IActionResult> Tour()
        {
            var listCateTour = await _context.CategoryTours
                                    .Include(ct => ct.CateTourChildren)
                                    .Include(ct => ct.Tours)
                                    .ToListAsync();

            var listTour = await _context.Tours.Take(3).ToListAsync();

            ViewBag.ListCateTour = listCateTour;
            ViewBag.ListTour = listTour;

            return View();
        }

        [HttpGet("/tour/search")]
        public async Task<IActionResult> TourFilter(string slug = "", string departure = "")
        {
            var cateTour = await _context.CategoryTours
                            .Include(ct => ct.CateTourChildren)
                            .ThenInclude(ct => ct.CateTourChildren)
                            .FirstOrDefaultAsync(ct => ct.Slug == slug);

            if (cateTour == null)
            {
                TempData["Message"] = "Error: Không tìm thấy tour";
                return RedirectToAction(nameof(Tour));
            }

            // Lấy tất cả tour theo category
            List<Tour> tours = await GetTourByCategory(cateTour);

            if (!string.IsNullOrEmpty(departure))
            {
                tours = tours.Where(t => t.Departure == departure).ToList();
            }
            //tours = await _context.Tours.Where(t => t.Departure == departure && t.Slug == slug).ToListAsync();

            return View(tours);
        }

        [HttpPost("/tour/handle")]
        public async Task<JsonResult> TourFilterHandle(string filter)
        {
            var listTour = await _context.Tours.ToListAsync();
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            var json = JsonConvert.SerializeObject(listTour, jsonSettings);
            if (filter == "null")
                return Json(new
                {
                    Success = true,
                    Message = "Success",
                    Data = json
                });

            var parts = filter.Split(';')
                         .Select(p => p.Split(':'))
                         .Where(p => p.Length == 2);

            var grouped = parts.GroupBy(p => p[0])
                               .ToDictionary(g => g.Key, g => g.Select(x => x[1]));

            var result = string.Join(";", grouped.Select(g => $"{g.Key}:{string.Join("&", g.Value)}"));

            var listFilter = result.Split(";");

            // Lấy slug từ mảng đầu tiên
            var slug = listFilter.First().Split(":").Last();
            var cateTour = await _context.CategoryTours
                            .Include(ct => ct.CateTourChildren)
                            .ThenInclude(ct => ct.CateTourChildren)
                            .FirstOrDefaultAsync(ct => ct.Slug == slug);

            // Nếu có slug thì lấy danh sách tour theo id cate
            if (cateTour != null)
            {
                listTour = await GetTourByCategory(cateTour);
            }

            var listTourFilter = listTour;

            foreach (var itemFilter in listFilter)
            {
                var item = itemFilter.Split(":");
                string value = "&" + item.Last() + "&";
                if (item.First() == "Departure")
                {
                    listTourFilter = listTourFilter.Where(h => value.Contains("&" + h.Departure + "&")).ToList();
                    //listTourFilter.AddRange(tours);
                }
                if (item.First() == "Duration")
                {
                    listTourFilter = listTourFilter.Where(h => value.Contains("&" + h.Duration + "&")).ToList();
                    //listTourFilter.AddRange(tours);
                }
                if (item.First() == "Price")
                {
                    var listResult = new List<Tour>();
                    if (value.Contains("&0&"))
                    {
                        var lstFilter = listTourFilter.Where(h => h.Price >= 1000000 && h.Price < 5000000).ToList();
                        listResult.AddRange(lstFilter);
                    }
                    if (value.Contains("&1&"))
                    {
                        var lstFilter = listTourFilter.Where(h => h.Price >= 5000000 && h.Price < 10000000).ToList();
                        listResult.AddRange(lstFilter);
                    }
                    if (value.Contains("&2&"))
                    {
                        var lstFilter = listTourFilter.Where(h => h.Price >= 10000000 && h.Price < 15000000).ToList();
                        listResult.AddRange(lstFilter);
                    }
                    if (value.Contains("&3&"))
                    {
                        var lstFilter = listTourFilter.Where(h => h.Price >= 15000000).ToList();
                        listResult.AddRange(lstFilter);
                    }

                    listTourFilter = listResult;
                }
            }

            // Nếu chỉ truyền mỗi slug thì lấy tất cả tour
            if (listFilter.Length == 1)
            {
                listTourFilter.AddRange(listTour);
            }

            listTourFilter = listTourFilter.Distinct().ToList();

            var utilities = await _context.Utilities.ToListAsync();
            ViewBag.Utilities = utilities;

            json = JsonConvert.SerializeObject(listTourFilter, jsonSettings);

            return Json(new
            {
                Success = true,
                Message = "Success",
                Data = json
            });
        }

        [HttpGet("/tour/{slug}")]
        public async Task<IActionResult> TourDetails(string slug)
        {
            if (slug == null)
            {
                TempData["Message"] = "Error: Không tìm thấy tour";
                return RedirectToAction(nameof(Tour));
            }
            var tour = await _context.Tours.FirstOrDefaultAsync(t => t.Slug == slug);
            if (tour == null)
            {
                TempData["Message"] = "Error: Không tìm thấy tour";
                return RedirectToAction(nameof(Tour));
            }

            var images = await _context.Medias
                                    .Where(m => m.AuthorType == "Tour"
                                    && m.AuthorId == tour.Id)
                                    .OrderByDescending(m => m.IsAvatar)
                                    .ToListAsync();

            ViewBag.Images = images;

            var travelSchedule = await _context.TravelSchedules
                                    .Where(ts => ts.IdTour == tour.Id)
                                    .OrderBy(ts => ts.Title)
                                    .ToListAsync();

            ViewBag.TravelSchedule = travelSchedule;

            var tourSimilar = await _context.Tours
                                    .Where(t => t.IdCateTour == tour.IdCateTour
                                    && t.Id != tour.Id)
                                    .Take(4)
                                    .ToListAsync();
            ViewBag.TourSimilar = tourSimilar;

            return View(tour);
        }

        [HttpGet("/tour/order/{slug}")]
        public async Task<IActionResult> TourOrder(string slug, string? returnUrl)
        {
            returnUrl ??= Url.Content(nameof(Tour));

            var tour = await _context.Tours.FirstOrDefaultAsync(t => t.Slug == slug);
            if (tour is null)
            {
                TempData["Message"] = "Không tìm thấy tour";
                return Redirect(returnUrl);
            }

            return View(tour);
        }

        [HttpPost("/tour/order/{slug}")]
        public async Task<IActionResult> TourOrder(string slug, TourOrder model)
        {
            try
            {
                var tour = await _context.Tours.FirstOrDefaultAsync(t => t.Slug == slug);
                if (tour is null)
                    return Json(new
                    {
                        Success = false,
                        Message = "Không tìm thấy tour"
                    });

                if (!ModelState.IsValid)
                    return Json(new
                    {
                        Success = false,
                        Message = "Dữ liệu không đúng"
                    });

                var price = tour.Discount ?? tour.Price;
                var priceChildren = Math.Round(price / 100 * 70 / 1000) * 1000;
                var tax = Math.Round(price * 15 / 100 / 1000) * 1000 * model.NumberAdult;
                var totalPrice = price * model.NumberAdult + priceChildren * model.NumberChildren + tax;
                DateTime departureDate = DateTime.ParseExact(model.DepartureDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var countDay = int.Parse(tour.Duration.Substring(0, 1));
                DateTime destinationDate = departureDate.AddDays(countDay);

                if(!string.IsNullOrEmpty(model.PaymentMethod) && model.PaymentMethod == "percent" && tour.PercentDeposit != null && tour.PercentDeposit > 0)
                {
                    totalPrice = Math.Round(totalPrice / 100 * (tour.PercentDeposit ?? 100) / 1000) * 1000;
                }

                var user = await _userManager.FindByEmailAsync(model.Email);

                var bill = new Bill
                {
                    Id = Guid.NewGuid(),
                    NumberAdult = model.NumberAdult,
                    NumberChildren = model.NumberChildren,
                    NumberInfant = model.NumberInfant,
                    UserName = model.UserName,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    ContentRequest = model.ContentRequest,
                    ServiceType = "Tour",
                    ServiceName = tour.TourName,
                    Price = price,
                    Tax = tax,
                    TotalPrice = totalPrice,
                    StatusBill = "Chờ thanh toán",
                    CheckInTime = departureDate,
                    CheckOutTime = destinationDate,
                    ServiceId = tour.Id,
                    IsActive = false
                };

                if (user is not null) bill.UserId = Guid.Parse(user.Id);

                _context.Bills.Add(bill);
                await _context.SaveChangesAsync();

                var createLink = await _payment.CreateLinkPayment(bill.Id);
                dynamic result = createLink.Value ?? new ExpandoObject();

                return Json(new
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data
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

        private async Task<List<Tour>> GetTourByCategory(CategoryTour cateTour)
        {
            List<Tour> tours = new List<Tour>();
            // Kiểm tra xem cate tour có cate con nào không
            if (cateTour.CateTourChildren.Count > 0)
            {
                foreach (var item in cateTour.CateTourChildren)
                {
                    tours.AddRange(await GetTourByCategory(item));
                }
            }
            else
            {
                // Nếu không có cate con thì lấy tour theo cate
                tours = await _context.Tours.Where(t => t.IdCateTour == cateTour.Id).ToListAsync();
            }

            return tours;
        }
        #endregion

        #region Hotel
        [HttpGet("/khach-san/")]
        public async Task<IActionResult> Hotel()
        {
            var listCateHotel = await _context.CategoryHotels.Include(ch => ch.Hotels).ToListAsync();
            ViewBag.ListCateHotel = listCateHotel;

            return View();
        }

        [HttpGet("/khach-san/search")]
        public async Task<IActionResult> HotelFilter(string slug = "")
        {
            var utilities = await _context.Utilities.ToListAsync();
            ViewBag.Utilities = utilities;

            var cateHotel = await _context.CategoryHotels.FirstOrDefaultAsync(ch => ch.Slug == slug);
            List<Hotel> listHotel;
            if (cateHotel == null)
            {
                listHotel = await _context.Hotels.Include(h => h.Rooms).ToListAsync();
            }
            else
            {
                listHotel = await _context.Hotels.Include(h => h.Rooms).Where(h => h.IdCateHotel == cateHotel.Id).ToListAsync();
            }

            listHotel = listHotel.OrderBy(h => h.Rooms.Min(r => r.Price)).ToList();
            return View(listHotel);
        }

        [HttpPost("/khach-san/handle")]
        public async Task<JsonResult> HotelFilterHandle(string filter)
        {
            var listHotel = await _context.Hotels.Include(h => h.Rooms).ToListAsync();
            foreach (var hotel in listHotel)
            {
                hotel.Rooms = hotel.Rooms.OrderBy(r => r.Price).ToList();
            }
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            var json = JsonConvert.SerializeObject(listHotel, jsonSettings);
            if (filter == "null")
                return Json(new
                {
                    Success = true,
                    Message = "Success",
                    Data = json
                });

            var parts = filter.Split(';')
                         .Select(p => p.Split(':'))
                         .Where(p => p.Length == 2);

            var grouped = parts.GroupBy(p => p[0])
                               .ToDictionary(g => g.Key, g => g.Select(x => x[1]));

            var result = string.Join(";", grouped.Select(g => $"{g.Key}:{string.Join("&", g.Value)}"));

            var listFilter = result.Split(";");

            // Lấy slug từ mảng đầu tiên
            var slug = listFilter.First().Split(":").Last();
            var cateTour = await _context.CategoryHotels.FirstOrDefaultAsync(ch => ch.Slug == slug);

            // Nếu có slug thì lấy danh sách hotel theo id cate
            if (cateTour != null)
            {
                listHotel = listHotel.Where(h => h.IdCateHotel == cateTour.Id).ToList();
            }

            var listHotelFilter = listHotel;

            foreach (var itemFilter in listFilter)
            {
                var item = itemFilter.Split(":");
                string value = "&" + item.Last() + "&";
                if (item.First() == "Residence")
                {
                    listHotelFilter = listHotelFilter.Where(h => value.Contains("&" + h.ResidenceType + "&")).ToList();
                    //listHotelFilter.AddRange(hotels);
                }
                if (item.First() == "Star")
                {
                    listHotelFilter = listHotelFilter.Where(h => value.Contains("&" + h.Star + "&")).ToList();
                    //listHotelFilter.AddRange(hotels);
                }
                if (item.First() == "Price")
                {
                    var listResult = new List<Hotel>();
                    if (value.Contains("&0&"))
                    {
                        var lstFilter = listHotelFilter.Where(h => h.Rooms.Any(r => r.Price < 500000)).ToList();
                        listResult.AddRange(lstFilter);
                    }
                    if (value.Contains("&1&"))
                    {
                        var lstFilter = listHotelFilter.Where(h => h.Rooms.Any(r => r.Price >= 500000 && r.Price < 1000000)).ToList();
                        listResult.AddRange(lstFilter);
                    }
                    if (value.Contains("&2&"))
                    {
                        var lstFilter = listHotelFilter.Where(h => h.Rooms.Any(r => r.Price >= 1000000 && r.Price < 2000000)).ToList();
                        listResult.AddRange(lstFilter);
                    }
                    if (value.Contains("&3&"))
                    {
                        var lstFilter = listHotelFilter.Where(h => h.Rooms.Any(r => r.Price >= 2000000 && r.Price < 3000000)).ToList();
                        listResult.AddRange(lstFilter);
                    }
                    if (value.Contains("&4&"))
                    {
                        var lstFilter = listHotelFilter.Where(h => h.Rooms.Any(r => r.Price >= 3000000)).ToList();
                        listResult.AddRange(lstFilter);
                    }

                    listHotelFilter = listResult;
                }
                if (item.First() == "Utility")
                {
                    //if (!Guid.TryParse(item.Last(), out Guid guidId))
                    //    return Json(new
                    //    {
                    //        Success = false,
                    //        Message = "Không tìm thấy tiện ích"
                    //    });

                    listHotelFilter = (from h in listHotelFilter
                                       join hu in _context.HotelUtilities on h.Id equals hu.IdHotel
                                       where value.Contains("&" + hu.IdUtility + "&")
                                       select h).ToList();
                    //listHotelFilter.RemoveAll(h => listHotelFilter.Contains(h));
                    //listHotelFilter.AddRange(hotels);
                }
            }

            // Nếu chỉ truyền mỗi slug thì lấy tất cả hotel
            if (listFilter.Length == 1)
            {
                listHotelFilter.AddRange(listHotel);
            }

            listHotelFilter = listHotelFilter.Distinct().ToList();

            foreach (var item in listHotelFilter)
            {
                item.HotelUtilitys = [];
            }

            var utilities = await _context.Utilities.ToListAsync();
            ViewBag.Utilities = utilities;

            json = JsonConvert.SerializeObject(listHotelFilter, jsonSettings);

            return Json(new
            {
                Success = true,
                Message = "Success",
                Data = json
            });
        }

        [HttpGet("/khach-san/{slug}")]
        public async Task<IActionResult> HotelDetails(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                TempData["Message"] = "Error: Không tìm thấy khách sạn";
                return RedirectToAction(nameof(Hotel));
            }

            var hotel = await _context.Hotels.Include(h => h.Rooms)
                                .FirstOrDefaultAsync(h => h.Slug == slug);

            if (hotel == null)
            {
                TempData["Message"] = "Error: Không tìm thấy khách sạn";
                return RedirectToAction(nameof(Hotel));
            }

            hotel.Rooms = hotel.Rooms.OrderBy(r => r.Price).ToList();


            hotel.Rooms = await FilterEmptyRoom(hotel.Rooms.ToList(), DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 1);
            //ViewBag.Rooms = rooms;

            var images = await _context.Medias
                                .Where(m => m.AuthorType == "Hotel"
                                && m.AuthorId == hotel.Id)
                                .OrderByDescending(m => m.IsAvatar)
                                .ToListAsync();
            ViewBag.Images = images;

            var utilities = await (from u in _context.Utilities
                                   join hu in _context.HotelUtilities on u.Id equals hu.IdUtility
                                   where hu.IdHotel == hotel.Id
                                   select u).ToListAsync();
            ViewBag.Utilities = utilities;

            return View(hotel);
        }

        [HttpGet("/khach-san/order/{id}")]
        public async Task<IActionResult> HotelOrder(string id, HotelOrderDTO model)
        {
            if (!Guid.TryParse(id, out Guid guidId))
            {
                TempData["Message"] = "Mã phòng không hợp lệ";
                return View(nameof(Hotel));
            }

            var room = await _context.Rooms.FindAsync(guidId);
            if (room is null)
            {
                TempData["Message"] = "Không tìm thấy phòng";
                return View(nameof(Hotel));
            }

            var hotel = await _context.Hotels.FindAsync(room.IdHotel);
            if (hotel is null)
            {
                TempData["Message"] = "Không tìm thấy khách sạn";
                return View(nameof(Hotel));
            }

            ViewBag.Hotel = hotel;
            ViewBag.HotelOrder = model;

            return View(room);
        }

        [HttpPost("/khach-san/order/{id}")]
        public async Task<IActionResult> HotelOrder(string id, HotelOrder model)
        {
            try
            {
                if (!Guid.TryParse(id, out Guid guidId))
                    return Json(new
                    {
                        Success = false,
                        Message = "Mã phòng không hợp lệ"
                    });

                var room = await _context.Rooms.FindAsync(guidId);
                if (room is null)
                    return Json(new
                    {
                        Success = false,
                        Message = "Không tìm thấy phòng"
                    });
                var night = Convert.ToInt32(model.CheckOutDate.Substring(0, 2)) - Convert.ToInt32(model.CheckInDate.Substring(0, 2));

                var price = room.Discount ?? room.Price;
                var tax = Math.Round(price * 15 / 100 / 1000) * 1000 * night * model.NumberRoom;
                var totalPrice = price * night * model.NumberRoom + tax;
                DateTime checkInDate = DateTime.ParseExact(model.CheckInDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime checkOutDate = DateTime.ParseExact(model.CheckOutDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                if (!string.IsNullOrEmpty(model.PaymentMethod) && model.PaymentMethod == "percent" && room.PercentDeposit != null && room.PercentDeposit > 0)
                {
                    totalPrice = Math.Round(totalPrice / 100 * (room.PercentDeposit ?? 100) / 1000) * 1000;
                }

                var user = await _userManager.FindByEmailAsync(model.Email);

                var bill = new Bill
                {
                    Id = Guid.NewGuid(),
                    NumberAdult = model.NumberAdult,
                    NumberChildren = model.NumberChildren,
                    NumberInfant = model.NumberInfant,
                    UserName = model.UserName,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    ContentRequest = model.ContentRequest,
                    ServiceType = "Room",
                    ServiceName = room.RoomName,
                    Price = price,
                    Tax = tax,
                    TotalPrice = totalPrice,
                    Quantity = model.NumberRoom,
                    StatusBill = "Chờ thanh toán",
                    CheckInTime = checkInDate,
                    CheckOutTime = checkOutDate,
                    ServiceId = room.Id,
                    IsActive = false
                };

                if (user is not null) bill.UserId = Guid.Parse(user.Id);

                _context.Bills.Add(bill);
                await _context.SaveChangesAsync();

                var createLink = await _payment.CreateLinkPayment(bill.Id);
                dynamic result = createLink.Value ?? new ExpandoObject();

                return Json(new
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data
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

        [HttpPost("/khach-san/room/filter")]
        public async Task<IActionResult> RoomFilter(string slug, string checkInDate, string checkOutDate, int numberRoom)
        {
            try
            {
                var hotel = await _context.Hotels.Include(h => h.Rooms).FirstOrDefaultAsync(h => h.Slug == slug);
                if (hotel is null)
                    return Json(new
                    {
                        Success = false,
                        Message = "Không tìm thấy khách sạn"
                    });

                DateTime checkInTime = DateTime.ParseExact(checkInDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime checkOutTime = DateTime.ParseExact(checkOutDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                checkOutTime.AddHours(24).AddMinutes(59).AddSeconds(59);

                var rooms = hotel.Rooms.ToList();

                rooms = await FilterEmptyRoom(rooms, checkInTime, checkOutTime, numberRoom);

                var json = JsonConvert.SerializeObject(rooms, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                return Json(new
                {
                    Success = true,
                    Message = "Success",
                    Data = json
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

        private async Task<List<Room>> FilterEmptyRoom(List<Room> rooms, DateTime checkInDate, DateTime checkOutDate, int numberRoom)
        {
            try
            {
                // Lấy tất cả bill đang có phòng trùng với ngày tìm kiếm
                // Gộp lại lấy tổng số phòng đang được sử dụng theo id room
                var bills = await _context.Bills
                            .Where(b => b.ServiceType == "Room"
                            && (b.StatusBill == "Hủy thanh toán"
                            || b.StatusBill == "Hủy dịch vụ")
                            && ((b.CheckInTime <= checkInDate
                            && b.CheckOutTime >= checkOutDate)
                            || (b.CheckInTime >= checkInDate
                            && b.CheckInTime <= checkOutDate)
                            || ((b.CheckOutTime >= checkInDate
                            && b.CheckOutTime <= checkOutDate))))
                            .GroupBy(b => b.ServiceId)
                            .Select(g => new
                            {
                                ServiceId = g.Key,
                                Quantity = g.Sum(s => s.Quantity)
                            })
                            .ToListAsync();

                foreach (var item in bills)
                {
                    var roomId = item.ServiceId;
                    var quantity = item.Quantity;

                    // Chỉ lấy những room theo hotel hiện tại
                    var room = rooms.FirstOrDefault(r => r.Id == roomId);
                    if (room is null) continue;

                    // Kiểm tra xem room còn phòng trống hay không
                    // Và số phòng trống phải >= số phòng khách cần tìm
                    if (room.Quantity - quantity > 0 && room.Quantity - quantity >= numberRoom)
                    {
                        // Sửa lại số lượng phòng còn trống
                        room.Quantity -= quantity;
                        // Cập nhật lại phòng trống trong room
                        rooms[rooms.IndexOf(room)] = room;
                    }
                    else
                    {
                        // Xóa những room đã hết phòng
                        rooms.Remove(room);
                    }
                }
                return rooms;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        [HttpGet("/location")]
        public JsonResult GetLocation()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "location.json");
            string jsonData = System.IO.File.ReadAllText(filePath);

            return Json(jsonData);
        }

        private async Task RemoveMediaIsNotUsed()
        {
            //Xóa ảnh chưa được sử dụng khi quá giờ tạo 30m
            DateTime oneTimeAgo = DateTime.UtcNow.AddMinutes(-30);
            var mediaIsNotUsed = await _context.Medias
                                    .Where(m => !m.IsUsed && m.CreateAt < oneTimeAgo)
                                    .Select(m => m.PublicId)
                                    .Take(15)
                                    .ToListAsync();

            if (mediaIsNotUsed.Count > 0)
                await _cloudinary.DeleteMediaAsync(mediaIsNotUsed ?? new List<string>());
        }

        [HttpGet("/error/")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet("/khong-co-quyen")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

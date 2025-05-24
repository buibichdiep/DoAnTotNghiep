using Booking.Areas.Admin.Models.Flight;
using Booking.EF;
using Booking.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Globalization;

namespace Booking.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("/flight/")]
    public class FlightController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _conf;

        public FlightController
            (
                AppDbContext context,
                IHttpClientFactory httpClientFactory,
                IConfiguration conf
            )
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _conf = conf;
        }

        [HttpGet()]
        public async Task<IActionResult> Index()
        {
            //var json = TempData["FlightDTO"] as string ?? string.Empty;

            //var flightDTO = JsonConvert.DeserializeObject<FlightDTO>(json);

            ////var flightDTO = ViewBag.FlightDTO as FlightDTO;

            //if (flightDTO == null)
            //{
            //    TempData["Message"] = "Error: Lỗi dữ liệu";
            //    return RedirectToAction(nameof(Oder));
            //}

            //var client = _httpClientFactory.CreateClient();

            //var outboundDate = flightDTO.OutboundDate.ToString("yyyy-MM-dd");
            //var returnDate = flightDTO.ReturnDate.ToString("yyyy-MM-dd");
            //var numberChildren = flightDTO.NumberChildren is null ? "" : $"&children={flightDTO.NumberChildren}";
            //var numberInfants = flightDTO.NumberInfants is null ? "" : $"&infants_in_seat={flightDTO.NumberInfants}";
            //var apiKey = _conf["SerpApi:ApiKey"] ?? string.Empty;

            //string urlTest = "https://serpapi.com/search.json?engine=google_flights&departure_id=HAN&arrival_id=SGN&gl=vn&hl=vi&currency=VND&type=1&outbound_date=2024-08-22&return_date=2024-08-28&travel_class=1&adults=1&children=1&infants_in_seat=1&api_key=15ac47dc99c1cb8b6cb0cb359591943906e8ec6482737d23c0633a879d0f17fc";

            //string urlApi = "https://serpapi.com/search.json?engine=google_flights" +
            //    $"&departure_id={flightDTO.DepartureId}&arrival_id={flightDTO.ArrivalId}&gl=vn&hl=vi&currency=VND" +
            //    $"&outbound_date={outboundDate}&return_date={returnDate}" +
            //    $"&travel_class={flightDTO.TravelClass}" +
            //    $"&adults={flightDTO.NumberAdults}{numberChildren}{numberInfants}" +
            //    $"&api_key={apiKey}";

            //HttpResponseMessage response = await client.GetAsync(urlApi);
            //if (response.IsSuccessStatusCode)
            //{
            //    var jsonData = await response.Content.ReadAsStringAsync();
            //    // Deserialize JSON thành JObject
            //    var jsonObject = JObject.Parse(jsonData);

            //    if (jsonObject["error"] != null)
            //    {
            //        TempData["Message"] = "Error: Không tìm thấy chuyến bay";
            //        return RedirectToAction(nameof(Oder));
            //    }

            //    // Lấy phần `best_flights`
            //    var bestFlights = jsonObject["best_flights"];

            //    return View(bestFlights);
            //}
            //else
            //{
            //    TempData["Message"] = "Error: Lỗi khi tìm chuyến bay";
            //    return RedirectToAction(nameof(Oder));
            //}

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "flight.json");
            string jsonData = System.IO.File.ReadAllText(filePath);
            // Deserialize JSON thành JObject
            var jsonObject = JObject.Parse(jsonData);

            // Lấy phần `best_flights`
            var bestFlights = jsonObject["best_flights"];

            var listFlight = new List<FlightVM>();

            foreach (var flight in bestFlights ?? new JArray())
            {
                var flightData = new FlightVM
                {
                    Airplane = flight["flights"]?[0]?["airplane"]?.ToString() ?? string.Empty,
                    Airline = flight["flights"]?[0]?["airline"]?.ToString() ?? string.Empty,
                    Departure = "Hà Nội (HAN)",
                    Arrival = "Hồ Chí Minh (SGN)",
                    Price = decimal.Parse(flight["price"]?.ToString() ?? "0"),
                    DepartureTime = ConvertModel.ConvertStringToDatetime(flight["flights"]?[0]?["departure_airport"]?["time"]?.ToString() ?? string.Empty),
                    ArrivalTime = ConvertModel.ConvertStringToDatetime(flight["flights"]?[0]?["arrival_airport"]?["time"]?.ToString() ?? string.Empty),
                    FlightNumber = flight["flights"]?[0]?["flight_number"]?.ToString() ?? string.Empty,
                    FlightLogo = flight["airline_logo"]?.ToString() ?? string.Empty
                };

                listFlight.Add(flightData);
            }

            ViewBag.Flights = listFlight;

            return View();
        }

        [HttpGet("oder")]
        public async Task<IActionResult> Oder()
        {
            await RenderViewFlight();

            return View();
        }

        [HttpPost("oder")]
        public async Task<IActionResult> Oder(FlightDTO model)
        {
            await RenderViewFlight();
            //if (!ModelState.IsValid)
            //{
            //    TempData["Message"] = "Error: Dữ liệu không đúng";
            //    return View(model);
            //}

            //string[] area = ["DNA", "VN", "CU", "CM", "CAU", "CA"];

            //if (area.Contains(model.DepartureId) || area.Contains(model.ArrivalId))
            //{
            //    TempData["Message"] = "Error: Vui lòng chọn thành phố";
            //    return View(model);
            //}

            //if (model.DepartureId == model.ArrivalId)
            //{
            //    TempData["Message"] = "Error: Điểm đi và điểm đến không thể cùng một nơi";
            //    return View(model);
            //}

            //if (model.OutboundDate < DateTime.UtcNow)
            //{
            //    TempData["Message"] = "Error: Ngày đi không thể là ngày trong quá khứ";
            //    return View(model);
            //}

            //if (model.OutboundDate > model.ReturnDate)
            //{
            //    TempData["Message"] = "Error: Thời gian về phải lớn hơn thời gian đi";
            //    return View(model);
            //}

            //string json = JsonConvert.SerializeObject(model);
            //TempData["FlightDTO"] = json;

            return RedirectToAction(nameof(Index));
        }

        private async Task RenderViewFlight()
        {
            var listCityCode = await _context.CityCodes
                                .Where(ct => ct.IdParent == null)
                                .Include(ct => ct.CityCodeIATAChildren)
                                .ToListAsync();

            var items = new List<CityCodeIATA>();
            CreateSelectItems(listCityCode, items, 0);
            var selectList = new SelectList(items, "CodeIATA", "CityName");

            ViewBag.CityCode = selectList;
        }

        private void CreateSelectItems(List<CityCodeIATA> source, List<CityCodeIATA> des, int level)
        {
            string prefix = string.Concat(Enumerable.Repeat("--", level));
            foreach (var cityCode in source)
            {
                // category.Title = prefix + " " + category.Title;
                des.Add(new CityCodeIATA()
                {
                    CodeIATA = cityCode.CodeIATA,
                    CityName = level == 0 ? prefix + " " + cityCode.CityName
                                : prefix + " " + cityCode.CityName + $" ({cityCode.CodeIATA})"
                });
                if (cityCode.CityCodeIATAChildren?.Count > 0)
                {
                    CreateSelectItems(cityCode.CityCodeIATAChildren.ToList(), des, level + 1);
                }
            }
        }
    }
}

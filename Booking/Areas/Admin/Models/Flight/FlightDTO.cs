using System.ComponentModel.DataAnnotations;

namespace Booking.Areas.Admin.Models.Flight
{
    public class FlightDTO
    {
        public string From { get; set; } = null!;
        public string To { get; set; } = null!;
        public string DepartDate { get; set; } = null!;
        public string? ReturnDate { get; set; }
        public string ADT { get; set; } = null!;
        public string? CHD { get; set; }
        public string? INF { get; set; }
        public string TravelClass { get; set; } = null!;
    }
}

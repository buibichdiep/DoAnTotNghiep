using Booking.Models;

namespace Booking.Areas.Admin.Models.Flight
{
    public class FlightOrder : Order
    {
        public decimal Price { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalPrice { get; set; }
        public string Departure { get; set; } = null!;
        public string DepartureTime { get; set; } = null!;
        public string Arrival { get; set; } = null!;
        public string ArrivalTime { get; set; } = null!;
    }
}

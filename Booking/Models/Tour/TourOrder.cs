namespace Booking.Models.Tour
{
    public class TourOrder : Order
    {
        public string DepartureDate { get; set; } = null!;
        public string PaymentMethod { get; set; } = null!;
    }
}

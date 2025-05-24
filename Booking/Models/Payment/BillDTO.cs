namespace Booking.Models.Payment
{
    public class BillDTO
    {
        public string ServiceType { get; set; } = null!;
        public Guid ServiceId { get; set; }
        public int NumberAdults { get; set; }
        public int NumberChildren { get; set; }
        public int NumberInfants { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

    }
}

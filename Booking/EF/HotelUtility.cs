namespace Booking.EF
{
    public class HotelUtility
    {
        public Guid IdHotel { get; set; }
        public Guid IdUtility { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public Hotel Hotel { get; set; } = null!;
        public Utility Utility { get; set; } = null!;
    }
}

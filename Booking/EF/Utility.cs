namespace Booking.EF
{
    public class Utility
    {
        public Guid Id { get; set; }
        public string Icon { get; set; } = null!;
        public string UtilityName { get; set; } = null!;
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }
        public ICollection<HotelUtility>? HotelUtilitys { get; set; }
    }
}

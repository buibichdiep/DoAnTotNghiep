namespace Booking.EF
{
    public class Hotel
    {
        public Guid Id { get; set; }
        public string HotelName { get; set; } = null!;
        public string Avatar { get; set; } = null!;
        public string Address { get; set; } = null!;
        public int Star {  get; set; }
        public string ResidenceType { get; set; } = null!; // Loại cư trú
        public string? Tag { get; set; }
        public string Slug { get; set; } = null!;
        public string Outstanding { get; set; } = null!;
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }
        public Guid IdCateHotel { get; set; }
        public CategoryHotel CateHotel { get; set; } = null!;
        public ICollection<HotelUtility> HotelUtilitys { get; set; } = new List<HotelUtility>();
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}

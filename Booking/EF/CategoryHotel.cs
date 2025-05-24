namespace Booking.EF
{
    public class CategoryHotel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Avatar { get; set; }
        public bool IsOutstanding { get; set; }
        public Guid? IdParent { get; set; }
        public CategoryHotel? CateHotelParent { get; set; }
        public ICollection<CategoryHotel> CateHotelChildren { get; set; } = new List<CategoryHotel>();
        public ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }
    }
}

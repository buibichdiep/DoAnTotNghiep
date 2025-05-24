namespace Booking.EF
{
    public class CategoryTour
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Avatar { get; set; }
        public bool IsOutstanding { get; set; }
        public Guid? IdParent { get; set; }
        public CategoryTour? CateTourParent { get; set; }
        public ICollection<CategoryTour> CateTourChildren { get; set; } = new List<CategoryTour>();
        public ICollection<Tour> Tours { get; set; } = new List<Tour>();
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }
    }
}

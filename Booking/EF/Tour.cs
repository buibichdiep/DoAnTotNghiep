namespace Booking.EF
{
    public class Tour
    {
        public Guid Id { get; set; }
        public string TourName { get; set; } = null!;
        public string Avatar { get; set; } = null!;
        public string Duration { get; set; } = null!; // Khoảng thời gian
        public string Departure { get; set; } = null!; // Điểm khởi hành
        public string Destination { get; set; } = null!; // Điểm đến
        public string Sightseeing { get; set; } = null!; // Các điểm tham quan
        public string Vehicle { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Tag { get; set; }
        public decimal Price { get; set; }
        public decimal? Discount { get; set; }
        public string Description { get; set; } = null!;
        public string Overview { get; set; } = null!;
        public string TravelSchedule { get; set; } = null!;
        public string ServiceInclude { get; set; } = null!;
        public string ServiceNotInclude { get; set; } = null!;
        public int? PercentDeposit { get; set; }
        public Guid IdCateTour { get; set; }
        public CategoryTour CateTour { get; set; } = null!;
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }
        public ICollection<TravelSchedule>? TravelSchedules { get; set; }
    }
}

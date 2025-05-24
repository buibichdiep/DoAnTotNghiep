using Microsoft.AspNetCore.Mvc;

namespace Booking.EF
{
    public class TravelSchedule
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public Guid? IdTour { get; set; }
        public Tour? Tour { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdateAt { get; set; }
    }
}

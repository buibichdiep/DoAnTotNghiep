namespace Booking.Areas.Admin.Models.Tour
{
    public class TravelScheduleDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? Images { get; set; }
        public Guid? IdTour { get; set; }
    }
}

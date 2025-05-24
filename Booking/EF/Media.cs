namespace Booking.EF
{
    public class Media
    {
        public Guid Id { get; set; }
        public string PublicId { get; set; } = null!;
        public string MediaUrl { get; set; } = null!;
        public string MediaType { get; set; } = null!;
        public bool IsAvatar { get; set; }
        public bool IsUsed { get; set; }
        public string? AuthorType { get; set; }
        public Guid? AuthorId { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }
    }
}

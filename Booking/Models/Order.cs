namespace Booking.Models
{
    public abstract class Order
    {
        public int NumberAdult { get; set; }
        public int NumberChildren { get; set; }
        public int NumberInfant { get; set; }
        public string UserName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ContentRequest { get; set; }
    }
}

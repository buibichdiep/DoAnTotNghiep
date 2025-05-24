using Microsoft.AspNetCore.Identity;

namespace Booking.EF
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = null!;
        public string? BirthDay { get; set; }
        public Genders? Gender { get; set; }
        public int Level { get; set; } = 1;
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }
    }

    public enum Genders
    {
        None,
        Male,
        Female,
        Other
    }
}

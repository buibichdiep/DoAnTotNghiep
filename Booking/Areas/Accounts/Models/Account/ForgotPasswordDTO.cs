namespace Booking.Areas.Accounts.Models.Account
{
    public class ForgotPasswordDTO
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
        public string Code { get; set; } = null!;
    }
}

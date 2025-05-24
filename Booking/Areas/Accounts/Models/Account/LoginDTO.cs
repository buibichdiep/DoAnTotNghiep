using System.ComponentModel.DataAnnotations;

namespace Booking.Areas.Accounts.Models.Account
{
    public class LoginDTO
    {
        [Display(Name = "Email hoặc Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập {0}")]
        public string EmailOrPhoneNumber { get; set; } = null!;

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập {0}")]
        [DataType(DataType.Password)]
        [StringLength(32, MinimumLength = 6, ErrorMessage = "Vui lòng nhập {0} độ dài từ {2} tới {1} ký tự")]
        public string Password { get; set; } = null!;

        [Display(Name = "Duy trì đăng nhập")]
        public bool RememberMe { get; set; }
    }
}

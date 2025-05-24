using System.ComponentModel.DataAnnotations;

namespace Booking.Areas.Accounts.Models.Account
{
    public class RegisterDTO
    {
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập {0}")]
        [EmailAddress(ErrorMessage = "Địa chỉ {0} không hợp lệ")]
        public string Email { get; set; } = null!;

        [Display(Name = "Tên người dùng")]
        [Required(ErrorMessage = "Vui lòng nhập {0}")]
        public string UserName { get; set; } = null!;

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập {0}")]
        [RegularExpression(@"^\+?\d+$", ErrorMessage = "{0} không hợp lệ")]
        public string PhoneNumber { get; set; } = null!;

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập {0}")]
        [DataType(DataType.Password)]
        [StringLength(32, MinimumLength = 6, ErrorMessage = "Vui lòng nhập {0} độ dài từ {2} tới {1} ký tự")]
        public string Password { get; set; } = null!;

        [Display(Name = "Nhập lại mật khẩu")]
        [Required(ErrorMessage = "Vui lòng {0}")]
        [Compare("Password", ErrorMessage = "Nhập lại mật khẩu không đúng")]
        public string ConfirmPassword { get; set; } = null!;
    }
}

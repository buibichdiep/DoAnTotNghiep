
using System.ComponentModel.DataAnnotations;

namespace Booking.Areas.Admin.Models.Admin
{
    public class UserAdminDTO
    {
        public string? Id { get; set; }

        [Display(Name = "Họ tên")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public string UserName { get; set; } = null!;

        [Display(Name = "Email")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        [EmailAddress(ErrorMessage = "{0} không đúng")]
        public string Email { get; set; } = null!;

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        [RegularExpression(@"^\+?\d+$", ErrorMessage = "{0} không hợp lệ")]
        public string PhoneNumber { get; set; } = null!;

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập {0}")]
        [DataType(DataType.Password)]
        [StringLength(32, MinimumLength = 6, ErrorMessage = "Vui lòng nhập {0} độ dài từ {2} tới {1} ký tự")]
        public string Password { get; set; } = null!;
        public string? Role { get; set; }
    }
}

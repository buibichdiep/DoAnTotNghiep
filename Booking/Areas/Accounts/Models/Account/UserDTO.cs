using Booking.EF;
using System.ComponentModel.DataAnnotations;

namespace Booking.Areas.Accounts.Models.Account
{
    public class UserDTO
    {
        public string Id { get; set; } = null!;

        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Vui lòng nhập {0}")]
        public string FullName { get; set; } = null!;

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập {0}")]
        [EmailAddress(ErrorMessage = "Địa chỉ {0} không hợp lệ")]
        public string Email { get; set; } = null!;

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập {0}")]
        [RegularExpression(@"^\+?\d+$", ErrorMessage = "{0} không hợp lệ")]
        public string PhoneNumber { get; set; } = null!;

        [Display(Name = "Ngày sinh")]
        public string? BirthDay { get; set; }

        [Display(Name = "Giới tính")]
        public Genders? Gender { get; set; }
    }
}

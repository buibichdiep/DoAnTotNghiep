using Booking.EF;
using System.ComponentModel.DataAnnotations;

namespace Booking.Areas.Admin.Models.Hotel
{
    public class HotelDTO
    {
        public Guid Id { get; set; }
        [Display(Name = "Tên khách sạn")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string HotelName { get; set; } = null!;

        [Display(Name = "Địa chỉ")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string Address { get; set; } = null!;

        [Display(Name = "Hạng khách sạn")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public int Star { get; set; }

        [Display(Name = "Loại hình cư trú")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string ResidenceType { get; set; } = null!; // Loại cư trú

        [Display(Name = "Hình ảnh")]
        public string? Images { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string? AvatarId { get; set; }

        public string? Slug { get; set; }

        public string? Tag { get; set; }

        [Display(Name = "Điểm nổi bật")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string Outstanding { get; set; } = null!;

        [Display(Name = "Danh mục khách sạn")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public Guid IdCateHotel { get; set; }

        public List<Guid> SelectedUtilityIds { get; set; } = new List<Guid>();
        [Display(Name = "Tiện ích")]
        public List<Utility>? Utilities { get; set; }
    }
}

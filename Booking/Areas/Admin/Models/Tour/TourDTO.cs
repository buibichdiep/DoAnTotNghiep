using Booking.EF;
using System.ComponentModel.DataAnnotations;

namespace Booking.Areas.Admin.Models.Tour
{
    public class TourDTO
    {
        public Guid Id { get; set; }

        [Display(Name = "Tên tour")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public string TourName { get; set; } = null!;

        [Display(Name = "Hình ảnh")]
        public string? Images { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string? AvatarId { get; set; }

        [Display(Name = "Khoảng thời gian(ngày/đêm)")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public string Duration { get; set; } = null!;

        [Display(Name = "Điểm khởi hành")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public string Departure { get; set; } = null!;

        [Display(Name = "Điểm đến")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public string Destination { get; set; } = null!;

        [Display(Name = "Các điểm tham quan")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public string Sightseeing { get; set; } = null!;

        [Display(Name = "Phương tiện đi")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public string Vehicle { get; set; } = null!;

        public string? Slug { get; set; }

        public string? Tag { get; set; }

        [Display(Name = "Giá tour")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public decimal Price { get; set; }

        [Display(Name = "Giá giảm")]
        public decimal? Discount { get; set; }

        [Display(Name = "Mô tả tour")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public string Description { get; set; } = null!;

        [Display(Name = "Tổng quan")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public string Overview { get; set; } = null!;

        [Display(Name = "Lịch trình tour")]
        public string? TravelSchedule { get; set; } = null!;

        [Display(Name = "Dịch vụ bao gồm")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public string ServiceInclude { get; set; } = null!;

        [Display(Name = "Dịch vụ không bao gồm")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public string ServiceNotInclude { get; set; } = null!;

        [Display(Name = "Danh mục tour")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public Guid IdCateTour { get; set; }

        [Display(Name = "Phần trăm cọc")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public int? PercentDeposit { get; set; }
    }
}

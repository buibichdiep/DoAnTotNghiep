using System.ComponentModel.DataAnnotations;

namespace Booking.Areas.Admin.Models.Hotel
{
    public class RoomDTO
    {
        public Guid Id { get; set; }

        [Display(Name = "Tên phòng")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string RoomName { get; set; } = null!;
        public string? Style { get; set; }

        [Display(Name = "Giá phòng")]
        [Required(ErrorMessage = "{0} không được để trống")]
        [DataType(DataType.Currency, ErrorMessage = "{0} phải là số")]
        public decimal Price { get; set; }

        [Display(Name = "Giá giảm")]
        [DataType(DataType.Currency, ErrorMessage = "{0} phải là số")]
        public decimal? Discount { get; set; }

        [Display(Name = "Hiển thị giá")]
        public bool PriceShow { get; set; }

        [Display(Name = "Số lượng phòng")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public int Quantity { get; set; }

        [Display(Name = "Hình ảnh")]
        public string? Images { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string? AvatarId { get; set; }

        [Display(Name = "Diện tích phòng")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public int Area { get; set; }

        [Display(Name = "Số lượng người")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string AmountPeople { get; set; } = null!;

        [Display(Name = "Hướng phòng")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string Direction { get; set; } = null!;

        [Display(Name = "Chi tiết giường")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string Bed { get; set; } = null!;

        [Display(Name = "Giường phụ")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string BedMore { get; set; } = null!;

        [Display(Name = "Khách sạn")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public Guid IdHotel { get; set; }

        [Display(Name = "Phần trăm cọc")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public int? PercentDeposit { get; set; }
        public string? HotelName { get; set; }

    }
}

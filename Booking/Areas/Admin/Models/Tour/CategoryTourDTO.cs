using System.ComponentModel.DataAnnotations;

namespace Booking.Areas.Admin.Models.Tour
{
    public class CategoryTourDTO
    {
        public Guid Id { get; set; }

        [Display(Name = "Tên danh mục")]
        [Required(ErrorMessage = "{0} bắt buộc nhập")]
        public string Name { get; set; } = null!;
        [Display(Name = "Ảnh đại diện")]
        public IFormFile? Avatar { get; set; }
        [Display(Name = "Địa điểm nổi bật")]
        public bool IsOutstanding { get; set; }
        [Display(Name = "Danh mục cha")]
        public Guid? IdParent { get; set; }
    }
}

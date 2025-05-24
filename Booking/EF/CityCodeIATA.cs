namespace Booking.EF
{
    public class CityCodeIATA
    {
        public Guid Id { get; set; }
        public string CityName { get; set; } = null!;
        public string CodeIATA { get; set; } = null!;
        public Guid? IdParent { get; set; }
        public CityCodeIATA? CityCodeIATAParent { get; set; }
        public ICollection<CityCodeIATA> CityCodeIATAChildren { get; set; } = new List<CityCodeIATA>();
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }
    }
}

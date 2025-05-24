namespace Booking.EF
{
    public class Room
    {
        public Guid Id { get; set; }
        public string RoomName { get; set; } = null!;
        public string Avatar { get; set; } = null!;
        public string? Style { get; set; }
        public decimal Price { get; set; }
        public decimal? Discount { get; set; }
        public bool PriceShow { get; set; }
        public int Quantity { get; set; }
        public int Area { get; set; }
        public string AmountPeople { get; set; } = null!;
        public string Direction { get; set; } = null!;
        public string Bed { get; set; } = null!;
        public string BedMore { get; set; } = null!;
        public Guid IdHotel { get; set; }
        public Hotel Hotel { get; set; } = null!;
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }
        public int? PercentDeposit { get; set; }
    }
}

namespace Booking.EF
{
    public class Bill
    {
        public Guid Id { get; set; }
        public int NumberAdult { get; set; }
        public int NumberChildren { get; set; }
        public int NumberInfant { get; set; }
        public string UserName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ContentRequest { get; set; }
        public string ServiceType { get; set; } = null!;
        public string ServiceName { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal? Tax { get; set; } // Thuế
        public decimal? Discount { get; set; }
        public decimal TotalPrice { get; set; }
        public int Quantity { get; set; } = 1;
        public string StatusBill { get; set; } = null!;
        public DateTime CheckInTime { get; set; }
        public DateTime CheckOutTime { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }
        public bool? IsActive { get; set; }
        public Guid ServiceId { get; set; }
        public Guid? UserId { get; set; }
        public ICollection<BillPayment> BillPayments { get; set; } = new List<BillPayment>();
    }
}

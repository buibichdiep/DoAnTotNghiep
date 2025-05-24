namespace Booking.EF
{
    public class BillPayment
    {
        public Guid Id { get; set; }
        public int AmountPayment { get; set; } // Số tiền cần thanh toán
        public int? AmountPaid { get; set; } // Số tiền đã thanh toán
        public string NumberAccountReceived { get; set; } = null!; // Số tài khoản nhận tiền
        public string NumberAccountPayment { get; set; } = null!; // Số tài khoản của khách ck
        public string NameAccountPayment { get; set; } = null!; // Tên tài khoản của khách ck
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }
        public DateTime? PaymentTime { get; set; } // Giờ thanh toán
        public Guid BillId { get; set; }
        public Bill Bill { get; set; } = null!;
    }
}

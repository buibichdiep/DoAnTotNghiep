namespace Booking.Models.Payment
{
    public class PaymentQuery
    {
        public int Code {  get; set; }
        public string Id { get; set; } = null!;
        public bool Cancel {  get; set; }
        public string Status { get; set; } = null!;
        public long OrderCode { get; set; }
    }
}

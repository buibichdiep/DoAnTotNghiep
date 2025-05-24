namespace Booking.Models.Hotel
{
    public class HotelOrderDTO
    {
        public string CheckInDate { get; set; } = null!;
        public string CheckOutDate { get; set; } = null!;
        public int ADT { get; set; }
        public int CHD { get; set; }
        public int INF { get; set; }
        public int NumberRoom { get; set; }
    }
}

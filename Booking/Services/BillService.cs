using Booking.EF;
using Microsoft.AspNetCore.Http.Metadata;

namespace Booking.Services
{
    public class BillService
    {
        private readonly ILogger<BillService> _logger;
        private readonly AppDbContext _content;

        public BillService
            (
                ILogger<BillService> logger, 
                AppDbContext content
            )
        {
            _logger = logger;
            _content = content;
        }
    }
}

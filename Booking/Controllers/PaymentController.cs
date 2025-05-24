using Booking.EF;
using Booking.Models;
using Booking.Models.Payment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Net.payOS;
using Net.payOS.Types;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Booking.Services;
using System.Text.Encodings.Web;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using System.Globalization;

namespace Booking.Controllers
{
    [Route("/payment/")]
    public class PaymentController : Controller
    {
        private readonly ILogger<PaymentController> logger;
        private readonly AppDbContext _context;
        private readonly PayOS _payOS;
        private readonly SendMailService _sendMailService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IUrlHelperFactory _urlHelper;

        private readonly int PERCENT_TAX = 20;
        private readonly int PERCENT_ADULTS_PRICE = 80;

        public PaymentController
            (
                ILogger<PaymentController> logger,
                AppDbContext context,
                PayOS payOS,
                SendMailService sendMailService,
                IHttpContextAccessor httpContext,
                IUrlHelperFactory urlHelper
            )
        {
            this.logger = logger;
            _context = context;
            _payOS = payOS;
            _sendMailService = sendMailService;
            _httpContext = httpContext;
            _urlHelper = urlHelper;
        }

        [HttpGet("payOS")]
        public async Task<IActionResult> PaymentPayOS()
        {
            Random random = new Random();
            long orderCode = random.Next(1, 1000000);

            var returnUrl = Url.Content(nameof(ComfirmPayment)); // https://localhost:44319/payment/payOS/confirm
            var cancelUrl = Url.Content(nameof(CancelPayment)); //https://localhost:44319/payment/payOS/cancel

            ItemData item = new ItemData("Deluxe King (1 đêm)", 5, 20000);
            List<ItemData> items = new List<ItemData>();
            items.Add(item);

            //var totalPrice = item.price * item.quantity;
            var totalPrice = 10000;

            PaymentData paymentData = new PaymentData(orderCode, totalPrice, RandomGenerator.RandomCode(10), items, cancelUrl, returnUrl);

            CreatePaymentResult createPaymentResult = await _payOS.createPaymentLink(paymentData);

            if (createPaymentResult == null || string.IsNullOrEmpty(createPaymentResult.checkoutUrl))
            {
                // Xử lý lỗi nếu không có URL thanh toán trả về
                return View("Error");
            }

            return Redirect(createPaymentResult.checkoutUrl);
        }

        [HttpGet("payOS/create-link")]
        public async Task<JsonResult> CreateLinkPayment(Guid id)
        {
            try
            {
                var bill = await _context.Bills.FindAsync(id);
                if (bill is null)
                {
                    TempData["Message"] = "Không tìm thấy hóa đơn";
                    return Json(new
                    {
                        Success = false,
                        Message = "Không tìm thấy hóa đơn"
                    });
                }

                Random random = new Random();
                long orderCode = random.Next(1, 1000000);

                var httpContext = _httpContext.HttpContext;
                var urlHelper = _urlHelper.GetUrlHelper(new ActionContext(httpContext, httpContext.GetRouteData(), new ActionDescriptor()));

                var returnUrlAbsolute = urlHelper.Action(nameof(ComfirmPayment), "Payment", new { id = bill.Id });
                var cancelUrlAbsolute = urlHelper.Action(nameof(CancelPayment), "Payment", new { id = bill.Id });
                var host = httpContext.Request.Scheme + "://" + httpContext.Request.Host;
                var returnUrl = host + returnUrlAbsolute; //https://localhost:44319/payment/payOS/confirm?orderCode=
                var cancelUrl = host + cancelUrlAbsolute; //https://localhost:44319/payment/payOS/cancel?orderCode=

                var priceItem = Math.Round(bill.TotalPrice / bill.Quantity / 1000) * 1000;

                ItemData item = new ItemData(bill.ServiceName, bill.Quantity, Convert.ToInt32(priceItem));
                List<ItemData> items = new List<ItemData>();
                items.Add(item);

                var totalPrice = item.price * item.quantity;

                PaymentData paymentData = new PaymentData(orderCode, totalPrice, RandomGenerator.RandomCode(10), items, cancelUrl, returnUrl);

                CreatePaymentResult createPaymentResult = await _payOS.createPaymentLink(paymentData);

                if (createPaymentResult == null || string.IsNullOrEmpty(createPaymentResult.checkoutUrl))
                {
                    // Xử lý lỗi nếu không có URL thanh toán trả về
                    TempData["Message"] = "Lỗi thanh toán";
                    return Json(new
                    {
                        Success = false,
                        Message = "Lỗi thanh toán"
                    });
                }

                //return Redirect(createPaymentResult.checkoutUrl);
                return Json(new
                {
                    Success = true,
                    Message = "Success",
                    Data = createPaymentResult.checkoutUrl
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("payOS/confirm/{id}")]
        public async Task<IActionResult> ComfirmPayment(Guid id, PaymentQuery model)
        {
            try
            {
                PaymentLinkInformation paymentLinkInfo = await _payOS.getPaymentLinkInformation(model.OrderCode);
                if (paymentLinkInfo.status != "PAID")
                {
                    TempData["Message"] = paymentLinkInfo.status;
                    return RedirectToAction("Index", "Home");
                }

                var bill = await _context.Bills.FindAsync(id);
                if (bill is null)
                {
                    TempData["Message"] = "Không tìm thấy hóa đơn";
                    return RedirectToAction("Index", "Home");
                }
                var dateString = paymentLinkInfo.transactions.First().transactionDateTime;
                DateTime paymentTime = DateTime.ParseExact(dateString, "yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture);

                var billPayment = new BillPayment
                {
                    Id = Guid.NewGuid(),
                    AmountPayment = paymentLinkInfo.amount,
                    AmountPaid = paymentLinkInfo.amountPaid,
                    NumberAccountReceived = paymentLinkInfo.transactions.First().accountNumber,
                    NumberAccountPayment = paymentLinkInfo.transactions.First().counterAccountNumber ?? string.Empty,
                    NameAccountPayment = paymentLinkInfo.transactions.First().counterAccountName ?? string.Empty,
                    PaymentTime = paymentTime,
                    BillId = bill.Id
                };

                _context.BillPayments.Add(billPayment);

                bill.StatusBill = "Đã thanh toán";
                _context.Bills.Update(bill);

                await _context.SaveChangesAsync();

                // Gửi email thông báo đặt dịch vụ thành công
                var htmlMessage = $@"<h3>Bạn đã đặt dịch vụ <b>{bill.ServiceName}</b> trên trang DiepTourist của chúng tôi.</h3>
                    <p>Chúng tôi cảm ơn khi bạn đã tin tưởng và sử dụng dịch vụ của chúng tôi.</p>
                    <p>Chúc bạn có những trải nghiệm vui vẻ.</p>";

                await _sendMailService.SendMailAsync(bill.Email, "Xác nhận đặt dịch vụ", htmlMessage);

                TempData["Message"] = "Thanh toán thành công";

                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                TempData["Message"] = "Lỗi khi thanh toán dịch vụ";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet("payOS/cancel/{id}")]
        public async Task<IActionResult> CancelPayment(Guid id, PaymentQuery model)
        {
            try
            {
                PaymentLinkInformation paymentCancel = await _payOS.cancelPaymentLink(model.OrderCode);

                if (paymentCancel.status != "CANCELLED")
                {
                    TempData["Message"] = paymentCancel.status;
                    return RedirectToAction("Index", "Home");
                }

                var bill = await _context.Bills.FindAsync(id);
                if (bill is null)
                {
                    TempData["Message"] = "Không tìm thấy hóa đơn";
                    return RedirectToAction("Index", "Home");
                }

                bill.StatusBill = "Hủy thanh toán";
                bill.UpdateAt = DateTime.UtcNow;

                _context.Bills.Update(bill);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Hủy thanh toán thành công";

                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                TempData["Message"] = "Lỗi khi hủy dịch vụ";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet("success")]
        public IActionResult PaymentSuccess()
        {
            return View();
        }

        [HttpGet("cancel")]
        public IActionResult PaymentCancel()
        {
            return View();
        }
    }
}

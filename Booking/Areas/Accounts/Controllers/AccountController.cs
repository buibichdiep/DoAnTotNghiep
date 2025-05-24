using Booking.Areas.Accounts.Models.Account;
using Booking.Controllers;
using Booking.EF;
using Booking.Models;
using Booking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace Booking.Areas.Accounts.Controllers
{
    [Area("Accounts")]
    [Route("/customer/")]
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly SendMailService _sendMailService;

        public AccountController
            (
                AppDbContext context,
                UserManager<User> userManager,
                SignInManager<User> signInManager,
                SendMailService sendMailService
            )
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _sendMailService = sendMailService;
        }

        [HttpGet("login")]
        public IActionResult Login(string? returnUrl, string? error)
        {
            returnUrl ??= Url.Content("/");
            ViewData["ReturnUrl"] = returnUrl;

            if (!string.IsNullOrEmpty(error))
                TempData["Message"] = "Error: Lỗi sử dụng dịch vụ ngoài";

            return View();
        }

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDTO model, string? returnUrl)
        {
            returnUrl ??= Url.Content("~/");
            ViewData["ReturnUrl"] = returnUrl;

            // Tìm user theo email
            var user = await _userManager.FindByEmailAsync(model.EmailOrPhoneNumber);
            if (user == null)
            {
                // Tìm user theo phone number
                user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.EmailOrPhoneNumber);
                if (user == null)
                {
                    TempData["Message"] = "Error: Không tìm thấy thông tin tài khoản";
                    return View(model);
                }
            }

            if (!user.EmailConfirmed)
            {
                TempData["Message"] = "Error: Vui lòng xác thực email";
                return View(model);
            }
            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                TempData["Message"] = "Error: Thông tin đăng nhập không chính xác";
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
                return LocalRedirect(returnUrl);

            if (!result.IsLockedOut)
            {
                TempData["Message"] = "Error: Tài khoản của bạn đã bị khóa";
                return View(model);
            }

            TempData["Message"] = "Error: Hệ thống đang bảo trì";

            return View(model);
        }

        [HttpGet("register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "error : Thông tin không chính xác";
                return View(model);
            }

            var userByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (userByEmail != null)
            {
                TempData["Message"] = "error : Email đã được sử dụng";
                return View(model);
            }

            var userByPhoneNumber = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);
            if (userByPhoneNumber != null)
            {
                TempData["Message"] = "error : Số điện thoại đã được sử dụng";
                return View(model);
            }

            var user = new User
            {
                Email = model.Email,
                FullName = model.UserName,
                UserName = ConvertModel.ConvertUserName(model.UserName),
                PhoneNumber = model.PhoneNumber
            };

            Random random = new Random();

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    if (error.Code == nameof(IdentityErrorDescriber.DuplicateUserName))
                        error.Description = "Tên người dùng đã tồn tại";

                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Tạo code xác thực email
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.ActionLink(
                    action: nameof(ConfirmEmail),
                    values:
                        new
                        {
                            area = "Accounts",
                            userId = user.Id,
                            code = code
                        },
                    protocol: Request.Scheme);

            await _sendMailService.SendMailAsync(model.Email,
                "Xác nhận địa chỉ email",
                @$"Bạn đã đăng ký tài khoản trên DiepTourist, 
                       hãy <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>bấm vào đây</a> 
                       để kích hoạt tài khoản.");

            TempData["Message"] = "Hãy kiểm tra hòm thư để biết cách xác thực tài khoản";

            return RedirectToAction(nameof(Login));
        }

        [HttpGet("confirm")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            TempData["Message"] = "Error: Có lỗi xảy ra, vui lòng thao tác lại";
            if (userId == null || code == null) return View("Register");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return View("Register");

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                TempData["Message"] = "Success: Xác thực thành công, hãy đăng nhập vào tài khoản";
                return View("Login");
            }

            return View("Register");
        }

        [HttpGet("logout")]
        [Authorize]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost("login-external")]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl)
        {
            returnUrl ??= Url.Content("~/");
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet("login-external-callback")]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl, string? remoteError)
        {
            returnUrl ??= Url.Content("~/");

            var info = _signInManager.GetExternalLoginInfoAsync();
            if (info == null || remoteError != null)
            {
                TempData["Message"] = "Error: Lỗi sử dụng dịch vụ ngoài";
                return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
            }

            var email = info.Result?.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var userName = info.Result?.Principal.FindFirst(ClaimTypes.Name)?.Value;
            if (email == null)
            {
                TempData["Message"] = "Error: Không tìm thấy email";
                return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
            }

            // Kiểm tra user đã được đăng ký chưa
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                // Kiểm tra user có liên kết với google hay không
                var userLogins = await _userManager.GetLoginsAsync(user);
                var hasLoginGoogle = userLogins.Any(u => u.LoginProvider == "Google");
                // User chưa được liên kết với google
                if (!hasLoginGoogle)
                {
                    TempData["Message"] = "Error: Email đã được đăng ký bằng hình thức khác";
                    return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
                }
            }

            // Đăng nhập nếu user đã được liên kết với google
            var result = await _signInManager
                .ExternalLoginSignInAsync(info.Result!.LoginProvider, info.Result.ProviderKey, isPersistent: false);

            if (result.Succeeded)
            {
                // Cập nhật lại token
                await _signInManager.UpdateExternalAuthenticationTokensAsync(info.Result);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                TempData["Message"] = "Info: Tài khoản bị tạm khóa, hãy thử lại sau";
                return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
            }
            else // User chưa có account, tạo mới account
            {
                var newUser = new User
                {
                    Email = email,
                    FullName = userName!,
                    UserName = ConvertModel.ConvertUserName(userName!)
                };

                var resultNewUser = await _userManager.CreateAsync(newUser);
                if (resultNewUser.Succeeded)
                {
                    await _userManager.AddLoginAsync(newUser, info.Result);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                    await _userManager.ConfirmEmailAsync(newUser, code);

                    await _signInManager.SignInAsync(newUser, isPersistent: false);

                    return LocalRedirect(returnUrl);
                }
                else
                {
                    TempData["Message"] = "Error: Lỗi sử dụng dịch vụ ngoài";
                    return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
                }
            }
        }

        [HttpGet("quen-mat-khau")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost("quen-mat-khau")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (!IsValidEmail(email))
            {
                TempData["Message"] = "Error: Nhập sai định dạng email";
                return View();
            }

            var user = await _userManager.FindByNameAsync(email);
            if(user is null)
            {
                TempData["Message"] = "Error: Không tìm thấy tài khoản theo email";
                return View();
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.ActionLink(
                action: nameof(ResetPassword),
                values: new { area = "Accounts", code },
                protocol: Request.Scheme);

            await _sendMailService.SendMailAsync(email,
                "Quên mật khẩu",
                @$"Bạn đã yêu cầu lấy lại mật khẩu, 
                       hãy <a href='{HtmlEncoder.Default.Encode(callbackUrl ?? string.Empty)}'>bấm vào đây</a> 
                       để lấy lại mật khẩu.");

            TempData["Message"] = "Hãy kiểm tra hòm thư để biết cách lấy lại mật khẩu";

            return View();
        }

        [HttpGet()]
        public IActionResult ResetPassword(string? code = null)
        {
            return View();
        }

        [HttpGet("info")]
        [Authorize]
        public async Task<IActionResult> InfoUser()
        {
            var userName = User.Identity?.Name ?? string.Empty;
            var user = await _userManager.FindByNameAsync(userName);

            if (user is null)
            {
                TempData["Message"] = "Error: Không tìm thấy thông tin người dùng";
                return RedirectToAction(nameof(Login));
            }

            var userDTO = new UserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                BirthDay = user.BirthDay,
                Gender = user.Gender,
            };

            return View(userDTO);
        }

        [HttpPost("info")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InfoUser(UserDTO model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Error: Thông tin không chính xác";
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user is null)
            {
                TempData["Message"] = "Error: Không tìm thấy người dùng";
                return View(model);
            }

            if (user.Email != model.Email)
            {
                var userByEmail = await _userManager.FindByEmailAsync(model.Email);
                if (userByEmail is not null)
                {
                    TempData["Message"] = "Error: Địa chỉ email đã được sử dụng";
                    return View(model);
                }
            }

            if (user.PhoneNumber != model.PhoneNumber)
            {
                var userByPhone = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);
                if (userByPhone is not null)
                {
                    TempData["Message"] = "Error: Số điện thoại đã được sử dụng";
                    return View(model);
                }
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.BirthDay = model.BirthDay;
            user.Gender = model.Gender;

            TempData["Message"] = "Success: Cập nhật thông tin thành công";

            return View(model);
        }

        [HttpGet("security")]
        [Authorize]
        public IActionResult SecurityUser()
        {
            return View();
        }

        [HttpPost("change-password")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmNewPassword)
        {
            var userName = User.Identity?.Name ?? string.Empty;
            var user = await _userManager.FindByNameAsync(userName);

            if (user is null)
            {
                TempData["Message"] = "Error: Không tìm thấy thông tin người dùng";
                return RedirectToAction(nameof(Login));
            }

            if (string.IsNullOrEmpty(oldPassword)
                || string.IsNullOrEmpty(newPassword)
                || string.IsNullOrEmpty(confirmNewPassword)
            )
            {
                TempData["Message"] = "Error: Vui lòng nhập đầy đủ thông tin";
                return RedirectToAction(nameof(SecurityUser));
            }

            if (newPassword.Length < 6 || newPassword.Length > 32)
            {
                TempData["Message"] = "Error: Mật khẩu mới phải từ 6 đến 32 ký tự";
                return RedirectToAction(nameof(SecurityUser));
            }

            if (!newPassword.Equals(confirmNewPassword))
            {
                TempData["Message"] = "Error: Mật khẩu và nhập lại mật khẩu phải trùng nhau";
                return RedirectToAction(nameof(SecurityUser));
            }

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

            if (!result.Succeeded)
            {
                TempData["Message"] = "Error: Xảy ra lỗi";
                return RedirectToAction(nameof(Login));
            }

            TempData["Message"] = "Success: Đổi mật khẩu thành công";

            return RedirectToAction(nameof(SecurityUser));
        }

        [HttpGet("service/order-history")]
        [Authorize]
        public async Task<IActionResult> ServiceOrderHistory()
        {
            var userName = User.Identity?.Name ?? string.Empty;
            var user = await _userManager.FindByNameAsync(userName) ?? new User();

            var bills = await _context.Bills.Where(b => b.UserId.ToString() == user.Id && b.StatusBill != "Chờ thanh toán").ToListAsync();
            return View(bills);
        }

        [HttpGet("service/order-history/details")]
        [Authorize]
        public async Task<IActionResult> ServiceOrderDetails(Guid id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if(bill is null)
            {
                TempData["Message"] = "Error: Không tìm thấy hóa đơn";
                return RedirectToAction(nameof(ServiceOrderHistory));
            }

            return View(bill);
        }

        [HttpPost("service/cancel")]
        [Authorize]
        public async Task<IActionResult> ServiceCancel(string id)
        {
            try
            {
                if (!Guid.TryParse(id, out var guidId))
                    return Json(new
                    {
                        Success = false,
                        Message = "Kiểu dữ liệu id không đúng"
                    });

                var bill = await _context.Bills.FindAsync(guidId);
                if (bill is null)
                    return Json(new
                    {
                        Success = false,
                        Message = "Không tìm thấy hóa đơn"
                    });

                bill.StatusBill = "Hủy dịch vụ";
                bill.UpdateAt = DateTime.UtcNow;

                _context.Bills.Update(bill);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    Success = true,
                    Message = "Success"
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

        private bool IsValidEmail(string email)
        {
            // Định nghĩa mẫu regex để kiểm tra email
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            // Sử dụng Regex.IsMatch để kiểm tra
            return Regex.IsMatch(email, pattern);
        }
    }
}

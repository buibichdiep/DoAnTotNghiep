using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Booking.EF;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using Booking.Models;
using Booking.Controllers;
using MailKit.Net.Imap;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;

namespace Booking.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("/admin/")]
    [Authorize(Roles = RoleName.Administrator)]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly AppDbContext _context;
        private readonly CloudinaryController _cloudinary;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly int ITEM_PER_PAGE = 10;

        public AdminController
            (
                ILogger<AdminController> logger,
                AppDbContext context,
                CloudinaryController cloudinary,
                UserManager<User> userManager,
                RoleManager<IdentityRole> roleManager
            )
        {
            _logger = logger;
            _context = context;
            _cloudinary = cloudinary;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("user/authorize")]
        public async Task<IActionResult> AuthorizeUser(string idUser, string? roleName)
        {
            var user = await _userManager.FindByIdAsync(idUser);
            if (user is null)
            {
                TempData["Message"] = "Error: Không tìm thấy người dùng";
                return View("Index");
            }

            var roleByUser = await _userManager.GetRolesAsync(user);

            if (string.IsNullOrEmpty(roleName))
            {
                if (roleByUser.Any())
                {
                    var removeRole = await _userManager.RemoveFromRolesAsync(user, roleByUser.ToArray());
                    if (!removeRole.Succeeded)
                    {
                        TempData["Message"] = "Error: Phân quyền không thành công";
                        return View("Index");
                    }
                }

                TempData["Message"] = "Success: Phân quyền thành công";
                return View("Index");
            } 

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                TempData["Message"] = "Error: Không tìm thấy quyền hạn";
                return View("Index");
            }
            
            if (roleByUser.Any())
            {
                var removeRole = await _userManager.RemoveFromRolesAsync(user, roleByUser.ToArray());
                if (!removeRole.Succeeded)
                {
                    TempData["Message"] = "Error: Phân quyền không thành công";
                    return View("Index");
                }
            }

            var result = await _userManager.AddToRoleAsync(user, role.Name ?? string.Empty);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    if (error.Code == "UserAlreadyInRole")
                    {
                        TempData["Message"] = "Error: Người dùng đã có quyền đó rồi";
                        return View("Index");
                    }
                }
                TempData["Message"] = "Error: Phân quyền không thành công";
                return View("Index");
            }

            TempData["Message"] = "Success: Phân quyền thành công";

            return View("Index");
        }

        [HttpGet("revenue")]
        [Authorize(Roles = RoleName.Administrator + "," + RoleName.ServiceProvider)]
        public async Task<IActionResult> Revenue(string? dateIn, string? dateOut, int page = 1)
        {
            var bills = await _context.Bills.Where(b => b.StatusBill != "Chờ thanh toán").ToListAsync();

            if (dateIn != null && dateOut != null)
            {
                DateTime checkIn = DateTime.ParseExact(dateIn, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime checkOut = DateTime.ParseExact(dateOut, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                bills = bills.Where(b => b.CreateAt >= checkIn && b.CreateAt <= checkOut).ToList();
            }

            if (dateIn != null && dateOut == null)
            {
                DateTime checkIn = DateTime.ParseExact(dateIn, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                bills = bills.Where(b => b.CheckInTime >= checkIn).ToList();
            }

            var total = bills.Count();
            var countPage = (int)Math.Ceiling((double)total / ITEM_PER_PAGE);
            page = page > countPage ? countPage : page;

            bills = bills.Skip((page - 1) * ITEM_PER_PAGE).Take(ITEM_PER_PAGE).ToList();

            ViewBag.CountPage = countPage;
            ViewBag.DateIn = dateIn;
            ViewBag.DateOut = dateOut;

            return View(bills);
        }

        [HttpGet("revenue/detail")]
        [Authorize(Roles = RoleName.Administrator + "," + RoleName.ServiceProvider)]
        public async Task<IActionResult> RevenueDetail(Guid id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill is null)
            {
                TempData["Message"] = "Error: Không tìm thấy hóa đơn";
                return RedirectToAction(nameof(Revenue));
            }

            var billPayment = await _context.BillPayments.FirstOrDefaultAsync(b => b.BillId == bill.Id);
            ViewBag.BillPayment = billPayment;

            return View(bill);
        }

        [HttpGet("service")]
        [Authorize(Roles = RoleName.Administrator + "," + RoleName.ServiceProvider)]
        public async Task<IActionResult> Service(string? dateIn, string? dateOut, int page = 1)
        {
            var bills = await _context.Bills.Where(b => b.StatusBill != "Chờ thanh toán").ToListAsync();

            if (dateIn != null && dateOut != null)
            {
                DateTime checkIn = DateTime.ParseExact(dateIn, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime checkOut = DateTime.ParseExact(dateOut, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                bills = bills.Where(b => b.CreateAt >= checkIn && b.CreateAt <= checkOut).ToList();
            }

            if (dateIn != null && dateOut == null)
            {
                DateTime checkIn = DateTime.ParseExact(dateIn, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                bills = bills.Where(b => b.CheckInTime >= checkIn).ToList();
            }

            var total = bills.Count();
            var countPage = (int)Math.Ceiling((double)total / ITEM_PER_PAGE);
            page = page > countPage ? countPage : page;

            bills = bills.Skip((page - 1) * ITEM_PER_PAGE).Take(ITEM_PER_PAGE).ToList();

            ViewBag.CountPage = countPage;
            ViewBag.DateIn = dateIn;
            ViewBag.DateOut = dateOut;

            return View(bills);
        }

        [HttpPost("active-bill")]
        [Authorize(Roles = RoleName.Administrator + "," + RoleName.ServiceProvider)]
        public async Task<ActionResult> ActiveBill(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new
                    {
                        Success = false,
                        Message = "Id không có dữ liệu"
                    });

                if (!Guid.TryParse(id, out var guidId))
                    return Json(new
                    {
                        Success = false,
                        Message = "Kiểu dữ liệu id không đúng"
                    });

                var bill = await _context.Bills.FindAsync(guidId);
                if (bill == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Không tìm thấy hóa đơn"
                    });
                }

                bill.IsActive = true;
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

        [HttpDelete("delete-bill")]
        [Authorize(Roles = RoleName.Administrator + "," + RoleName.ServiceProvider)]
        public async Task<ActionResult> DeleteBill(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new
                    {
                        Success = false,
                        Message = "Id không có dữ liệu"
                    });

                if (!Guid.TryParse(id, out var guidId))
                    return Json(new
                    {
                        Success = false,
                        Message = "Kiểu dữ liệu id không đúng"
                    });

                var bill = await _context.Bills.FindAsync(guidId);
                if (bill == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Không tìm thấy hóa đơn"
                    });
                }

                var billPayment = await _context.BillPayments.FirstOrDefaultAsync(x => x.BillId == bill.Id);
                if (billPayment != null)
                {
                    _context.BillPayments.Remove(billPayment);
                }

                _context.Bills.Remove(bill);

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
    }
}

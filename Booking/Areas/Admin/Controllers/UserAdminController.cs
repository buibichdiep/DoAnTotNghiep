using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Booking.Areas.Admin.Models.Admin;
using Booking.EF;
using Microsoft.AspNetCore.Identity;
using Booking.Controllers;
using Booking.Models;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Authorization;

namespace Booking.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("/admin/user")]
    [Authorize(Roles = RoleName.Administrator)]
    public class UserAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryController _cloudinary;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly int ITEM_PER_PAGE = 10;

        public UserAdminController
            (
                AppDbContext context,
                CloudinaryController cloudinary,
                UserManager<User> userManager, 
                RoleManager<IdentityRole> roleManager
            )
        {
            _context = context;
            _cloudinary = cloudinary;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            var users = await _context.Users.Where(u => u.Email != "admin@gmail.com").ToListAsync();

            var roles = _roleManager.Roles.ToList();

            ViewBag.Roles = roles;

            var total = users.Count();
            var countPage = (int)Math.Ceiling((double)total / ITEM_PER_PAGE);
            page = page > countPage ? countPage : page;

            users = users.Skip((page - 1) * ITEM_PER_PAGE).Take(ITEM_PER_PAGE).ToList();

            ViewBag.CountPage = countPage;

            return View(users);
        }

        [HttpDelete("delete")]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return Json(new
                    {
                        Success = false,
                        Message = "Id không có dữ liệu"
                    });

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Không tìm thấy tài khoản"
                    });
                }

                _context.Users.Remove(user);

                // Xóa tất cả ảnh theo user
                var listPublicId = await _context.Medias
                                .Where(m => m.AuthorType == "User"
                                && m.AuthorId.ToString() == user.Id)
                                .Select(m => m.PublicId)
                                .ToListAsync();

                if (listPublicId.Count > 0)
                {
                    // Xóa ảnh trên cloundinary
                    await _cloudinary.DeleteMediaAsync(listPublicId);
                }

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

        //// GET: Admin/UserAdmin/Details/5
        //public async Task<IActionResult> Details(string id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var userAdminDTO = await _context.UserAdminDTO
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (userAdminDTO == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(userAdminDTO);
        //}

        [HttpGet("create")]
        public IActionResult Create()
        {
            var roles = _roleManager.Roles.ToList();

            ViewBag.Roles = roles;

            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserAdminDTO model)
        {
            var roles = _roleManager.Roles.ToList();
            ViewBag.Roles = roles;

            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Error: Vui lòng nhập đầy đủ thông tin";
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
                EmailConfirmed = true,
                FullName = model.UserName,
                UserName = ConvertModel.ConvertUserName(model.UserName),
                PhoneNumber = model.PhoneNumber,
                PhoneNumberConfirmed = true
            };

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

            if(model.Role != null)
            {
                var resultRole = await _userManager.AddToRoleAsync(user, model.Role);
                if (!resultRole.Succeeded)
                {
                    TempData["Message"] = "Error : Lỗi phân quyền";
                    return View(model);
                }
            }

            TempData["Message"] = "Success : Tạo mới tài khoản thành công";

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/UserAdmin/Edit/5
        [HttpGet("update")]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Message"] = "Error: Không tìm thấy người dùng";
                return RedirectToAction(nameof(Index));
            }

            var roles = await _context.Roles.ToListAsync();
            ViewBag.Roles = roles;
            
            var roleByUser = await _userManager.GetRolesAsync(user);

            var userAdminDTO = new UserAdminDTO
            {
                Id = user.Id,
                UserName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Password = string.Empty,
                Role = roleByUser.FirstOrDefault(),
            };

            return View(userAdminDTO);
        }

        [HttpPost("update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserAdminDTO model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || id != model.Id)
            {
                TempData["Message"] = "Error: Không tìm thấy người dùng";
                return RedirectToAction(nameof(Index));
            }

            var roles = await _context.Roles.ToListAsync();
            ViewBag.Roles = roles;

            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Error: Thông tin không chính xác";
                return View(model);
            }

            user.FullName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            await _userManager.RemovePasswordAsync(user);
            await _userManager.AddPasswordAsync(user, model.Password);

            var role = await _roleManager.FindByNameAsync(model.Role ?? string.Empty);
            if(role is null)
            {
                TempData["Message"] = "Error: Không tìm thấy quyền";
                return View(model);
            }

            var result = await _userManager.AddToRoleAsync(user, role.Name ?? string.Empty);
            if(!result.Succeeded)
            {
                TempData["Message"] = "Error: Phân quyền không thành công";
                return View(model);
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Success: Cập nhật thành công";

            return RedirectToAction(nameof(Index));
        }

        //// GET: Admin/UserAdmin/Delete/5
        //public async Task<IActionResult> Delete(string id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var userAdminDTO = await _context.UserAdminDTO
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (userAdminDTO == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(userAdminDTO);
        //}

        //// POST: Admin/UserAdmin/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(string id)
        //{
        //    var userAdminDTO = await _context.UserAdminDTO.FindAsync(id);
        //    if (userAdminDTO != null)
        //    {
        //        _context.UserAdminDTO.Remove(userAdminDTO);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool UserAdminDTOExists(string id)
        //{
        //    return _context.UserAdminDTO.Any(e => e.Id == id);
        //}
    }
}

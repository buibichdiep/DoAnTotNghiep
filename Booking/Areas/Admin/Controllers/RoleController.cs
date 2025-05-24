using Booking.EF;
using Booking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("/admin/role")]
    [Authorize(Roles = RoleName.Administrator)]
    public class RoleController : Controller
    {
        private readonly AppDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(AppDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var role = _context.Roles.ToList();

            return View(role);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                return Json(new
                {
                    Success = false,
                    Message = "Tên role không được để trống"
                });

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if(!result.Succeeded)
            {
                foreach(var error in result.Errors)
                {
                    if(error.Code == "DuplicateRoleName")
                        return Json(new
                        {
                            Success = false,
                            Message = "Tên role đã được sử dụng"
                        });
                }
            }

            return Json(new
            {
                Success = true,
                Message = "Success"
            });
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update(string id, string roleName)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(roleName))
                return Json(new
                {
                    Success = false,
                    Message = "Dữ liệu không được để trống"
                });

            var roleDb = await _roleManager.FindByIdAsync(id);
            if (roleDb == null)
                return Json(new
                {
                    Success = false,
                    Message = "Không tìm thấy role trong hệ thống"
                });

            roleDb.Name = roleName;

            var result = await _roleManager.UpdateAsync(roleDb);

            if (!result.Succeeded)
            {
                foreach(var error in result.Errors)
                {
                    if(error.Code == "DuplicateRoleName")
                        return Json(new
                        {
                            Success = false,
                            Message = "Tên role đã được sử dụng"
                        });
                }
            }

            return Json(new
            {
                Success = true,
                Message = "Success"
            });
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

                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Không tìm thấy role"
                    });
                }

                await _roleManager.DeleteAsync(role);

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

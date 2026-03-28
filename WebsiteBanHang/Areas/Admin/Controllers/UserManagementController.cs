using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebsiteBanHang.Models;

namespace WebsiteBanHang.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller quản lý người dùng dành cho Admin.
    /// Cho phép xem danh sách, thêm mới, chỉnh sửa thông tin, đổi mật khẩu và xóa tài khoản.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Hiển thị danh sách toàn bộ người dùng và vai trò của họ.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                var viewModel = new UserRolesViewModel
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Roles = await _userManager.GetRolesAsync(user)
                };
                userRolesViewModel.Add(viewModel);
            }

            return View(userRolesViewModel);
        }

        /// <summary>
        /// Hiển thị form chỉnh sửa thông tin người dùng.
        /// </summary>
        /// <param name="id">ID của người dùng cần sửa</param>
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();

            var viewModel = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Address = user.Address,
                Age = user.Age,
                Roles = allRoles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name,
                    Selected = r.Name != null && userRoles.Contains(r.Name)
                }).ToList()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Xử lý cập nhật thông tin người dùng, đổi mật khẩu và gán lại vai trò.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null) return NotFound();

                user.FullName = model.FullName;
                user.Address = model.Address;
                user.Age = model.Age;
                user.Email = model.Email;
                user.UserName = model.Email;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Xử lý đặt lại mật khẩu nếu Admin nhập mật khẩu mới
                    if (!string.IsNullOrEmpty(model.NewPassword))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var resetResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                        if (!resetResult.Succeeded)
                        {
                            foreach (var error in resetResult.Errors)
                            {
                                ModelState.AddModelError("", "Lỗi khi đặt lại mật khẩu: " + error.Description);
                            }
                            goto ReturnView; 
                        }
                    }

                    // Cập nhật vai trò mới (Xóa vai trò cũ, thêm vai trò đã chọn)
                    var userRoles = await _userManager.GetRolesAsync(user);
                    var selectedRoles = model.SelectedRoles ?? new List<string>();

                    var rolesToAdd = selectedRoles.Except(userRoles);
                    var rolesToRemove = userRoles.Except(selectedRoles);

                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

        ReturnView:
            var allRoles = await _roleManager.Roles.ToListAsync();
            model.Roles = allRoles.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Name,
                Selected = r.Name != null && (model.SelectedRoles?.Contains(r.Name) ?? false)
            }).ToList();

            return View(model);
        }

        /// <summary>
        /// Xử lý xóa người dùng khỏi hệ thống.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            return BadRequest("Error deleting user");
        }

        /// <summary>
        /// Hiển thị form tạo người dùng mới.
        /// </summary>
        public async Task<IActionResult> Create()
        {
            var allRoles = await _roleManager.Roles.ToListAsync();
            var viewModel = new CreateUserViewModel
            {
                Roles = allRoles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToList()
            };
            return View(viewModel);
        }

        /// <summary>
        /// Xử lý lưu người dùng mới vào database và gán vai trò ban đầu.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    Address = model.Address,
                    Age = model.Age
                };

                var result = await _userManager.CreateAsync(user, model.Password ?? string.Empty);

                if (result.Succeeded)
                {
                    if (model.SelectedRoles != null && model.SelectedRoles.Any())
                    {
                        await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                    }
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            var allRoles = await _roleManager.Roles.ToListAsync();
            model.Roles = allRoles.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Name,
                Selected = r.Name != null && (model.SelectedRoles?.Contains(r.Name) ?? false)
            }).ToList();

            return View(model);
        }
    }

    /// <summary>
    /// ViewModel dùng để hiển thị thông tin người dùng kèm vai trò ở trang danh sách.
    /// </summary>
    public class UserRolesViewModel
    {
        public string? UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public IEnumerable<string>? Roles { get; set; }
    }

    /// <summary>
    /// ViewModel dùng để nhận dữ liệu khi tạo tài khoản mới.
    /// </summary>
    public class CreateUserViewModel
    {
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public int? Age { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public List<SelectListItem>? Roles { get; set; }
        public List<string>? SelectedRoles { get; set; }
    }

    /// <summary>
    /// ViewModel dùng để xử lý cập nhật thông tin và đổi mật khẩu.
    /// </summary>
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public int? Age { get; set; }
        public List<SelectListItem>? Roles { get; set; }
        public List<string>? SelectedRoles { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}

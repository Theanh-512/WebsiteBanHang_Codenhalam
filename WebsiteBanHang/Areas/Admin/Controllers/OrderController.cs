using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteBanHang.Data;
using WebsiteBanHang.Models;

namespace WebsiteBanHang.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller quản lý đơn hàng cho Admin và Nhân viên.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Hiển thị danh sách toàn bộ các đơn hàng trong hệ thống.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.ApplicationUser)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        /// <summary>
        /// Xem chi tiết một đơn hàng, bao gồm các sản phẩm đã mua.
        /// </summary>
        /// <param name="id">ID của đơn hàng</param>
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        /// <summary>
        /// Cập nhật trạng thái của đơn hàng.
        /// </summary>
        /// <param name="id">ID của đơn hàng</param>
        /// <param name="status">Trạng thái mới cần chuyển sang</param>
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.OrderStatus = status;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật trạng thái thành công!";
            }
            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}

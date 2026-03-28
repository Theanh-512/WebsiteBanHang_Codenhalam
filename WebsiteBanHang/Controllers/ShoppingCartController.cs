using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebsiteBanHang.Extensions;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repositories;
using WebsiteBanHang.Data;
using System.Security.Claims;

namespace WebsiteBanHang.Controllers
{
    /// <summary>
    /// Controller xử lý các chức năng liên quan đến giỏ hàng và thanh toán.
    /// Sử dụng Session để lưu trữ thông tin giỏ hàng tạm thời.
    /// </summary>
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShoppingCartController(ApplicationDbContext context, IProductRepository productRepository, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _productRepository = productRepository;
            _userManager = userManager;
        }

        /// <summary>
        /// Hiển thị danh sách các món hàng hiện có trong giỏ hàng.
        /// </summary>
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            return View(cart.Items);
        }

        /// <summary>
        /// Thêm một sản phẩm vào giỏ hàng.
        /// </summary>
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            var product = _productRepository.GetById(productId);
            if (product == null) return NotFound();

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();

            cart.AddItem(new CartItem
            {
                ProductId = productId,
                Name = product.Name,
                Price = product.Price,
                Quantity = quantity,
                ImageUrl = product.ImageUrl
            });

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Xóa hoàn toàn một sản phẩm khỏi giỏ hàng.
        /// </summary>
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            cart.RemoveItem(productId);
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Cập nhật số lượng của một sản phẩm trong giỏ hàng.
        /// </summary>
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                item.Quantity = quantity;
                if (item.Quantity <= 0)
                {
                    cart.RemoveItem(productId);
                }
            }
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Hiển thị trang đặt hàng (Checkout). Yêu cầu người dùng phải đăng nhập.
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            if (cart == null || cart.Items.Count == 0)
            {
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            var order = new Order
            {
                ApplicationUser = user!,
                ShippingAddress = user!.Address ?? "",
            };

            return View(order);
        }

        /// <summary>
        /// Xử lý lưu đơn hàng và chi tiết đơn hàng vào database sau khi thanh toán.
        /// </summary>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order order)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            if (cart == null || cart.Items.Count == 0)
            {
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            order.UserId = user!.Id;
            order.OrderDate = DateTime.Now;
            order.OrderStatus = SD.StatusPending; // Mặc định là đang chờ
            order.TotalPrice = cart.ComputeTotalValue();
            order.OrderDetails = new List<OrderDetail>();

            // Chuyển đổi từ CartItem sang OrderDetail
            foreach (var item in cart.Items)
            {
                order.OrderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Xóa giỏ hàng sau khi đặt thành công
            HttpContext.Session.Remove("Cart");

            return View("OrderCompleted", order.Id);
        }

        /// <summary>
        /// Hiển thị thông báo sau khi đặt hàng thành công.
        /// </summary>
        public IActionResult OrderCompleted(int id)
        {
            return View(id);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using WebsiteBanHang.Repositories;

namespace WebsiteBanHang.Controllers
{
    /// <summary>
    /// Controller chính phục vụ trang chủ của Website.
    /// Hiển thị danh sách các sản phẩm nổi bật cho khách hàng.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IProductRepository _repo;

        public HomeController(IProductRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Trang chủ mặc định hiển thị toàn bộ hoặc danh sách sản phẩm.
        /// </summary>
        public IActionResult Index()
        {
            return View(_repo.GetAll());
        }
    }
}

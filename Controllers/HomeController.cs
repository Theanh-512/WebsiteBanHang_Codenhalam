using Microsoft.AspNetCore.Mvc;
using WebsiteBanHang.Repositories;

namespace WebsiteBanHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _repo;

        public HomeController(IProductRepository repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            return View(_repo.GetAll());
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repositories;

namespace WebsiteBanHang.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller quản lý danh mục sản phẩm (Admin).
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        /// <summary>
        /// Hiển thị danh sách tất cả danh mục.
        /// </summary>
        public IActionResult Index()
        {
            var categories = _categoryRepository.GetAllCategories();
            return View(categories);
        }

        /// <summary>
        /// Hiển thị form thêm danh mục mới.
        /// </summary>
        public IActionResult Add()
        {
            return View();
        }

        /// <summary>
        /// Xử lý lưu danh mục mới.
        /// </summary>
        [HttpPost]
        public IActionResult Add(Category category)
        {
            if (ModelState.IsValid)
            {
                _categoryRepository.Add(category);
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        /// <summary>
        /// Hiển thị form cập nhật danh mục.
        /// </summary>
        public IActionResult Update(int id)
        {
            var category = _categoryRepository.GetById(id);
            if (category == null) return NotFound();
            return View(category);
        }

        /// <summary>
        /// Xử lý cập nhật thông tin danh mục.
        /// </summary>
        [HttpPost]
        public IActionResult Update(Category category)
        {
            if (ModelState.IsValid)
            {
                _categoryRepository.Update(category);
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        /// <summary>
        /// Hiển thị trang xác nhận xóa danh mục.
        /// </summary>
        public IActionResult Delete(int id)
        {
            var category = _categoryRepository.GetById(id);
            if (category == null) return NotFound();
            return View(category);
        }

        /// <summary>
        /// Xử lý xóa danh mục sau khi xác nhận.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            _categoryRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}

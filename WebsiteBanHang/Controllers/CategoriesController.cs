using Microsoft.AspNetCore.Mvc;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repositories;

namespace WebsiteBanHang.Controllers
{
    /// <summary>
    /// Controller xử lý hiển thị danh mục sản phẩm phía người dùng (User Interface).
    /// </summary>
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        /// <summary>
        /// Hiển thị danh sách các danh mục sản phẩm cho người dùng.
        /// </summary>
        public IActionResult Index()
        {
            var categories = _categoryRepository.GetAllCategories();
            return View(categories);
        }

        /// <summary>
        /// Trang thêm danh mục (phía Client - nếu có phân quyền).
        /// </summary>
        public IActionResult Add()
        {
            return View();
        }

        /// <summary>
        /// Xử lý thêm danh mục mới.
        /// </summary>
        [HttpPost]
        public IActionResult Add(Category category)
        {
            if (ModelState.IsValid)
            {
                _categoryRepository.Add(category);
                return RedirectToAction("Index");
            }
            return View(category);
        }

        /// <summary>
        /// Hiển thị trang cập nhật danh mục.
        /// </summary>
        public IActionResult Update(int id)
        {
            var category = _categoryRepository.GetById(id);
            if (category == null)
            {
                return NotFound();
            }
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
                return RedirectToAction("Index");
            }
            return View(category);
        }

        /// <summary>
        /// Hiển thị trang xác nhận xóa danh mục.
        /// </summary>
        public IActionResult Delete(int id)
        {
            var category = _categoryRepository.GetById(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        /// <summary>
        /// Xác nhận xóa danh mục.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            _categoryRepository.Delete(id);
            return RedirectToAction("Index");
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repositories;

namespace WebsiteBanHang.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller quản lý tất cả sản phẩm của cửa hàng (Admin).
    /// Hỗ trợ thêm, sửa, xóa và tải lên nhiều hình ảnh cho sản phẩm.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _env;

        public ProductController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IWebHostEnvironment env)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _env = env;
        }

        /// <summary>
        /// Hiển thị danh sách tất cả sản phẩm hiện có.
        /// </summary>
        public IActionResult Index()
        {
            var products = _productRepository.GetAll();
            return View(products);
        }

        /// <summary>
        /// Hiển thị form thêm sản phẩm mới.
        /// </summary>
        public IActionResult Add()
        {
            ViewBag.Categories = new SelectList(_categoryRepository.GetAllCategories(), "Id", "Name");
            return View();
        }

        /// <summary>
        /// Xử lý dữ liệu khi thêm sản phẩm mới kèm hình ảnh.
        /// </summary>
        [HttpPost]
        public IActionResult Add(Product product)
        {
            if (ModelState.IsValid)
            {
                // Lưu ảnh chính nếu có
                if (product.ImageFile != null)
                {
                    product.ImageUrl = SaveImage(product.ImageFile);
                }

                // Lưu danh sách ảnh phụ
                if (product.ImageFiles != null && product.ImageFiles.Count > 0)
                {
                    product.ImageUrls = new List<string>();
                    foreach (var file in product.ImageFiles)
                    {
                        product.ImageUrls.Add(SaveImage(file));
                    }
                }

                _productRepository.Add(product);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(_categoryRepository.GetAllCategories(), "Id", "Name");
            return View(product);
        }

        /// <summary>
        /// Hiển thị form cập nhật thông tin sản phẩm.
        /// </summary>
        public IActionResult Update(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null) return NotFound();

            ViewBag.Categories = new SelectList(_categoryRepository.GetAllCategories(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        /// <summary>
        /// Xử lý cập nhật sản phẩm và quản lý thay đổi hình ảnh.
        /// </summary>
        [HttpPost]
        public IActionResult Update(Product product)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = _productRepository.GetById(product.Id);
                if (existingProduct == null) return NotFound();

                // Cập nhật ảnh chính mới nếu có upload lại
                if (product.ImageFile != null)
                {
                    existingProduct.ImageUrl = SaveImage(product.ImageFile);
                }

                // Cập nhật thêm các ảnh phụ mới
                if (product.ImageFiles != null && product.ImageFiles.Count > 0)
                {
                    if (existingProduct.ImageUrls == null) existingProduct.ImageUrls = new List<string>();
                    foreach (var file in product.ImageFiles)
                    {
                        existingProduct.ImageUrls.Add(SaveImage(file));
                    }
                }

                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;

                _productRepository.Update(existingProduct);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(_categoryRepository.GetAllCategories(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        /// <summary>
        /// Hiển thị trang xác nhận xóa sản phẩm (Chỉ Admin).
        /// </summary>
        public IActionResult Delete(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null) return NotFound();
            return View(product);
        }

        /// <summary>
        /// Xử lý xóa sản phẩm sau khi đã xác nhận.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            _productRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Hàm phụ hỗ trợ lưu file ảnh vào thư mục wwwroot/images.
        /// </summary>
        private string SaveImage(IFormFile image)
        {
            string folder = Path.Combine(_env.WebRootPath, "images");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            string filePath = Path.Combine(folder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                image.CopyTo(stream);
            }
            return "/images/" + fileName;
        }
    }
}

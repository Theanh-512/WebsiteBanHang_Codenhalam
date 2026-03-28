using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repositories;

namespace WebsiteBanHang.Controllers
{
    /// <summary>
    /// Controller xử lý các chức năng liên quan đến sản phẩm phía khách hàng.
    /// Bao gồm: xem danh sách, tìm kiếm, lọc theo danh mục và xem chi tiết sản phẩm.
    /// </summary>
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
        /// Trang hiển thị danh sách sản phẩm, có hỗ trợ lọc theo danh mục và tìm kiếm theo từ khóa.
        /// </summary>
        public IActionResult Index(int? categoryId, string keyword)
        {
            var products = _productRepository.GetAll();

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();
                products = products.Where(p => 
                    p.Name.ToLower().Contains(keyword) || 
                    (p.Category != null && p.Category.Name.ToLower().Contains(keyword))
                );
            }

            return View(products);
        }

        /// <summary>
        /// Hiển thị form thêm sản phẩm từ phía trang ngoài (thường dành cho người quản lý).
        /// </summary>
        public IActionResult Add()
        {
            ViewBag.Categories = new SelectList(
                _categoryRepository.GetAllCategories(),
                "Id", "Name");

            return View();
        }

        /// <summary>
        /// Xử lý lưu sản phẩm mới kèm các tệp hình ảnh.
        /// </summary>
        [HttpPost]
        public IActionResult Add(Product product)
        {
            if (ModelState.IsValid)
            {
                // Xử lý lưu ảnh chính
                if (product.ImageFile != null)
                {
                    string folder = Path.Combine(_env.WebRootPath, "images");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                    string filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        product.ImageFile.CopyTo(stream);
                    }
                    product.ImageUrl = "/images/" + fileName;
                }

                // Xử lý lưu danh sách ảnh mô tả
                if (product.ImageFiles != null && product.ImageFiles.Count > 0)
                {
                    string folder = Path.Combine(_env.WebRootPath, "images");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    if (product.ImageUrls == null)
                        product.ImageUrls = new List<string>();

                    foreach (var file in product.ImageFiles)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string filePath = Path.Combine(folder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        string url = "/images/" + fileName;
                        product.ImageUrls.Add(url);

                        if (string.IsNullOrEmpty(product.ImageUrl))
                        {
                            product.ImageUrl = url;
                        }
                    }
                }

                _productRepository.Add(product);
                return RedirectToAction("Index");
            }

            ViewBag.Categories = new SelectList(
                _categoryRepository.GetAllCategories(),
                "Id", "Name");

            return View(product);
        }

        /// <summary>
        /// Xem chi tiết thông tin một sản phẩm.
        /// </summary>
        public IActionResult Display(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        /// <summary>
        /// Hiển thị form cập nhật sản phẩm.
        /// </summary>
        public IActionResult Update(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
                return NotFound();

            ViewBag.Categories = new SelectList(
                _categoryRepository.GetAllCategories(),
                "Id",
                "Name",
                product.CategoryId);

            return View(product);
        }

        /// <summary>
        /// Xử lý cập nhật thông tin sản phẩm và quản lý hình ảnh.
        /// </summary>
        [HttpPost]
        public IActionResult Update(Product product)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(
                    _categoryRepository.GetAllCategories(),
                    "Id",
                    "Name",
                    product.CategoryId);

                return View(product);
            }

            var existingProduct = _productRepository.GetById(product.Id);
            if (existingProduct == null)
                return NotFound();

            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Description = product.Description;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.DeleteAllImages = product.DeleteAllImages;

            // Xóa hết ảnh nếu người dùng chọn checkbox xóa
            if (product.DeleteAllImages)
            {
                existingProduct.ImageUrl = null;
                if (existingProduct.ImageUrls != null)
                {
                    existingProduct.ImageUrls.Clear();
                }
            }

            // Upload ảnh chính mới
            if (product.ImageFile != null)
            {
                string folder = Path.Combine(_env.WebRootPath, "images");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    product.ImageFile.CopyTo(stream);
                }
                existingProduct.ImageUrl = "/images/" + fileName;
            }

            // Upload thêm các ảnh phụ
            if (product.ImageFiles != null && product.ImageFiles.Count > 0)
            {
                string folder = Path.Combine(_env.WebRootPath, "images");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                if (existingProduct.ImageUrls == null)
                    existingProduct.ImageUrls = new List<string>();

                foreach (var file in product.ImageFiles)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    string url = "/images/" + fileName;
                    existingProduct.ImageUrls.Add(url);

                    if (string.IsNullOrEmpty(existingProduct.ImageUrl))
                    {
                        existingProduct.ImageUrl = url;
                    }
                }
            }

            _productRepository.Update(existingProduct);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Quản lý cập nhật sản phẩm đại trà.
        /// </summary>
        public IActionResult ManageUpdate()
        {
            var products = _productRepository.GetAll();
            return View(products);
        }

        /// <summary>
        /// Trang nội dung xóa sản phẩm.
        /// </summary>
        public IActionResult Delete(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        /// <summary>
        /// Trang quản lý xóa sản phẩm hàng loạt.
        /// </summary>
        public IActionResult ManageDelete()
        {
            var products = _productRepository.GetAll();
            return View(products);
        }

        /// <summary>
        /// Xác nhận xóa sản phẩm khỏi hệ thống.
        /// </summary>
        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            _productRepository.Delete(id);
            return RedirectToAction("ManageDelete");
        }
    }
}

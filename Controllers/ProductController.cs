using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repositories;

namespace WebsiteBanHang.Controllers
{
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


        public IActionResult Index(int? categoryId)
        {
            var products = _productRepository.GetAll();

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            return View(products);
        }

        public IActionResult Add()
        {
            ViewBag.Categories = new SelectList(
                _categoryRepository.GetAllCategories(),
                "Id", "Name");

            return View();
        }

        [HttpPost]
        public IActionResult Add(Product product)
        {
            if (ModelState.IsValid)
            {
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


        public IActionResult Display(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

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

            if (product.DeleteAllImages)
            {
                existingProduct.ImageUrl = null;
                if (existingProduct.ImageUrls != null)
                {
                    existingProduct.ImageUrls.Clear();
                }
            }

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
                    // Khi update sẽ cộng dồn ảnh mô tả
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

        public IActionResult ManageUpdate()
        {
            var products = _productRepository.GetAll();
            return View(products);
        }

        public IActionResult Delete(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
                return NotFound();

            return View(product);
        }
        public IActionResult ManageDelete()
        {
            var products = _productRepository.GetAll();
            return View(products);
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            _productRepository.Delete(id);
            return RedirectToAction("ManageDelete");
        }

    }


}

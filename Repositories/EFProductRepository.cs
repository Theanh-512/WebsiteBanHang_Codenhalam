using WebsiteBanHang.Data;
using WebsiteBanHang.Models;
using Microsoft.EntityFrameworkCore;

namespace WebsiteBanHang.Repositories
{
    public class EFProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public EFProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Product> GetAll()
        {
            return _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .ToList();
        }

        public Product GetById(int id)
        {
            return _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefault(p => p.Id == id);
        }

        public void Add(Product product)
        {
            _context.Products.Add(product);

            if (product.ImageUrls != null && product.ImageUrls.Any())
            {
                foreach (var url in product.ImageUrls)
                {
                    _context.ProductImages.Add(new ProductImage
                    {
                        Url = url,
                        Product = product
                    });
                }
            }

            _context.SaveChanges();
        }

        public void Update(Product product)
        {
            var existing = _context.Products
                .Include(p => p.Images)
                .FirstOrDefault(p => p.Id == product.Id);

            if (existing != null)
            {
                existing.Name = product.Name;
                existing.Price = product.Price;
                existing.Description = product.Description;
                existing.CategoryId = product.CategoryId;
                existing.ImageUrl = product.ImageUrl;

                if (product.DeleteAllImages)
                {
                    if (existing.Images != null && existing.Images.Any())
                    {
                        _context.ProductImages.RemoveRange(existing.Images);
                    }
                }

                if (product.ImageUrls != null && product.ImageUrls.Any())
                {
                    foreach (var url in product.ImageUrls)
                    {
                        _context.ProductImages.Add(new ProductImage
                        {
                            Url = url,
                            ProductId = product.Id
                        });
                    }
                }

                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
        }
    }
}

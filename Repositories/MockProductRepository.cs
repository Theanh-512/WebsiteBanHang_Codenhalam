using WebsiteBanHang.Models;

namespace WebsiteBanHang.Repositories
{
    public class MockProductRepository : IProductRepository
    {
        private readonly List<Product> _products;

        public MockProductRepository()
        {
            _products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Iphone 15 Pro Max",
                    Price = 35000000,
                    Description = "Apple A17 Pro, 256GB",
                    ImageUrl = "/images/Iphone 15 Pro Max.jpg",
                    CategoryId = 1
                },
                new Product
                {
                    Id = 2,
                    Name = "Samsung Galaxy S24",
                    Price = 21990000,
                    Description = "Snapdragon 8 Gen 3",
                    ImageUrl = "/images/Samsung Galaxy S24.jpg",
                    CategoryId = 1
                }
            };
        }

        public IEnumerable<Product> GetAll()
        {
            return _products;
        }

        public Product GetById(int id)
        {
            return _products.FirstOrDefault(p => p.Id == id);
        }

        public void Add(Product product)
        {
            if (_products.Any())
                product.Id = _products.Max(p => p.Id) + 1;
            else
                product.Id = 1;

            _products.Add(product);
        }

        public void Update(Product product)
        {
            var existing = GetById(product.Id);
            if (existing == null) return;

            existing.Name = product.Name;
            existing.Price = product.Price;
            existing.Description = product.Description;
            existing.ImageUrl = product.ImageUrl;
            if (product.ImageUrls != null && product.ImageUrls.Count > 0)
            {
                existing.ImageUrls = product.ImageUrls;
            }
            existing.CategoryId = product.CategoryId;
        }

        public void Delete(int id)
        {
            var product = GetById(id);
            if (product != null)
                _products.Remove(product);
        }
    }
}

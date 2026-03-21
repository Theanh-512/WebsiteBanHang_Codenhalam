using WebsiteBanHang.Models;

namespace WebsiteBanHang.Repositories
{
    public class MockCategoryRepository : ICategoryRepository
    {
        private List<Category> _categories;

        public MockCategoryRepository()
        {
            _categories = new List<Category>
            {
                new Category { Id = 1, Name = "Điện thoại" },
                new Category { Id = 2, Name = "Laptop" },
                new Category { Id = 3, Name = "Phụ kiện" },
                new Category { Id = 4, Name = "Tai nghe" },
                new Category { Id = 5, Name = "Đồng hồ" },
                new Category { Id = 6, Name = "Loa" },
                new Category { Id = 7, Name = "Máy ảnh" },
            };
        }

        public IEnumerable<Category> GetAllCategories()
        {
            return _categories;
        }

        public Category GetById(int id)
        {
            return _categories.FirstOrDefault(c => c.Id == id);
        }

        public void Add(Category category)
        {
            category.Id = _categories.Any() ? _categories.Max(c => c.Id) + 1 : 1;
            _categories.Add(category);
        }

        public void Update(Category category)
        {
            var existing = GetById(category.Id);
            if (existing != null)
            {
                existing.Name = category.Name;
            }
        }

        public void Delete(int id)
        {
            var category = GetById(id);
            if (category != null)
                _categories.Remove(category);
        }
    }
}

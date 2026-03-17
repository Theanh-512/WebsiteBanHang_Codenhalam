using Microsoft.AspNetCore.Mvc;
using WebsiteBanHang.Repositories;

namespace WebsiteBanHang.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryMenuViewComponent(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public IViewComponentResult Invoke()
        {
            var categories = _categoryRepository.GetAllCategories();
            return View(categories);
        }
    }
}

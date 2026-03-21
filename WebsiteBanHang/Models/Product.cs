using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace WebsiteBanHang.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Range(1, 10000000000)]
        public decimal Price { get; set; }

        public string Description { get; set; }

        // Lưu đường dẫn ảnh chính
        public string? ImageUrl { get; set; }

        // Lưu danh sách đường dẫn ảnh (nhiều ảnh)
        public List<string>? ImageUrls { get; set; } = new List<string>();

        // Liên kết 1-nhiều với ProductImage trong DB
        public List<ProductImage>? Images { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // Upload file ảnh chính từ máy (giữ lại nếu cần)
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        // Upload nhiều file từ máy
        [NotMapped]
        public List<IFormFile>? ImageFiles { get; set; }

        [NotMapped]
        public bool DeleteAllImages { get; set; }
    }

}

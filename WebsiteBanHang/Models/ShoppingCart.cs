using System.Collections.Generic;
using System.Linq;

namespace WebsiteBanHang.Models
{
    public class ShoppingCart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public void AddItem(CartItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existingItem == null)
            {
                Items.Add(item);
            }
            else
            {
                existingItem.Quantity += item.Quantity;
            }
        }

        public void RemoveItem(int productId)
        {
            Items.RemoveAll(i => i.ProductId == productId);
        }

        public decimal ComputeTotalValue()
        {
            return Items.Sum(i => i.Price * i.Quantity);
        }

        public void Clear()
        {
            Items.Clear();
        }
    }
}

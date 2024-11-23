using Shop.DAL.Models;

namespace Shop.ViewModels.Wrappers
{
    public class CartItem
    {
        // Свойства
        public Product Product { get; set; }  // Продукт, который добавлен в корзину
        public decimal Price { get; set; }    // Цена за единицу товара
        public int Quantity { get; set; }     // Количество товара в корзине

        // Свойство для расчёта общей стоимости
        public decimal TotalPrice => Price * Quantity;

        // Конструктор
        public CartItem(Product product, decimal price, int quantity)
        {
            Product = product ?? throw new ArgumentNullException(nameof(product));
            Price = price;
            Quantity = quantity;
        }
    }
}

using DAL.Entities.Interfaces;

namespace DAL.Entities
{
    public class StoreInventory
    {
        public int StoreId { get; set; }
        public int ProductId { get; set; }

        // Цена товара в магазине
        public decimal Price { get; set; }

        // Количество товара в магазине
        public int Quantity { get; set; }

        // Навигационные свойства
        public Store Store { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }

}

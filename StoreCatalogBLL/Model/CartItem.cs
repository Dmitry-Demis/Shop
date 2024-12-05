using StoreCatalogDAL.Model;

namespace StoreCatalogBLL.Model;

public class CartItem
{
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public int Quantity { get; set; }
    public decimal Total => Quantity * Price;
}

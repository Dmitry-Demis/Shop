using StoreCatalogBLL.Model;

namespace StoreCatalogPresentation.Models;

public class CartItemWrapper(CartItem cartItem) : Wrapper<CartItem>(cartItem)
{
    public string Name => Base.Name;

    public int Quantity
    {
        get => Base.Quantity;
        set => Set(() => Base.Quantity, v => Base.Quantity = value, value);
    }
    public decimal Price => Base.Price;
    public decimal Total => Base.Total;
}
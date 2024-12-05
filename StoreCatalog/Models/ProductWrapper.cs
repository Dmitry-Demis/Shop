using StoreCatalogDAL.Model;

namespace StoreCatalogPresentation.Models;

public class ProductWrapper(Product product) : Wrapper<Product>(product)
{
    public string Name
    {
        get => Base.Name;
        set => Set(() => Base.Name, v => Base.Name = v, value);
    }

    public int StoreId
    {
        get => Base.StoreId;
        set => Set(() => Base.StoreId, v => Base.StoreId = v, value);
    }

    public int Quantity
    {
        get => Base.Quantity;
        set => Set(() => Base.Quantity, v => Base.Quantity = v, value);
    }

    private int _selectedQuantity = 0;
    public int SelectedQuantity
    {
        get => _selectedQuantity;
        set
        {
            if (value <= Quantity)
                Set(ref _selectedQuantity, value);
        }
    }

    public decimal Price
    {
        get => Base.Price;
        set => Set(() => Base.Price, v => Base.Price = v, value);
    }
}
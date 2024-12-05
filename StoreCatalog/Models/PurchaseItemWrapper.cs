using StoreCatalogBLL.Model;
namespace StoreCatalogPresentation.Models;
public class PurchaseItemWrapper(PurchaseItem purchaseItem) : Wrapper<PurchaseItem>(purchaseItem)
{
    public string Name
    {
        get => Base.Name;
        set => Set(() => Base.Name, v => Base.Name = v, value);
    }

    public int Quantity
    {
        get => Base.Quantity;
        set => Set(() => Base.Quantity, v => Base.Quantity = v, value);
    }
}
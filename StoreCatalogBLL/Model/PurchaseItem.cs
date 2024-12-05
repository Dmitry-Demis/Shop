namespace StoreCatalogBLL.Model;

public class PurchaseItem
{
    public string Name { get; set; } = null!;

    private int _quantity;
    public int Quantity
    {
        get => _quantity;
        set => _quantity = value < 0 ? 0 : value;
    }
    public override string ToString() => $"{Name} — {Quantity}";
}
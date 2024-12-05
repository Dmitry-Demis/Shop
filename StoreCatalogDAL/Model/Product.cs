namespace StoreCatalogDAL.Model
{
    public class Product : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int StoreId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public override string ToString() => $"{Name} — {Quantity} — {Price} ₽";
    }
}

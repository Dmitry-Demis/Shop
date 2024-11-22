namespace Shop.DAL.Models
{
    public class Product : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public List<StoreInventory> StoreInventories { get; set; } = [];

        public override string ToString() => Name;
    }
}

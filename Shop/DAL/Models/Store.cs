namespace Shop.DAL.Models
{
    public class Store : IEntity
    {
        public int Id { get; set; }
        public string Code { get; private set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!; 
        public List<StoreInventory> StoreInventories { get; set; } = [];
        public override string ToString() => $"{Code}: {Name}, {Address}";
    }
}

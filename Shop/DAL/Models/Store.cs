namespace Shop.DAL.Models
{
    public class Store : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;  // Инициализируем строку с null!
        public string Address { get; set; } = null!;  // Инициализируем строку с null!
        public List<StoreInventory> StoreInventories { get; set; } = [];  // Инициализация пустым списком
        public override string ToString() => $"{Name} — {Address}";
    }
}

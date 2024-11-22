using DAL.Entities.Interfaces;

namespace DAL.Entities
{
    public class Store : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;

        public List<StoreInventory> Inventory { get; set; } = [];
    }
}

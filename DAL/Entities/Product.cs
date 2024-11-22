namespace DAL.Entities.Interfaces
{
    public class Product : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}

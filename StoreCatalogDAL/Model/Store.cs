namespace StoreCatalogDAL.Model
{
    public class Store : IEntity
    {
        public int Id { get; set; }
        public string Code { get; private set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public override string ToString() => $"{Name}, {Address}";
    }
}

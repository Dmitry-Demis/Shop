namespace Shop.DAL.Models
{
    public interface IEntity
    {
        int Id { get; set; } // Уникальный идентификатор
        string Name { get; set; }
    }
}

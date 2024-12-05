using Microsoft.EntityFrameworkCore;
using StoreCatalogDAL.Model;

namespace StoreCatalogDAL.Storage
{
    public class StoreDbContext(DbContextOptions<StoreDbContext> options) : DbContext(options)
    {
        public DbSet<Store> Stores { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Устанавливаем связь один ко многим: один магазин может иметь много продуктов
            modelBuilder.Entity<Product>()
                .HasOne<Store>() // Каждый продукт связан с одним магазином
                .WithMany() // Магазин не содержит коллекции продуктов
                .HasForeignKey(p => p.StoreId) // Внешний ключ в продукте
                .OnDelete(DeleteBehavior.Cascade); // Если магазин удаляется, все его товары удаляются

            // Уникальные индексы
            modelBuilder.Entity<Store>()
                .HasIndex(s => s.Code)
                .IsUnique();

            // Изменяем индекс для продукта на уникальную комбинацию Name и StoreId
            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.Name, p.StoreId })
                .IsUnique();
        }

    }
}

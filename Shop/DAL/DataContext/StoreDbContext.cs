using Microsoft.EntityFrameworkCore;
using Shop.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop.DAL.DataContext
{
    public class StoreDbContext(DbContextOptions<StoreDbContext> options) : DbContext(options)
    {
        public DbSet<Store> Stores { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<StoreInventory> StoreInventories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Конфигурация для Store и StoreInventory
            modelBuilder.Entity<StoreInventory>()
                .HasKey(si => new { si.StoreId, si.ProductId }); // Составной ключ для StoreInventory

            modelBuilder.Entity<StoreInventory>()
                .HasOne(si => si.Store)
                .WithMany(s => s.StoreInventories)
                .HasForeignKey(si => si.StoreId);

            modelBuilder.Entity<StoreInventory>()
                .HasOne(si => si.Product)
                .WithMany(p => p.StoreInventories)
                .HasForeignKey(si => si.ProductId);

            // Уникальный индекс для поля Name в Product
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name)
                .IsUnique();

        }
    }
}

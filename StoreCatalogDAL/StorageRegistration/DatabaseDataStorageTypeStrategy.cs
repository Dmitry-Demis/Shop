using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StoreCatalogDAL.Model;
using StoreCatalogDAL.Storage;

namespace StoreCatalogDAL.StorageRegistration
{
    public class DatabaseDataStorageTypeStrategy : IDataStorageTypeStrategy
    {
        public void RegisterRepositories(IServiceCollection services, IConfiguration configuration)
        {
            var basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\.."));
            var dbSettings = configuration.GetSection("DatabaseSettings");
            var databasePath = dbSettings["DatabasePath"]
                               ?? throw new InvalidOperationException("DatabasePath is missing in the configuration.");
            var connectionString = dbSettings["ConnectionString"]
                                   ?? throw new InvalidOperationException("ConnectionString is missing in the configuration.");
            
            var fullDbPath = Path.Combine(basePath, databasePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullDbPath)!);

            services
                .AddDbContext<StoreDbContext>(options =>
                options.UseSqlite(connectionString.Replace(databasePath, fullDbPath)))
                .AddScoped<IRepository<Store>, DbRepository<Store>>()
                .AddScoped<IRepository<Product>, DbRepository<Product>>()
                ;
        }
    }

}

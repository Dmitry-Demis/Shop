using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shop.DAL.DataContext;
using Shop.DAL.Models;
using Shop.DAL.Repositories;
using System.IO;

namespace Shop.DAL.Services.DataAccess
{
    public class DatabaseDataAccessStrategy : IDataAccessStrategy
    {
        public void RegisterRepositories(IServiceCollection services, IConfiguration configuration)
        {
            // Получаем путь к корню проекта
            var basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\.."));

            // Получаем настройки базы данных
            var dbSettings = configuration.GetSection("DatabaseSettings");
            var databasePath = dbSettings["DatabasePath"]
                               ?? throw new InvalidOperationException("DatabasePath is missing in the configuration.");
            var connectionString = dbSettings["ConnectionString"]
                                   ?? throw new InvalidOperationException("ConnectionString is missing in the configuration.");

            // Формируем абсолютный путь к базе данных
            var fullDbPath = Path.Combine(basePath, databasePath);

            // Убедимся, что каталог для базы данных существует
            Directory.CreateDirectory(Path.GetDirectoryName(fullDbPath)!);

            // Регистрируем DbContext с обновлённой строкой подключения
            services.AddDbContext<StoreDbContext>(options =>
                options.UseSqlite(connectionString.Replace(databasePath, fullDbPath)));

            // Регистрируем репозитории
            services.AddScoped<IRepository<Store>, DbRepository<Store>>();
            services.AddScoped<IRepository<Product>, DbRepository<Product>>();
        }
    }
}

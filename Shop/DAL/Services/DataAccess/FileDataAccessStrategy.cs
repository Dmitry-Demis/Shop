using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shop.DAL.Models;
using Shop.DAL.Repositories;
using System.IO;

namespace Shop.DAL.Services.DataAccess
{
    public class FileDataAccessStrategy : IDataAccessStrategy
    {
        public void RegisterRepositories(IServiceCollection services, IConfiguration configuration)
        {
            // Получаем путь к корню проекта
            var basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\.."));

            // Получаем настройки из конфигурации
            var fileSettings = configuration.GetSection("FileSettings") ?? throw new InvalidOperationException("FileSettings section is missing in configuration.");
            var shopFilePath = fileSettings["ShopFilePath"] ?? throw new InvalidOperationException("ShopFilePath is missing in the configuration.");
            var productFilePath = fileSettings["ProductFilePath"] ?? throw new InvalidOperationException("ProductFilePath is missing in the configuration.");
            var storeInventoryFilePath = fileSettings["StoreInventoryFilePath"] ?? throw new InvalidOperationException("StoreInventoryFilePath is missing in the configuration.");

            // Формируем абсолютные пути относительно корня проекта
            var fullShopFilePath = Path.Combine(basePath, shopFilePath);
            var fullProductFilePath = Path.Combine(basePath, productFilePath);
            var fullStoreInventoryFilePath = Path.Combine(basePath, storeInventoryFilePath);

            // Проверяем и создаём директории, если они не существуют
            Directory.CreateDirectory(Path.GetDirectoryName(fullShopFilePath)!);
            Directory.CreateDirectory(Path.GetDirectoryName(fullProductFilePath)!);
            Directory.CreateDirectory(Path.GetDirectoryName(fullStoreInventoryFilePath)!);

            // Проверяем наличие файлов и создаём их, если они не существуют
            //CreateFileIfNotExists(fullShopFilePath);
            //CreateFileIfNotExists(fullProductFilePath);
            //CreateFileIfNotExists(fullStoreInventoryFilePath);

            // Регистрируем репозитории для Store, Product и StoreInventory
            services.AddSingleton<IRepository<Store>>(new FileRepository<Store>(fullShopFilePath));
            services.AddSingleton<IRepository<Product>>(new FileRepository<Product>(fullProductFilePath));
        }

        // Метод для создания файла, если его нет, с заданным содержимым
        private static void CreateFileIfNotExists(string filePath, string defaultContent)
        {
            if (!File.Exists(filePath))
            {
                // Создаём файл с пустым содержимым или с дефолтным содержимым
                File.WriteAllText(filePath, defaultContent);
                Console.WriteLine($"File created: {filePath}");
            }
        }

    }
}

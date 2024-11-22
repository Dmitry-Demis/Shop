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
            var fileSettings = configuration.GetSection("FileSettings") ?? throw new InvalidOperationException("FileSettings section is missing in configuration.");
            var shopFilePath = fileSettings["ShopFilePath"] ?? throw new InvalidOperationException("ShopFilePath is missing in the configuration.");
            var productFilePath = fileSettings["ProductFilePath"] ?? throw new InvalidOperationException("ProductFilePath is missing in the configuration.");

            // Проверяем и создаём каталоги
            EnsureDirectoryExists(shopFilePath);
            EnsureDirectoryExists(productFilePath);

            services.AddSingleton<IRepository<Store>>(new FileRepository<Store>(shopFilePath));
            services.AddSingleton<IRepository<Product>>(new FileRepository<Product>(productFilePath));
        }

        private static void EnsureDirectoryExists(string filePath)
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            if (directoryPath != null && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Console.WriteLine($"Directory created: {directoryPath}");
            }
        }
    }

}

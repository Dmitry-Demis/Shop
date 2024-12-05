using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StoreCatalogDAL.Model;
using StoreCatalogDAL.Storage;

namespace StoreCatalogDAL.StorageRegistration
{
    public class FileDataStorageTypeStrategy : IDataStorageTypeStrategy
    {
        public void RegisterRepositories(IServiceCollection services, IConfiguration configuration)
        {
            // Получаем путь к корню проекта
            var basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\.."));

            // Получаем настройки из конфигурации
            var fileSettings = configuration.GetSection("FileSettings") ?? throw new InvalidOperationException("FileSettings section is missing in configuration.");
            var shopFilePath = fileSettings["ShopFilePath"] ?? throw new InvalidOperationException("ShopFilePath is missing in the configuration.");
            var productFilePath = fileSettings["ProductFilePath"] ?? throw new InvalidOperationException("ProductFilePath is missing in the configuration.");

            // Формируем абсолютные пути относительно корня проекта
            var fullShopFilePath = Path.Combine(basePath, shopFilePath);
            var fullProductFilePath = Path.Combine(basePath, productFilePath);

            // Проверяем и создаём директории, если они не существуют
            Directory.CreateDirectory(Path.GetDirectoryName(fullShopFilePath)!);
            Directory.CreateDirectory(Path.GetDirectoryName(fullProductFilePath)!);

            services.AddSingleton<IRepository<Store>>(
                new FileRepository<Store>(fullShopFilePath));
            services.AddSingleton<IRepository<Product>>(new FileRepository<Product>(fullProductFilePath));
        }
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StoreCatalogDAL.StorageRegistration
{
    public static class StorageRegistration
    {
        public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
        {
            var mode = configuration["DataAccessMode"] ?? throw new InvalidOperationException("DataAccessMode is missing or empty in the configuration.");

            _ = mode.ToLowerInvariant() switch
            {
                "database" => services.AddSingleton<IDataStorageTypeStrategy, DatabaseDataStorageTypeStrategy>(),
                "file" => services.AddSingleton<IDataStorageTypeStrategy, FileDataStorageTypeStrategy>(),
                _ => throw new InvalidOperationException($"Unsupported data access mode: {mode}")
            };

            var serviceProvider = services.BuildServiceProvider();
            var dataAccessStrategy = serviceProvider.GetRequiredService<IDataStorageTypeStrategy>();
            dataAccessStrategy.RegisterRepositories(services, configuration);
            return services;
        }
    }

}

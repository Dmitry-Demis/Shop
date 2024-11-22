using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shop.DAL.Services.DataAccess
{
    public static class ServiceExtensions
    {
        // Метод расширения для IServiceCollection
        public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
        {
            // Получаем режим доступа из конфигурации, используя null-условный оператор
            var mode = configuration["DataAccessMode"] ?? throw new InvalidOperationException("DataAccessMode is missing or empty in the configuration.");

            // Регистрируем стратегию в зависимости от выбранного режима с использованием switch выражения
            _ = mode.ToLowerInvariant() switch
            {
                "database" => services.AddSingleton<IDataAccessStrategy, DatabaseDataAccessStrategy>(),
                "file" => services.AddSingleton<IDataAccessStrategy, FileDataAccessStrategy>(),
                _ => throw new InvalidOperationException($"Unsupported data access mode: {mode}")
            };

            // Регистрация репозиториев через выбранную стратегию
            var serviceProvider = services.BuildServiceProvider();
            var dataAccessStrategy = serviceProvider.GetRequiredService<IDataAccessStrategy>();
            dataAccessStrategy.RegisterRepositories(services, configuration);

            return services;
        }
    }
}

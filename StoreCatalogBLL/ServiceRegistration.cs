using Microsoft.Extensions.DependencyInjection;

namespace StoreCatalogBLL
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddServices(this IServiceCollection services) => services
           .AddScoped<StoreService>()
           .AddScoped<ProductService>()
           .AddScoped<CartService>()
            ;
    }
}

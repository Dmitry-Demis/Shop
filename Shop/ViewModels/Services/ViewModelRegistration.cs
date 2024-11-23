using Microsoft.Extensions.DependencyInjection;

namespace Shop.ViewModels.Services
{
    static class ViewModelRegistration
    {
        public static IServiceCollection AddViewModels(this IServiceCollection services) => services
           .AddScoped<MainWindowViewModel>()
           .AddScoped<CreateStoreViewModel>()
           .AddScoped<CreateProductViewModel>()
           .AddScoped<StockProductViewModel>()
           .AddTransient<SearchViewModel>()
            ;
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace Shop.ViewModels.Services
{
    static class ViewModelRegistration
    {
        public static IServiceCollection AddViewModels(this IServiceCollection services) => services
           .AddScoped<MainWindowViewModel>()
           .AddTransient<CreateStoreViewModel>()
           .AddTransient<CreateProductViewModel>()
           .AddTransient<StockProductViewModel>()
           .AddTransient<SearchViewModel>()
            ;
    }
}

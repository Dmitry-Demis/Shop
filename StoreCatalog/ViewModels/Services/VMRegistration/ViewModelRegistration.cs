using Microsoft.Extensions.DependencyInjection;

namespace StoreCatalogPresentation.ViewModels.Services.VMRegistration
{
    internal static class ViewModelRegistration
    {
        public static IServiceCollection AddViewModels(this IServiceCollection services) => services
           .AddScoped<MainWindowViewModel>()
           .AddTransient<CreateStoreViewModel>()
           .AddTransient<CreateProductViewModel>()
           .AddTransient<CartViewModel>()
           .AddTransient<PurchaseViewModel>()
            ;
    }
}

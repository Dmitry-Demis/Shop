using Microsoft.Extensions.DependencyInjection;

namespace Shop.ViewModels
{
    public class ViewModelLocator
    {
        public MainWindowViewModel MainWindowViewModel
            => App.Services.GetRequiredService<MainWindowViewModel>();
        public CreateStoreViewModel CreateStoreViewModel
            => App.Services.GetRequiredService<CreateStoreViewModel>();
        public CreateProductViewModel CreateProductViewModel
            => App.Services.GetRequiredService<CreateProductViewModel>();

        public StockProductViewModel StockProductViewModel
             => App.Services.GetRequiredService<StockProductViewModel>();

        public SearchViewModel SearchViewModel
     => App.Services.GetRequiredService<SearchViewModel>();
    }
}

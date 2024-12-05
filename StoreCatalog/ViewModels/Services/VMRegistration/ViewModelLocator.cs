using Microsoft.Extensions.DependencyInjection;

namespace StoreCatalogPresentation.ViewModels.Services.VMRegistration
{
    public class ViewModelLocator
    {
        private static T Resolve<T>() where T : class
            => App.Services.GetRequiredService<T>();

        public static CreateProductViewModel CreateProductViewModel => Resolve<CreateProductViewModel>();
        public static CreateStoreViewModel CreateStoreViewModel => Resolve<CreateStoreViewModel>();
        public static MainWindowViewModel MainWindowViewModel => Resolve<MainWindowViewModel>();
        public static PurchaseViewModel PurchaseViewModel => Resolve<PurchaseViewModel>();
        public static CartViewModel CartViewModel => Resolve<CartViewModel>();
    }
}

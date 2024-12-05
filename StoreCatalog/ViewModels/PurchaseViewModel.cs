using MathCore.ViewModels;
using MathCore.WPF.Commands;
using StoreCatalogBLL;
using StoreCatalogBLL.Model;
using StoreCatalogPresentation.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using StoreCatalogPresentation.ViewModels.Services.Dialogs;

namespace StoreCatalogPresentation.ViewModels
{
    public class PurchaseViewModel : ViewModel
    {
        private readonly ProductService _productService;
        private readonly StoreService _storeService;
        private readonly IUserDialogService _userDialog;
        public string Title { get; init; } = "Купить партию товаров";

        public PurchaseViewModel(
            ProductService productService
            , StoreService storeService
            , IUserDialogService userDialog)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _storeService = storeService ?? throw new ArgumentNullException(nameof(storeService));
            _userDialog = userDialog ?? throw new ArgumentNullException(nameof(userDialog)); ;

            _ = LoadProductsAsync();
        }

        public ObservableCollection<PurchaseItemWrapper> PurchaseItems { get; set; } = [];

        private async Task LoadProductsAsync()
        {
            try
            {
                // Загружаем имена продуктов из сервиса
                var productNames = await _productService.LoadProductNamesAsync();

                // Создаём новую коллекцию обёрток PurchaseItemWrapper
                var purchaseItems = productNames
                    .Select(name => new PurchaseItemWrapper(new PurchaseItem { Name = name }))
                    .ToList();

                // Обновляем ObservableCollection через метод ReplaceRange для минимизации изменений
                PurchaseItems.Clear();
                PurchaseItems.AddRange(purchaseItems);
            }
            catch (Exception ex)
            {
                // Обработка ошибок при загрузке
                Console.WriteLine($"Ошибка при загрузке продуктов: {ex.Message}");
            }
        }


        private ICommand? _purchaseCommand;
        public ICommand PurchaseCommand => _purchaseCommand ??= new LambdaCommandAsync(OnPurchaseExecutedAsync, CanPurchaseExecute);

        private bool CanPurchaseExecute() =>
            PurchaseItems.Any(item => item.Quantity > 0);

        private async Task OnPurchaseExecutedAsync()
        {
            try
            {
                var purchaseItems = PurchaseItems
                    .Where(item => item.Quantity > 0)
                    .Select(p => p.Base);

                var result = await _productService.FindCheapestStoreAsync(purchaseItems);
                
                if (result is { StoreId: not null, Cost: not null })
                {
                    // Загрузка информации о магазине
                    var store = await _storeService.LoadStoreByIdAsync(result.StoreId.Value);

                    if (store != null)
                    {
                        _userDialog.ShowInformation(
                            $"Самый дешёвый магазин: {store}. Итоговая стоимость: {result.Cost:C}.");
                    }
                    else
                    {
                        _userDialog.ShowError("Магазин не найден.");
                    }
                }
                else
                {
                    _userDialog.ShowWarning("Нет доступных магазинов, которые могут удовлетворить запрос.");
                }

            }
            catch (Exception ex)
            {
                _userDialog.ShowError($"Ошибка при покупке товаров: {ex.Message}");
            }
            finally
            {
                _userDialog.Close();
            }
        }
    }
}

using MathCore.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Input;
using StoreCatalogBLL;
using StoreCatalogDAL.Model;
using StoreCatalogPresentation.Models;
using StoreCatalogPresentation.ViewModels.Services.Dialogs;
using MathCore.WPF.Commands;

namespace StoreCatalogPresentation.ViewModels
{
    public class CreateProductViewModel : ViewModel
    {
        private readonly StoreService _storeService;
        private readonly ProductService _productService;
        private readonly IUserDialogService _userDialog;
        public string Title { get; init; } = "Завести товар";
        public CreateProductViewModel(
            StoreService storeService
            , ProductService productService
            , IUserDialogService userDialog)
        {
            _storeService = storeService ?? throw new ArgumentNullException(nameof(storeService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _userDialog = userDialog ?? throw new ArgumentNullException(nameof(userDialog));

            _ = LoadStoresAsync();
        }

        public ObservableCollection<StoreWrapper> Stores { get; set; } = [];
        private StoreWrapper? _selectedStore;
        public StoreWrapper? SelectedStore
        {
            get => _selectedStore;
            set
            {
                if (null != value && Set(ref _selectedStore, value)) 
                {
                    _ = LoadProductsByStoreAsync(value.Id);
                }
            }
        }
        
        public ObservableCollection<ProductWrapper> FilteredProducts { get; set; } = [];
        private ProductWrapper? _selectedProduct = new(new Product());
        public ProductWrapper? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (Set(ref _selectedProduct, value))
                {
                }
            }
        }


        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && Set(ref _searchText, value))
                {
                    UpdateSelectedProduct(value);
                }
            }
        }
        private void UpdateSelectedProduct(string searchText)
        {
            if (SelectedStore == null || string.IsNullOrWhiteSpace(searchText))
            {
                SelectedProduct = null; // No store selected, clear product selection
                return;
            }

            // Filter products for the selected store
            var storeProducts = FilteredProducts
                .Where(fp => fp.StoreId == SelectedStore.Id)
                .Select(fp => fp.Base)
                .ToList();

            // Update the selected product if found or create a new one
            var updatedProduct = ProductService.UpdateSelectedProduct(searchText, storeProducts);

            SelectedProduct = updatedProduct != null ? new ProductWrapper(updatedProduct) : new ProductWrapper(new Product());
        }

        private ICommand? _addNewProductCommand;
        public ICommand AddNewProductCommand => _addNewProductCommand ??= new LambdaCommandAsync(OnAddNewProductAsync, CanExecuteOnAddNewProduct);

        private bool CanExecuteOnAddNewProduct() => Services.Extensions.StringExtensions.IsNotNullOrWhiteSpace(SelectedStore?.Name, SelectedProduct?.Name) && SelectedProduct is { Price: > 0, Quantity: > 0 };

        private async Task LoadStoresAsync()
        {
            try
            {
                // Загружаем магазины из сервиса и преобразуем их в StoreWrapper
                var storeWrappers = (await _storeService.LoadStoresAsync())
                    .Select(store => new StoreWrapper(store))
                    .ToList();

                // Очищаем коллекцию и добавляем новые элементы
                Stores.Clear();
                Stores.AddRange(storeWrappers);

                // Устанавливаем первый элемент как выбранный, если он существует
                SelectedStore = Stores.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // Обрабатываем ошибку
                _userDialog.ShowError($"Ошибка при загрузке магазинов: {ex.Message}");
            }
        }
        private async Task LoadProductsByStoreAsync(int storeId)
        {
            try
            {
                // Загружаем магазины из сервиса и преобразуем их в StoreWrapper
                var productsWrappers = (await _productService.LoadProductsByStoreAsync(storeId))
                    .Select(product => new ProductWrapper(product))
                    .ToList();

                // Очищаем коллекцию и добавляем новые элементы
                FilteredProducts.Clear();
                FilteredProducts.AddRange(productsWrappers);
                SelectedProduct = null;
            }
            catch (Exception ex)
            {
                _userDialog.ShowError($"Ошибка при загрузке товаров: {ex.Message}", "Ошибка");
            }
        }
        private async Task OnAddNewProductAsync()
        {
            if (SelectedStore is null || SelectedProduct is null)
                return;

            try
            {
                var isSuccess = await _productService.AddOrUpdateProductAsync(SelectedProduct, SelectedStore.Id);
                var resultMessage = isSuccess
                    ? $"Товар \"{SelectedProduct.Name}\" успешно добавлен/обновлён."
                    : "Произошла ошибка при добавлении/обновлении товара.";

                _userDialog.ShowInformation(resultMessage);
            }
            catch (Exception ex)
            {
                _userDialog.ShowError($"Ошибка при добавлении/обновлении товара: {ex.Message}");
            }
        }
    }
}

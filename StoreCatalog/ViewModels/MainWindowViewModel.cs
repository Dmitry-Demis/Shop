using System.Collections.ObjectModel;
using System.Windows.Input;
using MathCore.ViewModels;
using MathCore.WPF;
using MathCore.WPF.Commands;
using StoreCatalogBLL;
using StoreCatalogDAL.Model;
using StoreCatalogDAL.Model.Builders;
using StoreCatalogPresentation.Models;
using StoreCatalogPresentation.ViewModels.Services.Dialogs;
using StoreCatalogPresentation.Views.Windows;

namespace StoreCatalogPresentation.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        public string Title { get; init; } = "Магазин продуктов";

        private readonly IUserDialogService _userDialog;

        private readonly StoreService _storeService;
        private readonly ProductService _productService;
        private readonly CartService _cartService;


        private LambdaCommand? _createStoreCommand;

        public ICommand CreateStoreCommand =>
            _createStoreCommand ??= new LambdaCommand(App.OpenWindow<CreateStoreWindow>);

        private LambdaCommand? _createProductsCommand;

        public ICommand CreateProductsCommand =>
            _createProductsCommand ??= new LambdaCommand(App.OpenWindow<CreateProductWindow>);

        public ObservableCollection<StoreWrapper> Stores { get; set; } = [];
        private StoreWrapper? _selectedStore;

        public StoreWrapper? SelectedStore
        {
            get => _selectedStore;
            set
            {
                if (Set(ref _selectedStore, value))
                {
                    _ = LoadProductsByStoreAsync(value?.Id ?? 0);
                }
            }
        }


        private ObservableCollection<ProductWrapper> _filteredProducts = [];
        public ObservableCollection<ProductWrapper> FilteredProducts
        {
            get => _filteredProducts;
            set => Set(ref _filteredProducts, value);
        }
        private ProductWrapper? _selectedProduct = new(new Product());

        public MainWindowViewModel(ProductService productService,
            IUserDialogService userDialog,
            CartService cartService,
            StoreService storeService)
        {
            _userDialog = userDialog ?? throw new ArgumentNullException(nameof(userDialog));
            _storeService = storeService ?? throw new ArgumentNullException(nameof(storeService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
            _ = LoadStoresAsync();
        }

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

                FilteredProducts.Clear();
                if (productsWrappers.Count == 0)
                    return;

                // Очищаем коллекцию и добавляем новые элементы
                FilteredProducts.AddRange(productsWrappers);
                SelectedProduct = null;
            }
            catch (Exception ex)
            {
                _userDialog.ShowError($"Ошибка при загрузке товаров: {ex.Message}", "Ошибка");
            }
        }


        private string _productSearch;
        public string ProductSearch
        {
            get => _productSearch;
            set => Set(ref _productSearch, value);
        }

        private ICommand? _searchCheapestProductCommand;
        public ICommand SearchCheapestProductCommand => _searchCheapestProductCommand ??= new LambdaCommandAsync(SearchCheapestProductAsync, () => !string.IsNullOrWhiteSpace(ProductSearch));

        private async Task SearchCheapestProductAsync(object? arg)
        {
            var cheapestProduct = await _productService.SearchCheapestProductAsync(ProductSearch);
            if (cheapestProduct != null)
            {
                // Устанавливаем магазин для найденного товара
                SelectedStore = null;
                SelectedStore = Stores.FirstOrDefault(s => s.Id == cheapestProduct.StoreId);
                if (SelectedStore != null)
                {
                    SelectedProduct = FilteredProducts.Find(fp => fp.Base.Id == cheapestProduct.Id);
                }
            }
            else
            {
                SelectedProduct = null;
            }
        }


        private ICommand? _cheapestStoreCommand;
        public ICommand CheapestStoreCommand => _cheapestStoreCommand ??= new LambdaCommand(App.OpenWindow<PurchaseWindow>);


        private ICommand? _showCartCommand;
        public ICommand ShowCartCommand => _showCartCommand ??= new LambdaCommand(App.OpenWindow<CartWindow>, 
            CanShowCartCommand);

        private bool CanShowCartCommand() => _cartService.AddedStoreIds.Count != 0;

        private ICommand? _addToCartCommand;
        public ICommand AddToCartCommand => _addToCartCommand ??= new LambdaCommand<ProductWrapper>(AddToCart, CanAddToCart);

        private void AddToCart(ProductWrapper? product)
        {
            if (product == null || SelectedStore == null) return;
            product.SelectedQuantity = Math.Max(1, product.SelectedQuantity);

            var productCopy = ProductBuilder
                .Create()
                .FromExisting(product.Base)
                .Build();

            _cartService.AddToCart(productCopy, product.SelectedQuantity);

            product.Quantity = productCopy.Quantity;
            product.SelectedQuantity = 0;

        }

        private static bool CanAddToCart(ProductWrapper? product) => product is { Quantity: > 0 };

        private ICommand? _removeFromCartCommand;
        public ICommand RemoveFromCartCommand =>
            _removeFromCartCommand ??= new LambdaCommand<ProductWrapper>(RemoveFromCart);

        private void RemoveFromCart(ProductWrapper? product)
        {
            if (product == null || SelectedStore == null) return;

            // Копирование всех свойств вручную
            var productCopy = ProductBuilder
                                            .Create()
                                            .FromExisting(product.Base)
                                            .Build();
            _cartService.RemoveFromCart(productCopy);
            product.Quantity = productCopy.Quantity;
        }

        private decimal _purchaseAmount;
        public decimal PurchaseAmount
        {
            get => _purchaseAmount;
            set => Set(ref _purchaseAmount, value);
        }
        private ICommand? _purchaseCommand;
        public ICommand PurchaseCommand => _purchaseCommand ??= new LambdaCommand(ExecutePurchase, () => PurchaseAmount > 0);

        private void ExecutePurchase()
        {
            var result = _cartService.GetAffordableProducts(PurchaseAmount, FilteredProducts.Select(p => p.Base));
            if (result)
            {
                FilteredProducts = new ObservableCollection<ProductWrapper>(FilteredProducts);
                _userDialog.ShowInformation("Товары успешно добавлены в корзину.");
            }
            else
            {
                _userDialog.ShowWarning($"На {PurchaseAmount} ₽ невозможно ничего купить.");
            }

        }
    }
}

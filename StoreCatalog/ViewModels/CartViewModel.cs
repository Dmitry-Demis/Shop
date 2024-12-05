using MathCore.ViewModels;
using StoreCatalogBLL.Model;
using System.Collections.ObjectModel;
using StoreCatalogPresentation.Models;
using StoreCatalogBLL;
using MathCore.WPF;
using StoreCatalogDAL.Model;
using MathCore.WPF.Commands;
using System.Windows.Input;
using StoreCatalogPresentation.ViewModels.Services.Dialogs;

namespace StoreCatalogPresentation.ViewModels
{
    public class CartViewModel : ViewModel
    {
        public string Title { get; init; } = "Корзина";
        private readonly CartService _cartService;
        private readonly StoreService _storeService;
        private readonly ProductService _productService;
        private readonly IUserDialogService _userDialog;

        public CartViewModel(
            CartService cartService
            , StoreService storeService
            , ProductService productService
            , IUserDialogService userDialog)
        {
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
            _storeService = storeService ?? throw new ArgumentNullException(nameof(storeService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _userDialog = userDialog ?? throw new ArgumentNullException(nameof(userDialog));
            _ = LoadStoresAsync();

        }

        // Список товаров в корзине
        public ObservableCollection<CartItemWrapper> CartItems { get; set; } = [];

        private StoreWrapper? _selectedStore;

        // Свойство для выбранного магазина
        public StoreWrapper? SelectedStore
        {
            get => _selectedStore;
            set
            {
                if (Set(ref _selectedStore, value))
                {
                    _ = LoadCartItemsByStoreAsync(value?.Id ?? 0);
                }
            }
        }

        // Список магазинов, в которые были добавлены товары
        public ObservableCollection<StoreWrapper> Stores { get; private set; } = [];

        // Загружаем товары для выбранного магазина
        public async Task LoadCartItemsByStoreAsync(int? storeId)
        {
            if (storeId.HasValue)
            {
                var cartItems = await _cartService.LoadCartItemsByStoreAsync(storeId.Value);
                CartItems.Clear();
                CartItems.AddRange(cartItems.Select(cart => new CartItemWrapper(cart)));
            }
        }

        // Загружаем все магазины, в которые были добавлены товары, и устанавливаем выбранный магазин как последний добавленный
        public async Task LoadStoresAsync()
        {
            // Получаем все StoreId, в которые были добавлены товары
            var addedStoreIds = _cartService.AddedStoreIds;
            if (addedStoreIds.Count == 0)
                return;

            // Очищаем предыдущий список магазинов и добавляем новые
            Stores.Clear();

            // Загружаем магазины с использованием StoreService
            var stores = await Task.WhenAll(addedStoreIds.Select(storeId => _storeService.LoadStoreByIdAsync(storeId)));

            // Добавляем в коллекцию обёрнутые в StoreWrapper магазины
            foreach (var store in stores.Where(store => store != null))
            {
                if (store != null) Stores.Add(new StoreWrapper(store)); // Оборачиваем в StoreWrapper
            }

            // Устанавливаем выбранный магазин как последний добавленный
            if (_cartService.LastAddedStoreId.HasValue)
            {
                var lastAddedStore = Stores.FirstOrDefault(storeWrapper => storeWrapper.Id == _cartService.LastAddedStoreId);

                // Если магазин найден, устанавливаем его как выбранный
                if (lastAddedStore != null)
                    SelectedStore = lastAddedStore;
            }
            OnPropertyChanged(nameof(TotalCost));
        }

        public decimal TotalCost => SelectedStore == null ? 0 : _cartService.GetTotalCost(SelectedStore.Id);

        private ICommand? _checkoutCommand;
        public ICommand CheckoutCommand => _checkoutCommand ??= new LambdaCommandAsync(Checkout);

        private async Task Checkout()
        {
            try
            {
                if (SelectedStore == null)
                {
                    _userDialog.ShowError("Пожалуйста, выберите магазин для оформления заказа.");
                    return;
                }

                var storeId = SelectedStore.Id;
                await _cartService.Checkout(storeId);
                
                CartItems.Clear();

                _userDialog.ShowInformation("Заказ оформлен успешно.");
                Stores.Remove(SelectedStore);
                SelectedStore = Stores.FirstOrDefault();
                OnPropertyChanged(nameof(TotalCost));
                if (_cartService.AddedStoreIds.Count == 0)
                    _userDialog.Close();
            }
            catch (Exception ex)
            {
                _userDialog.ShowError($"Ошибка при оформлении заказа: {ex.Message}");
            }
        }


    }
}

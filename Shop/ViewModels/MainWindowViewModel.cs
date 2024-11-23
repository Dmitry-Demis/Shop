using MathCore.ViewModels;
using MathCore.WPF.Commands;
using Microsoft.EntityFrameworkCore;
using Shop.DAL.Models;
using Shop.DAL.Models.Builders;
using Shop.DAL.Repositories;
using Shop.ViewModels.Wrappers;
using Shop.Views.Windows;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Shop.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        private string _title = "Магазин продуктов";
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<Product> _productRepository;

        public MainWindowViewModel(IRepository<Store> storeRepository, IRepository<Product> productRepository)
        {
            _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));

            LoadStores();
        }

        #region Properties

        public ObservableCollection<StoreWrapper> Stores { get; private set; } = new ObservableCollection<StoreWrapper>();
        public ObservableCollection<StoreInventoryWrapper> Products { get; private set; } = new ObservableCollection<StoreInventoryWrapper>();
        public ObservableCollection<CartWrapper> CartItems { get; private set; } = new ObservableCollection<CartWrapper>();

        private StoreWrapper _selectedStore;
        public StoreWrapper SelectedStore
        {
            get => _selectedStore;
            set
            {
                Set(ref _selectedStore, value);
                LoadProducts();
            }
        }

        private StoreInventoryWrapper _selectedProduct;
        public StoreInventoryWrapper SelectedProduct
        {
            get => _selectedProduct;
            set => Set(ref _selectedProduct, value);
        }

        private CartWrapper _selectedCart;
        public CartWrapper SelectedCart
        {
            get => _selectedCart;
            set => Set(ref _selectedCart, value);
        }

        public decimal TotalCost => CartItems.Sum(cartWrapper => cartWrapper.TotalPrice);

        #endregion

        #region Commands

        public ICommand CreateStoreCommand => new LambdaCommand(() => OpenWindow<CreateStoreWindow>());
        public ICommand CreateProductCommand => new LambdaCommand(() => OpenWindow<CreateProductWindow>());
        public ICommand StockProductCommand => new LambdaCommand(() => OpenWindow<StockProductWindow>());

        public ICommand AddToCartCommand => new LambdaCommand<StoreInventoryWrapper>(AddToCart);
        public ICommand UpdateQuantityCommand => new LambdaCommand<CartWrapper>(UpdateQuantity);

        // Команда для оформления заказа
        private ICommand _checkoutCommand;
        public ICommand CheckoutCommand => _checkoutCommand ??= new LambdaCommand(Checkout);

        // Команда для поиска товара
        private ICommand _searchProductCommand;
        public ICommand SearchProductCommand => _searchProductCommand ??= new LambdaCommand(SearchProduct);

        private string _productSearch = "";
        public string ProductSearch
        {
            get => _productSearch;
            set => Set(ref _productSearch, value);
        }

        #endregion

        #region Methods

        private void OpenWindow<TWindow>() where TWindow : Window, new()
        {
            var window = new TWindow { Owner = Application.Current.MainWindow };
            window.ShowDialog();
        }

        private async void LoadStores()
        {
            var stores = await _storeRepository.Items
                .Include(s => s.StoreInventories)
                    .ThenInclude(si => si.Product)
                .ToListAsync();

            Stores.Clear();
            foreach (var store in stores)
                Stores.Add(new StoreWrapper(store));

            SelectedStore = Stores.FirstOrDefault();
        }

        private void LoadProducts()
        {
            if (SelectedStore == null) return;

            var storeInventories = SelectedStore.StoreInventories;
            Products.Clear();

            foreach (var storeInventory in storeInventories)
            {
                var storeInventoryInstance = new StoreInventoryBuilder()
                    .SetStore(storeInventory.Store)
                    .SetProduct(storeInventory.Product)
                    .SetQuantity(storeInventory.Quantity)
                    .SetPrice(storeInventory.Price)
                    .Build();

                Products.Add(new StoreInventoryWrapper(storeInventoryInstance));
            }
        }

        private void AddToCart(StoreInventoryWrapper storeInventoryWrapper)
        {
            if (storeInventoryWrapper == null || storeInventoryWrapper.Quantity <= 0) return;

            var existingItem = CartItems.FirstOrDefault(item => item.Product.Id == storeInventoryWrapper.Product.Id);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                var cartItem = new CartItem(storeInventoryWrapper.Product.Unwrap(), storeInventoryWrapper.Price, 1);
                var cartWrapper = new CartWrapper(cartItem, storeInventoryWrapper);
                CartItems.Add(cartWrapper);
                storeInventoryWrapper.Quantity--;
            }

            OnPropertyChanged(nameof(TotalCost));
        }

        private void UpdateQuantity(CartWrapper cartItem)
        {
            if (cartItem == null) return;

            var product = Products.FirstOrDefault(p => p.Product.Id == cartItem.Product.Id);
            if (product != null)
            {
                product.Quantity += cartItem.Quantity;
                CartItems.Remove(cartItem);
            }
            else
            {
                // Товар не найден в корзине
                Console.WriteLine("Товар не найден в корзине");
            }

            OnPropertyChanged(nameof(TotalCost));
        }

        // Метод для оформления заказа
        private void Checkout()
        {
            // Показать сообщение о том, что заказ оформлен
            MessageBox.Show("Заказ оформлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            // Очистить корзину
            CartItems.Clear();

            // Обновить UI (например, TotalCost автоматически обновится, так как коллекция пустая)
            OnPropertyChanged(nameof(TotalCost));
        }

        private void SearchProduct()
        {
            if (string.IsNullOrWhiteSpace(ProductSearch))
            {
                LoadProducts();
                return;
            }

            var matchingProducts = Stores
                .SelectMany(store => store.StoreInventories)
                .Where(storeInventory => storeInventory.Product.Name.Contains(ProductSearch, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matchingProducts.Any())
            {
                Products.Clear();
                foreach (var storeInventory in matchingProducts)
                {
                    var storeInventoryInstance = new StoreInventoryBuilder()
                        .SetStore(storeInventory.Store)
                        .SetProduct(storeInventory.Product)
                        .SetQuantity(storeInventory.Quantity)
                        .SetPrice(storeInventory.Price)
                        .Build();

                    Products.Add(new StoreInventoryWrapper(storeInventoryInstance));
                }

                var storeWithCheapestProduct = matchingProducts
                    .GroupBy(storeInventory => storeInventory.Store)
                    .Select(group => new
                    {
                        Store = group.Key,
                        MinPrice = group.Min(storeInventory => storeInventory.Price)
                    })
                    .OrderBy(store => store.MinPrice)
                    .FirstOrDefault();

                if (storeWithCheapestProduct != null)
                {
                    SelectedStore = Stores.FirstOrDefault(storeWrapper => storeWrapper.Id == storeWithCheapestProduct.Store.Id);
                }
            }
            else
            {
                Products.Clear();
                MessageBox.Show("Товары не найдены.");
            }
        }

        #endregion

        private decimal _purchaseAmount;
        public decimal PurchaseAmount
        {
            get => _purchaseAmount;
            set => Set(ref _purchaseAmount, value);
        }

        private ICommand _purchaseCommand;
        public ICommand PurchaseCommand => _purchaseCommand ??= new LambdaCommand(ExecutePurchase);

        private void ExecutePurchase()
        {
            // Проверка на наличие введённой суммы
            if (PurchaseAmount <= 0)
            {
                MessageBox.Show("Пожалуйста, введите корректную сумму.");
                return;
            }

            decimal remainingAmount = PurchaseAmount;
            var selectedProducts = Products
                .Where(p => p.Quantity > 0) // Отфильтровываем только товары, которые есть в наличии
                .ToList(); // Делаем копию списка продуктов

            // Сортируем товары по цене, чтобы пытаться купить более дешевые товары первыми
            selectedProducts = selectedProducts.OrderBy(p => p.Price).ToList();

            var cartItems = new List<CartWrapper>();
            bool addedProductThisRound;

            // Добавляем товары в корзину с начальным количеством 0
            foreach (var productWrapper in selectedProducts)
            {
                var product = productWrapper.Product;
                var price = productWrapper.Price;

                // Добавляем товар в корзину с нулевым количеством
                var cartItem = new CartItem(product.Unwrap(), price, 0);
                var cartWrapper = new CartWrapper(cartItem, productWrapper);
                cartItems.Add(cartWrapper);
            }

            // Фаза 1: Пытаемся добавить по одному товару до исчерпания бюджета
            addedProductThisRound = true;
            while (remainingAmount > 0 && addedProductThisRound)
            {
                addedProductThisRound = false;

                foreach (var cartWrapper in cartItems)
                {
                    var price = cartWrapper.Price;
                    var availableQuantity = cartWrapper.Product.StoreInventories
                        .Sum(si => si.Quantity); // Суммируем количество по всем инвентарям товара

                    // Если есть деньги и товар в наличии, покупаем
                    if (remainingAmount >= price && cartWrapper.Quantity < availableQuantity)
                    {
                        cartWrapper.Quantity++; // Увеличиваем количество товара в корзине
                        remainingAmount -= price; // Уменьшаем оставшуюся сумму
                        addedProductThisRound = true;
                    }
                }
            }

            // Фаза 2: Если деньги ещё остались, увеличиваем количество товаров на 2, 3 и так далее
            while (remainingAmount > 0)
            {
                addedProductThisRound = false;

                // Проверяем возможность покупки товаров по 2, 3 и так далее
                foreach (var cartWrapper in cartItems)
                {
                    var price = cartWrapper.Price;
                    var availableQuantity = cartWrapper.Product.StoreInventories
                        .Sum(si => si.Quantity); // Суммируем количество по всем инвентарям товара
                    var maxAffordableQuantity = (int)(remainingAmount / price);

                    // Мы увеличиваем количество на минимальное количество, которое можем купить
                    var quantityToAdd = Math.Min(maxAffordableQuantity, availableQuantity - cartWrapper.Quantity);

                    if (quantityToAdd > 0)
                    {
                        cartWrapper.Quantity += quantityToAdd;
                        remainingAmount -= quantityToAdd * price;
                        addedProductThisRound = true;
                    }
                }

                // Если мы не можем добавить ни одного товара, выходим из цикла
                if (!addedProductThisRound) break;
            }

            // Обновляем корзину
            CartItems.Clear();
            foreach (var cartWrapper in cartItems)
            {
                if (cartWrapper.Quantity > 0) // Если есть хотя бы один товар
                {
                    CartItems.Add(cartWrapper);
                }
            }

            // Обновляем инвентари
            foreach (var cartWrapper in cartItems)
            {
                if (cartWrapper.Quantity > 0)
                {
                    var totalQuantityToDeduct = cartWrapper.Quantity;
                    foreach (var storeInventory in cartWrapper.Product.StoreInventories)
                    {
                        if (totalQuantityToDeduct <= 0) break;

                        // Сколько можно забрать из текущего инвентаря
                        var quantityInInventory = storeInventory.Quantity;
                        var quantityToTake = Math.Min(totalQuantityToDeduct, quantityInInventory);

                        // Уменьшаем количество товара в инвентаре
                        storeInventory.Quantity -= quantityToTake;
                        totalQuantityToDeduct -= quantityToTake;
                    }
                }
            }

            // Обновляем UI
            OnPropertyChanged(nameof(CartItems));
            OnPropertyChanged(nameof(TotalCost));

        }

    }
}

using MathCore.ViewModels;
using MathCore.WPF.Commands;
using Microsoft.EntityFrameworkCore;
using Shop.DAL.Models;
using Shop.DAL.Models.Builders;
using Shop.DAL.Repositories;
using Shop.ViewModels.Wrappers;
using Shop.Views.Windows;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Shop.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        private string _Title = "Магазин продуктов";
        public string Title
        {
            get => _Title;
            set => Set(ref _Title, value);
        }

        // Универсальный метод для открытия окон
        private void OpenWindow<TWindow>() where TWindow : Window, new()
        {
            var window = new TWindow
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        }

        // Команда для создания магазина
        private ICommand? _createStoreCommand;
        public ICommand CreateStoreCommand => _createStoreCommand ??= new LambdaCommand(() => OpenWindow<CreateStoreWindow>());

        // Команда для создания продукта
        private ICommand? _createProductCommand;
        public ICommand CreateProductCommand => _createProductCommand ??= new LambdaCommand(() => OpenWindow<CreateProductWindow>());

        // Команда для завоза партии товаров
        private ICommand? _stockProductCommand;
        public ICommand StockProductCommand => _stockProductCommand ??= new LambdaCommand(() => OpenWindow<StockProductWindow>());
        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<Product> _productRepository;

        public MainWindowViewModel(IRepository<Store> storeRepository, IRepository<Product> productRepository)
        {
            _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));

            LoadStores();  // Загружаем магазины при инициализации
        }

        #region Properties

        // Список магазинов
        public ObservableCollection<StoreWrapper> Stores { get; private set; } = new ObservableCollection<StoreWrapper>();

        // Список продуктов
        public ObservableCollection<StoreInventoryWrapper> Products { get; private set; } = new ObservableCollection<StoreInventoryWrapper>();

        private StoreWrapper _selectedStore;
        public StoreWrapper SelectedStore
        {
            get => _selectedStore;
            set
            {
                Set(ref _selectedStore, value);
                LoadProducts(); // Загружаем продукты для выбранного магазина
            }
        }

        private StoreInventoryWrapper _selectedProduct;
        public StoreInventoryWrapper SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                Set(ref _selectedProduct, value);
            }
        }

        #endregion

        #region Methods

        // Загрузка магазинов из репозитория
        private async void LoadStores()
        {
            // Загружаем магазины с их инвентарём через Include()
            var stores = await _storeRepository.Items
                .Include(s => s.StoreInventories) // Включаем коллекцию StoreInventories
                .ThenInclude(si => si.Product) // Включаем продукты, если нужно
                .ToListAsync();

            Stores.Clear();
            foreach (var store in stores)
            {
                Stores.Add(new StoreWrapper(store));
            }

            // Устанавливаем первый магазин как выбранный
            SelectedStore = Stores.FirstOrDefault();
        }

        private void LoadProducts()
        {
            // Проверяем, выбран ли магазин
            if (SelectedStore == null) return;

            // Загружаем все StoreInventories для выбранного магазина
            var storeInventories = SelectedStore.StoreInventories;  // Доступ к StoreInventories через SelectedStore

            // Очистка списка продуктов
            Products.Clear();

            // Создаём StoreInventoryWrapper для каждого StoreInventory через StoreInventoryBuilder
            foreach (var storeInventory in storeInventories)
            {
                // Используем StoreInventoryBuilder для построения объекта StoreInventory
                var storeInventoryInstance = new StoreInventoryBuilder()
                    .SetStore(storeInventory.Store)
                    .SetProduct(storeInventory.Product)
                    .SetQuantity(storeInventory.Quantity)
                    .SetPrice(storeInventory.Price)
                    .Build();

                // Добавляем в список продуктов
                Products.Add(new StoreInventoryWrapper(storeInventoryInstance));
            }

            // Если продукт не выбран, установим первый товар как выбранный
            SelectedProduct = Products.FirstOrDefault();
        }
        #endregion

        public ObservableCollection<CartWrapper> CartItems { get; private set; } = new ObservableCollection<CartWrapper>();

        // Команда для добавления продукта в корзину
        // Команда для добавления товара в корзину
        private ICommand? _addToCartCommand;
        public ICommand AddToCartCommand => _addToCartCommand ??= new LambdaCommand<StoreInventoryWrapper>(storeInventoryWrapper => {

            if (storeInventoryWrapper == null || storeInventoryWrapper.Quantity <= 0) return;
            // Проверяем, есть ли уже этот товар в корзине
            var existingItem = CartItems.FirstOrDefault(item => item.Product.Id == storeInventoryWrapper.Product.Id);
            if (existingItem != null)
            {
                // Если товар уже в корзине, увеличиваем количество
                ++existingItem.Quantity;
            }
            else
            {
                // Если товара нет в корзине, создаём новый элемент корзины
                var cartItem = new CartItem(storeInventoryWrapper.Product.Unwrap(), storeInventoryWrapper.Price, 1);
                var cartWrapper = new CartWrapper(cartItem, storeInventoryWrapper);
                // Добавляем в корзину
                CartItems.Add(cartWrapper);
            }

            // Уменьшаем количество в инвентаре
            --storeInventoryWrapper.Quantity;
        });

        // Команда для изменения количества
        public ICommand UpdateQuantityCommand => new LambdaCommand<int>(UpdateQuantity);

        // Метод для изменения количества товара в корзине
        // Метод для изменения количества товара в корзине
        private void UpdateQuantity(int quantity)
        {
            var cartWrapper = CartItems.FirstOrDefault(item => item.Quantity == quantity);
            if (cartWrapper != null && cartWrapper.Quantity == 0)
            {
                // Если количество стало 0, удаляем товар из корзины
                CartItems.Remove(cartWrapper);
            }

            // В противном случае просто обновляем количество товара
            cartWrapper.Quantity = quantity;
        }
    }
}

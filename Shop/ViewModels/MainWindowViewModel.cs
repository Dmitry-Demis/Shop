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
                LoadProducts(value?.Id); // Загружаем продукты для выбранного магазина
            }
        }

        #endregion

        #region Methods

        // Загрузка магазинов из репозитория
        private async void LoadStores()
        {
            var stores = await _storeRepository.Items.ToListAsync();
            Stores.Clear();
            foreach (var store in stores)
            {
                Stores.Add(new StoreWrapper(store));
            }

            // Устанавливаем первый магазин по умолчанию
            SelectedStore = Stores.FirstOrDefault();
        }

        private async void LoadProducts(int? storeId)
        {
            if (storeId == null) return;

            // Загружаем StoreInventories для выбранного магазина с информацией о продукте
            var storeInventories = await _storeRepository.Items
                .Where(s => s.Id == storeId)
                .Include(s => s.StoreInventories)  // Включаем связанные StoreInventories
                    .ThenInclude(si => si.Product)  // Включаем связанные продукты
                .SelectMany(s => s.StoreInventories)
                .ToListAsync();

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
        }



        #endregion
    }
}

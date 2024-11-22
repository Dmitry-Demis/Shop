using MathCore.ViewModels;
using MathCore.WPF.Commands;
using Microsoft.EntityFrameworkCore;
using Shop.DAL.Models;
using Shop.DAL.Models.Builders;
using Shop.DAL.Repositories;
using Shop.ViewModels.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Shop.ViewModels
{
    public class StockProductViewModel : ViewModel
    {
        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IUserDialogService _userDialogService;

        public StockProductViewModel(
            IRepository<Store> storeRepository,
            IRepository<Product> productRepository,
            IUserDialogService userDialogService)
        {
            _storeRepository = storeRepository;
            _productRepository = productRepository;
            _userDialogService = userDialogService;

            _ = LoadDataAsync();
        }

        private string _title = "Завезти товары в магазин";
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        private ObservableCollection<string> _uniqueStoreNames = new();
        public ObservableCollection<string> UniqueStoreNames
        {
            get => _uniqueStoreNames;
            set => Set(ref _uniqueStoreNames, value);
        }

        private string? _selectedStoreName;
        public string? SelectedStoreName
        {
            get => _selectedStoreName;
            set
            {
                if (Set(ref _selectedStoreName, value))
                {
                    UpdateAddresses();
                }
            }
        }

        private ObservableCollection<string> _addresses = new();
        public ObservableCollection<string> Addresses
        {
            get => _addresses;
            set => Set(ref _addresses, value);
        }

        private string? _selectedAddress;
        public string? SelectedAddress
        {
            get => _selectedAddress;
            set
            {
                if (Set(ref _selectedAddress, value))
                {
                    UpdateSelectedStore();
                }
            }
        }

        private ObservableCollection<Product> _products = new();
        public ObservableCollection<Product> Products
        {
            get => _products;
            set => Set(ref _products, value);
        }

        private Product? _selectedProduct;
        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set => Set(ref _selectedProduct, value);
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set => Set(ref _quantity, value);
        }

        private decimal _price;
        public decimal Price
        {
            get => _price;
            set => Set(ref _price, value);
        }

        private Store? _selectedStore;
        public Store? SelectedStore
        {
            get => _selectedStore;
            set => Set(ref _selectedStore, value);
        }

        private ICommand? _addNewProductCommand;
        public ICommand AddNewProductCommand => _addNewProductCommand ??= new LambdaCommandAsync(OnAddNewProductExecutedAsync, CanAddNewProductExecute);

        private bool CanAddNewProductExecute() =>
            SelectedStore != null && SelectedProduct != null && Quantity > 0 && Price > 0;

        private async Task OnAddNewProductExecutedAsync()
        {
            // Проверка, что все обязательные данные выбраны
            if (SelectedStore == null || SelectedProduct == null || string.IsNullOrWhiteSpace(SelectedAddress))
            {
                _userDialogService.ShowError("Не выбраны все необходимые данные: магазин, адрес или товар.");
                return;
            }

            try
            {
                // Загружаем актуальные данные из репозитория для предотвращения рассинхронизации
                var store = await _storeRepository.Items
                    .Include(s => s.StoreInventories)
                    .FirstOrDefaultAsync(s => s.Id == SelectedStore.Id);

                if (store == null)
                {
                    _userDialogService.ShowError("Выбранный магазин не найден в базе данных.");
                    return;
                }

                // Проверяем наличие товара в магазине
                var existingInventory = store.StoreInventories
                    .FirstOrDefault(si => si.ProductId == SelectedProduct.Id);

                if (existingInventory != null)
                {
                    // Обновляем количество и цену
                    existingInventory.Quantity += Quantity;
                    existingInventory.Price = Price;
                    // Уведомляем об успешной операции
                    _userDialogService.ShowInformation("Товар успешно обновлён в магазине.");
                }
                else
                {
                    // Создаём новый StoreInventory
                    var newInventory = new StoreInventoryBuilder()
                        .SetStore(store)
                        .SetProduct(SelectedProduct)
                        .SetQuantity(Quantity)
                        .SetPrice(Price)
                        .Build();

                    store.StoreInventories.Add(newInventory);
                    // Уведомляем об успешной операции
                    _userDialogService.ShowInformation("Товар успешно добавлен");
                }

                // Сохраняем изменения
                await _storeRepository.UpdateAsync(store);
                // Сбрасываем поля
                Quantity = 0;
                Price = 0;
                SelectedProduct = null;
                _userDialogService.Close();
            }
            catch (Exception ex)
            {
                // Логируем и отображаем ошибку
                _userDialogService.ShowError($"Ошибка при добавлении товара: {ex.Message}");
            }
        }


        private async Task LoadDataAsync()
        {
            var stores = await _storeRepository.Items.ToListAsync();
            Stores = new ObservableCollection<Store>(stores);

            // Заполняем уникальные названия магазинов
            UniqueStoreNames = new ObservableCollection<string>(
                stores.Select(s => s.Name).Distinct()
            );

            // Выбираем первый элемент по умолчанию
            SelectedStoreName = UniqueStoreNames.FirstOrDefault();

            var products = await _productRepository.Items.ToListAsync();
            Products = new ObservableCollection<Product>(products);

            // Устанавливаем первый продукт по умолчанию
            SelectedProduct = Products.FirstOrDefault();
        }

        private void UpdateAddresses()
        {
            Addresses.Clear();
            if (string.IsNullOrWhiteSpace(SelectedStoreName)) return;

            var relevantStores = Stores.Where(s => s.Name == SelectedStoreName).ToList();
            foreach (var store in relevantStores)
            {
                Addresses.Add(store.Address);
            }

            // Выбираем первый адрес по умолчанию
            SelectedAddress = Addresses.FirstOrDefault();
        }


        private void UpdateSelectedStore()
        {
            if (string.IsNullOrWhiteSpace(SelectedAddress)) return;

            SelectedStore = Stores.FirstOrDefault(
                s => s.Name == SelectedStoreName && s.Address == SelectedAddress
            );
        }

        private ObservableCollection<Store> _stores = new();
        public ObservableCollection<Store> Stores
        {
            get => _stores;
            set => Set(ref _stores, value);
        }
    }
}

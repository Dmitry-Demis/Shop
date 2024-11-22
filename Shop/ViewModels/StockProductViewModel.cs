using MathCore.ViewModels;
using MathCore.WPF.Commands;
using Microsoft.EntityFrameworkCore;
using Shop.DAL.Models;
using Shop.DAL.Models.Builders;
using Shop.DAL.Repositories;
using Shop.ViewModels.Services;
using Shop.ViewModels.Wrappers;
using System;
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
        private readonly IUserDialogService _userDialog;

        public StockProductViewModel(
            IRepository<Store> storeRepository,
            IRepository<Product> productRepository,
            IUserDialogService userDialog)
        {
            _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _userDialog = userDialog ?? throw new ArgumentNullException(nameof(userDialog));

            StoreInventory = new StoreInventoryWrapper(new StoreInventory());            // Инициализация коллекций
            LoadStores();
            LoadProducts();
        }

        #region Properties

        // Свойства для привязки данных
        public ObservableCollection<StoreWrapper> Stores { get; private set; } = new ObservableCollection<StoreWrapper>();
        public ObservableCollection<ProductWrapper> Products { get; private set; } = new ObservableCollection<ProductWrapper>();

        private StoreWrapper _selectedStore;
        public StoreWrapper SelectedStore
        {
            get => _selectedStore;
            set => Set(ref _selectedStore, value);
        }

        private ProductWrapper _selectedProduct;
        public ProductWrapper SelectedProduct
        {
            get => _selectedProduct;
            set => Set(ref _selectedProduct, value);
        }

        public StoreInventoryWrapper _storeInventory;

        public StoreInventoryWrapper StoreInventory
        {
            get => _storeInventory;
            set => Set(ref _storeInventory, value);
        }
        #endregion

        #region Commands

        private ICommand? _addNewProductCommand;
        public ICommand AddNewProductCommand => _addNewProductCommand ??= new LambdaCommandAsync(OnAddOrUpdateProductExecutedAsync, CanAddNewProductExecute);

        private bool CanAddNewProductExecute() =>
            SelectedStore != null && SelectedProduct != null && StoreInventory.Quantity > 0 && StoreInventory.Price > 0;

        #endregion

        #region Methods

        // Метод загрузки магазинов с включением их инвентаря
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

        // Метод загрузки продуктов
        private async void LoadProducts()
        {
            var products = await _productRepository.Items.ToListAsync();
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(new ProductWrapper(product));
            }

            // Устанавливаем первый продукт как выбранный
            SelectedProduct = Products.FirstOrDefault();
        }


        private async Task OnAddOrUpdateProductExecutedAsync()
        {
            try
            {
                // Проверка, выбран ли товар и магазин
                if (SelectedProduct == null || SelectedStore == null)
                {
                    _userDialog.ShowError("Не выбран товар или магазин.", "Ошибка");
                    return; // Не выбраны товар или магазин
                }

                // Разворачиваем обёртки для получения реальных объектов Store и Product
                var store = SelectedStore.Unwrap();
                var product = SelectedProduct.Unwrap();

                // Проверяем, существует ли уже запись для этого товара в этом магазине
                var existingStoreInventory = store.StoreInventories
                    .FirstOrDefault(si => si.ProductId == product.Id);

                if (existingStoreInventory != null)
                {
                    // Если товар уже есть в магазине, обновляем количество и цену
                    existingStoreInventory.Quantity += StoreInventory.Quantity;
                    existingStoreInventory.Price = StoreInventory.Price;

                    // Обновляем StoreInventory через репозиторий
                    await _storeRepository.UpdateAsync(store);
                }
                else
                {
                    // Если товара нет в магазине, создаём новый StoreInventory через Builder
                    var storeInventory = new StoreInventoryBuilder()
                        .SetStore(store) // Устанавливаем магазин
                        .SetProduct(product) // Устанавливаем продукт
                        .SetQuantity(StoreInventory.Quantity) // Устанавливаем количество
                        .SetPrice(StoreInventory.Price) // Устанавливаем цену
                        .Build(); // Строим объект StoreInventory

                    // Добавляем новый StoreInventory в коллекцию StoreInventories
                    store.StoreInventories.Add(storeInventory);

                    // Сохраняем изменения в репозитории
                    await _storeRepository.UpdateAsync(store);
                }

                // Сброс полей после добавления/обновления товара
                StoreInventory.Quantity = 0;
                StoreInventory.Price = 0;
                SelectedProduct = null;

                _userDialog.ShowInformation("Товар успешно добавлен/обновлен в магазине.");
                _userDialog.Close();

                // Перезагружаем магазины и продукты
                LoadStores();
                LoadProducts();
            }
            catch (Exception ex)
            {
                _userDialog.ShowError($"Ошибка при добавлении/обновлении товара: {ex.Message}", "Ошибка");
            }
        }




        #endregion
    }
}

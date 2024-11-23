using MathCore.ViewModels;
using MathCore.WPF.Commands;
using MathCore.WPF.Dialogs;
using Microsoft.EntityFrameworkCore;
using Shop.DAL.Models;
using Shop.DAL.Repositories;
using Shop.ViewModels.Services;
using Shop.ViewModels.Wrappers;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Shop.ViewModels
{
    public class SearchViewModel : ViewModel
    {
        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IUserDialogService _userDialog;

        // Заголовок окна
        private string _title = "Поиск самого дешёвого магазина";
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        // Список товаров с количеством и флагом выбора
        public ObservableCollection<ProductQuantityWrapper> Products { get; set; } = new ObservableCollection<ProductQuantityWrapper>();

        // Сообщение о результате поиска
        private string _resultMessage = string.Empty;
        public string ResultMessage
        {
            get => _resultMessage;
            set => Set(ref _resultMessage, value);
        }

        // Команда для выполнения поиска
        private ICommand? _searchCommand;
        public ICommand SearchCommand => _searchCommand ??= new LambdaCommandAsync(OnSearchExecutedAsync, CanSearchExecute);

        // Конструктор
        public SearchViewModel(IRepository<Store> storeRepository, IRepository<Product> productRepository, IUserDialogService userDialog)
        {
            _storeRepository = storeRepository;
            _productRepository = productRepository;
            _userDialog = userDialog;
            LoadProductsAsync();
        }

        // Метод для загрузки всех товаров и обертки их в ProductQuantityWrapper
        public async Task LoadProductsAsync()
        {
            // Загружаем все продукты из репозитория
            var products = await _productRepository.Items.ToListAsync();

            // Заполняем ObservableCollection
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(new ProductQuantityWrapper { Product = product, Quantity = 1, IsSelected = false });
            }
        }

        // Метод для поиска самого дешевого магазина
        private async Task OnSearchExecutedAsync()
        {
            // Проверяем, что все выбранные товары имеют количество больше 0
            if (Products.Any(p => p.IsSelected && p.Quantity <= 0))
            {
                _userDialog?.ShowError("Убедитесь, что для всех выбранных товаров введены количества.");
                return;
            }

            var totalCostPerStore = new Dictionary<string, decimal>();

            // Получаем все магазины
            var stores = await _storeRepository.Items.ToListAsync();

            foreach (var store in stores)
            {
                decimal totalCost = 0;

                // Для каждого выбранного товара вычисляем стоимость в этом магазине
                foreach (var productWrapper in Products.Where(p => p.IsSelected))
                {
                    var product = productWrapper.Product;
                    var quantity = productWrapper.Quantity;

                    var storeInventory = store.StoreInventories
                        .FirstOrDefault(si => si.ProductId == product.Id);

                    if (storeInventory != null)
                    {
                        // Проверка, что количество товара не больше доступного
                        if (quantity > storeInventory.Quantity)
                        {
                            quantity = storeInventory.Quantity; // Ограничиваем максимальное количество
                            productWrapper.Quantity = quantity; // Обновляем количество в обертке
                        }

                        totalCost += storeInventory.Price * quantity;
                    }
                    else
                    {
                        totalCost = decimal.MaxValue; // Если товара нет, стоимость = бесконечность
                        break;
                    }
                }

                if (totalCost < decimal.MaxValue)
                {
                    totalCostPerStore[store.ToString()] = totalCost;
                }
            }

            // Находим магазин с минимальной стоимостью
            if (totalCostPerStore.Any())
            {
                var cheapestStore = totalCostPerStore.OrderBy(s => s.Value).First();
                ResultMessage = $"Самый дешёвый магазин для этой партии: {cheapestStore.Key}, стоимость: {cheapestStore.Value:C}";
            }
            else
            {
                ResultMessage = "Не удалось найти подходящий магазин для всех товаров.";
            }
        }

        // Проверка, можно ли выполнить поиск (только для выбранных товаров с количеством > 0)
        private bool CanSearchExecute()
        {
            return Products.Any(p => p.IsSelected && p.Quantity > 0);
        }
    }


    // Обертка для товара и его количества с флагом выбора
    public class ProductQuantityWrapper : Wrapper
    {
        private Product _product;
        private int _quantity;
        private bool _isSelected;

        // Свойство для товара
        public Product Product
        {
            get => _product;
            set => Set(ref _product, value); // Используем Set для уведомления об изменении
        }

        // Свойство для количества товара
        public int Quantity
        {
            get => _quantity;
            set => Set(ref _quantity, value); // Используем Set для уведомления об изменении
        }

        // Свойство для флага выбора товара
        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value); // Используем Set для уведомления об изменении
        }
    }

}

using MathCore.ViewModels;
using Shop.DAL.Models;

namespace Shop.ViewModels.Wrappers
{
    public class StoreWrapper(Store store) : Wrapper
    {
        private readonly Store _store = store ?? throw new ArgumentNullException(nameof(store));

        public int Id => _store.Id;
        public string Name
        {
            get => _store.Name;
            set
            {
                if (_store.Name == value) return;
                _store.Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public string Address
        {
            get => _store.Address;
            set
            {
                if (_store.Address == value) return;
                _store.Address = value;
                OnPropertyChanged(nameof(Address));
            }
        }
        public string Code => _store.Code; // Только для чтения
        public List<StoreInventory> StoreInventories => _store.StoreInventories;
        public override string ToString() => _store.ToString();
        public Store Unwrap() => _store;
    }

    public class CartItemWrapper : ViewModel
    {
        private int _productId;
        public int ProductId
        {
            get => _productId;
            set => Set(ref _productId, value);
        }

        private string _productName;
        public string ProductName
        {
            get => _productName;
            set => Set(ref _productName, value);
        }

        private decimal _price;
        public decimal Price
        {
            get => _price;
            set => Set(ref _price, value);
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set => Set(ref _quantity, value);
        }

        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            set => Set(ref _totalPrice, value);
        }

        // Обновление общей цены
        public void UpdateTotalPrice()
        {
            TotalPrice = Quantity * Price;
        }
    }
}

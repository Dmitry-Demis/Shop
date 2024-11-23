using Shop.DAL.Models;

namespace Shop.ViewModels.Wrappers
{
    public class StoreInventoryWrapper(StoreInventory storeInventory) : Wrapper
    {
        private readonly StoreInventory _storeInventory = storeInventory ?? throw new ArgumentNullException(nameof(storeInventory));

        public int StoreId
        {
            get => _storeInventory.StoreId;
            set
            {
                if (_storeInventory.StoreId == value) return;
                _storeInventory.StoreId = value;
                OnPropertyChanged(nameof(StoreId));
            }
        }

        public StoreWrapper Store
        {
            get => new(_storeInventory.Store); // Оборачиваем Store в StoreWrapper
        }

        public int ProductId
        {
            get => _storeInventory.ProductId;
            set
            {
                if (_storeInventory.ProductId == value) return;
                _storeInventory.ProductId = value;
                OnPropertyChanged(nameof(ProductId));
            }
        }

        public ProductWrapper Product
        {
            get => new(_storeInventory.Product); // Оборачиваем Product в ProductWrapper
        }

        public int Quantity
        {
            get => _storeInventory.Quantity;
            set
            {
                if (_storeInventory.Quantity == value) return;
                _storeInventory.Quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(IsAvailable));
            }
        }

        public decimal Price
        {
            get => _storeInventory.Price;
            set
            {
                if (_storeInventory.Price == value) return;
                _storeInventory.Price = value;
                OnPropertyChanged(nameof(Price));
            }
        }
        // Новый метод для проверки доступности товара
        // Свойство для проверки доступности товара
        public bool IsAvailable
        {
            get => _storeInventory.Quantity > 0; // Если количество больше нуля, товар доступен
        }
        public StoreInventory Unwrap() => _storeInventory;
        public override string ToString() => $"{Store.Name} - {Product.Name} - {Quantity} units at {Price:C}";
    }

}

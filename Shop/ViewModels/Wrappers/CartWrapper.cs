using Shop.DAL.Models;

namespace Shop.ViewModels.Wrappers
{
    public class CartWrapper : Wrapper
    {
        private readonly CartItem _cartItem;
        private readonly StoreInventoryWrapper _storeInventoryWrapper;

        public CartWrapper(CartItem cartItem, StoreInventoryWrapper storeInventoryWrapper)
        {
            _cartItem = cartItem ?? throw new ArgumentNullException(nameof(cartItem));
            _storeInventoryWrapper = storeInventoryWrapper ?? throw new ArgumentNullException(nameof(storeInventoryWrapper));
        }

        public Product Product
        {
            get => _cartItem.Product;
        }

        public decimal Price
        {
            get => _cartItem.Price;
        }

        public int Quantity
        {
            get => _cartItem.Quantity;
            set
            {
                if (_cartItem.Quantity == value || value < 0) return;

                // Если новое количество меньше текущего, увеличиваем количество на складе
                if (value < _cartItem.Quantity)
                {
                    _storeInventoryWrapper.Quantity += _cartItem.Quantity - value;
                }
                else
                {
                    // Если новое количество больше текущего, проверяем, достаточно ли товара на складе
                    if (_storeInventoryWrapper.Quantity >= value - _cartItem.Quantity)
                    {
                        _storeInventoryWrapper.Quantity -= value - _cartItem.Quantity;
                    }
                    else
                    {
                        // Если на складе недостаточно товара, устанавливаем максимально возможное количество
                        value = _cartItem.Quantity + _storeInventoryWrapper.Quantity;
                        _storeInventoryWrapper.Quantity = 0;  // Все товары из инвентаря списаны
                    }
                }

                // Обновляем количество в корзине
                _cartItem.Quantity = value;

                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(TotalPrice)); // Обновляем общую стоимость при изменении количества
            }
        }

        public decimal TotalPrice => _cartItem.TotalPrice;

        public override string ToString()
        {
            return $"{_cartItem.Product.Name} - {_cartItem.Quantity} шт. на сумму {_cartItem.TotalPrice:C}";
        }

        public CartItem Unwrap()
        {
            return _cartItem;
        }
    }
}

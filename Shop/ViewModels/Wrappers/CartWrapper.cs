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
                if (value < 0) { return; }
                // Проверяем, если новое количество меньше текущего (уменьшаем количество в корзине)
                if (value < _cartItem.Quantity)
                {
                    // Восстанавливаем количество на складе
                    _storeInventoryWrapper.Quantity += _cartItem.Quantity - value;
                }
                // Если новое количество больше текущего (увеличиваем количество в корзине)
                else if (value > _cartItem.Quantity)
                {
                    var requiredStock = value - _cartItem.Quantity;

                    // Проверяем, достаточно ли товара на складе
                    if (_storeInventoryWrapper.Quantity >= requiredStock)
                    {
                        // Уменьшаем количество товара на складе
                        _storeInventoryWrapper.Quantity -= requiredStock;
                    }
                    else
                    {
                        // Если на складе недостаточно товара, устанавливаем максимальное количество в корзине
                        value = _cartItem.Quantity + _storeInventoryWrapper.Quantity;
                        _storeInventoryWrapper.Quantity = 0; // Все товары из инвентаря списаны
                    }
                }

                // Обновляем количество в корзине
                _cartItem.Quantity = value;

                // Обновляем общую стоимость при изменении количества
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(TotalPrice));
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

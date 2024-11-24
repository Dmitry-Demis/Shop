﻿namespace Shop.DAL.Models.Builders
{
    public class StoreInventoryBuilder : IBuilder<StoreInventory>
    {
        private readonly StoreInventory _storeInventory = new ();
        public StoreInventoryBuilder SetStore(Store store)
        {
            _storeInventory.Store = store ?? throw new ArgumentNullException(nameof(store));
            _storeInventory.StoreId = store.Id;
            return this;
        }

        public StoreInventoryBuilder SetProduct(Product product)
        {
            _storeInventory.Product = product ?? throw new ArgumentNullException(nameof(product));
            _storeInventory.ProductId = product.Id;
            return this;
        }

        public StoreInventoryBuilder SetQuantity(int quantity)
        {
            _storeInventory.Quantity = 
                quantity >= 0? 
                quantity: 
                throw new ArgumentException("Количество не может быть отрицательным.", nameof(quantity));
            return this;
        }

        public StoreInventoryBuilder SetPrice(decimal price)
        {
            _storeInventory.Price = 
                price >= 0 ? 
                price : 
                throw new ArgumentException("Цена не может быть отрицательной.", nameof(price));
            return this;
        }

        public StoreInventory Build()
        {
            if (_storeInventory.Store == null)
                throw new InvalidOperationException("Магазин должен быть указан.");
            if (_storeInventory.Product == null)
                throw new InvalidOperationException("Продукт должен быть указан.");
            return _storeInventory;
        }
    }

}

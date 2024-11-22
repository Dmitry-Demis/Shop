namespace Shop.DAL.Models.Builders
{
    public class ProductBuilder
    {
        private readonly Product _product = new() { StoreInventories = [] };

        public ProductBuilder SetName(string name)
        {
            _product.Name = name ?? throw new ArgumentException("Название продукта не может быть пустым.", nameof(name));
            return this;
        }

        public ProductBuilder AddInventory(StoreInventory storeInventory)
        {
            ArgumentNullException.ThrowIfNull(storeInventory);
            _product.StoreInventories.Add(storeInventory);
            return this;
        }

        public Product Build()
        {
            if (string.IsNullOrWhiteSpace(_product.Name))
                throw new InvalidOperationException("Продукт должен иметь имя.");
            return _product;
        }
    }

}

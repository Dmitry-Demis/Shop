namespace StoreCatalogDAL.Model.Builders
{
    public class ProductBuilder : BuilderBase<Product, ProductBuilder>
    {
        public ProductBuilder SetName(string name) => SetProperty(nameof(Product.Name), name);
        public ProductBuilder SetStoreId(int storeId) => SetProperty(nameof(Product.StoreId), storeId);
        public ProductBuilder SetQuantity(int quantity) => SetProperty(nameof(Product.Quantity), quantity);
        public ProductBuilder SetPrice(decimal price) => SetProperty(nameof(Product.Price), price);
    }

}

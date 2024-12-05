namespace StoreCatalogDAL.Model.Builders
{
    public class StoreBuilder : BuilderBase<Store, StoreBuilder>
    {
        public StoreBuilder SetName(string name) => SetProperty(nameof(Store.Name), name);
        public StoreBuilder SetAddress(string address) => SetProperty(nameof(Store.Address), address);
        public StoreBuilder SetCode(string code) => SetProperty(nameof(Store.Code), code);
    }
}

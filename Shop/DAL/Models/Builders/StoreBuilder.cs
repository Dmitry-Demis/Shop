namespace Shop.DAL.Models.Builders
{
    public class StoreBuilder : IBuilder<Store>
    {
        private readonly Store _store = new();
        // Статический метод для удобного создания нового билдера
        public static StoreBuilder Create() => new();

        // Метод для установки имени магазина
        public StoreBuilder SetName(string name)
        {
            _store.Name = name ?? throw new InvalidOperationException("Store name must be provided.");
            return this;
        }

        // Метод для установки адреса магазина
        public StoreBuilder SetAddress(string address)
        {
            _store.Address = address ?? throw new InvalidOperationException("Store address must be provided.");
            return this;
        }

        // Метод для добавления начального инвентаря
        public StoreBuilder AddInventory(StoreInventory inventory)
        {
            _store.StoreInventories.Add(inventory);
            return this;
        }

        // Метод для сборки объекта
        public Store Build() => _store;
    }
}

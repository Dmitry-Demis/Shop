using StoreCatalogDAL.Storage;
using StoreCatalogDAL.Model;
using StoreCatalogDAL.Model.Builders;

namespace StoreCatalogBLL
{
    public class StoreService(IRepository<Store> storeRepository)
    {
        private readonly IRepository<Store> _storeRepository = storeRepository
            ?? throw new ArgumentNullException(nameof(storeRepository));

        public async Task<bool> CreateStoreAsync(Store store)
        {
            ArgumentNullException.ThrowIfNull(store.Name, "Имя магазина не может быть пустым.");
            ArgumentNullException.ThrowIfNull(store.Address, "Адрес магазина не может быть пустым.");

            try
            {
                var newStore = StoreBuilder.Create()
                                           .SetName(store.Name)
                                           .SetAddress(store.Address)
                                           .Build();

                await _storeRepository.AddAsync(newStore);
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ошибка при создании магазина.", ex);
            }
        }
        public async Task<IEnumerable<Store>> LoadStoresAsync()
        {
            try
            {
                return await _storeRepository.GetAllAsync() ?? [];
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ошибка при загрузке магазинов.", ex);
            }
        }
        public async Task<Store?> LoadStoreByIdAsync(int storeId)
        {
            try
            {
                // Используем репозиторий для поиска магазина по ID
                return await _storeRepository.GetAsync(storeId);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Ошибка при загрузке магазина с ID {storeId}.", ex);
            }
        }
    }
}

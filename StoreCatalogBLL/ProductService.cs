using StoreCatalogBLL.Model;
using StoreCatalogDAL.Model;
using StoreCatalogDAL.Model.Builders;
using StoreCatalogDAL.Storage;

namespace StoreCatalogBLL
{
    public class ProductService(IRepository<Product> productRepository)
    {
        private readonly IRepository<Product> _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));

        public async Task<IEnumerable<Product>> LoadProductsAsync()
        {
            try
            {
                return await _productRepository.GetAllAsync() ?? [];
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ошибка при загрузке магазинов.", ex);
            }
        }

        public async Task<IEnumerable<Product>> LoadProductsByStoreAsync(int storeId)
        {
            try
            {
                return (await _productRepository.GetAllAsync() ?? []).Where(p => p.StoreId == storeId);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Ошибка при загрузке товаров для магазина с ID {storeId}.", ex);
            }
        }

        public static Product? UpdateSelectedProduct(string searchText, IEnumerable<Product> filteredProducts)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return null;

            var matchingProduct = filteredProducts
                .FirstOrDefault(p => p.Name.StartsWith(searchText, StringComparison.InvariantCultureIgnoreCase));

            if (matchingProduct != null)
                return matchingProduct;

            // Если товар не найден, создаём новый с введённым именем
            return ProductBuilder
                .Create()
                .SetName(searchText)
                .SetQuantity(0) // Значение по умолчанию
                .SetPrice(0)    // Значение по умолчанию
                .Build();
        }

        // Добавление или обновление продукта
        public async Task<bool> AddOrUpdateProductAsync(Product selectedProduct, int storeId)
        {
            try
            {
                var existingProduct = await LoadProductsAsync();

                // Проверка на существование товара в магазине
                var foundProduct = existingProduct?
                    .FirstOrDefault(p => p.Name.Equals(selectedProduct.Name, StringComparison.InvariantCultureIgnoreCase)
                                         && p.StoreId == storeId);

                if (foundProduct != null)
                {
                    foundProduct.Quantity = selectedProduct.Quantity;
                    foundProduct.Price = selectedProduct.Price;
                    await _productRepository.UpdateAsync(foundProduct);
                    return true; // Успешно обновили товар
                }

                // Если товар не найден, создаём новый
                var newProduct = ProductBuilder
                    .Create()
                    .SetName(selectedProduct.Name)
                    .SetQuantity(selectedProduct.Quantity)
                    .SetPrice(selectedProduct.Price)
                    .SetStoreId(storeId)
                    .Build();

                await _productRepository.AddAsync(newProduct);
                return true; // Успешно добавили товар
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ошибка при добавлении или обновлении товара", ex);
            }
        }

        public async Task<Product?> SearchCheapestProductAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText)) return null;

            try
            {
                // Получаем данные один раз и приводим их к коллекции
                var products = (await _productRepository.GetAllAsync())?.ToList();

                if (products == null || products.Count == 0) return null;

                // Фильтруем по названию и ищем самый дешевый товар
                return products
                    .Where(p => p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .MinBy(p => p.Price);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<IEnumerable<string>> LoadProductNamesAsync() => 
            (await _productRepository.GetAllAsync())?.Select(p => p.Name).Distinct() ?? [];

        public async Task<(int? StoreId, decimal? Cost)> FindCheapestStoreAsync(IEnumerable<PurchaseItem> purchaseItems)
        {
            ArgumentNullException.ThrowIfNull(purchaseItems, nameof(purchaseItems));

            var purchaseItemsList = purchaseItems.ToList(); // Один раз преобразуем в список
            var allProducts = (await _productRepository.GetAllAsync())?.ToList(); // Преобразуем в список для однократного прохода

            if (allProducts == null || allProducts.Count == 0)
                return (null, null);

            // Группируем товары по StoreId
            var groupedProductsByStore = allProducts
                .GroupBy(p => p.StoreId)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList() // Преобразуем группы в списки для оптимального поиска
                );

            int? cheapestStoreId = null;
            decimal? cheapestCost = null;

            foreach (var store in groupedProductsByStore)
            {
                // Рассчитываем общую стоимость покупки для данного магазина
                var totalCost = 0m;
                var canFulfillOrder = true;

                foreach (var purchaseItem in purchaseItemsList)
                {
                    var product = store.Value.FirstOrDefault(p =>
                        p.Name.Equals(purchaseItem.Name, StringComparison.OrdinalIgnoreCase));

                    if (product == null || product.Quantity < purchaseItem.Quantity)
                    {
                        canFulfillOrder = false;
                        break; // Прерываем вычисление для этого магазина
                    }

                    totalCost += product.Price * purchaseItem.Quantity;
                }

                // Проверяем, может ли магазин предложить самый дешёвый вариант
                if (!canFulfillOrder || (cheapestCost != null && !(totalCost < cheapestCost))) continue;
                cheapestCost = totalCost;
                cheapestStoreId = store.Key;
            }
            return (cheapestStoreId, cheapestCost);
        }



    }
}

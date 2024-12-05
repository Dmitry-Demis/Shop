using StoreCatalogBLL.Model;
using StoreCatalogDAL.Model;
using System.Collections.ObjectModel;

namespace StoreCatalogBLL;

public class CartService(ProductService productService)
{
    private readonly ProductService _productService = productService ?? throw new ArgumentNullException(nameof(productService));

    // Хранение товаров в корзине для каждого магазина
    private readonly Dictionary<int, List<CartItem>> _cartItems = [];

    // Последний магазин, в который был добавлен товар
    public int? LastAddedStoreId { get; private set; }

    // Сет идентификаторов магазинов, в которые были добавлены товары
    public HashSet<int> AddedStoreIds { get; } = [];

    // Добавление товара в корзину
    public void AddToCart(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);

        // Получаем список товаров для магазина, если его нет - создаём новый
        if (!_cartItems.ContainsKey(product.StoreId))
        {
            _cartItems[product.StoreId] = [];
        }

        var storeCart = _cartItems[product.StoreId];

        // Проверяем, есть ли уже товар в корзине для данного магазина
        var existingItem = storeCart.FirstOrDefault(item => item.Name == product.Name);

        if (existingItem != null)
        {
            // Если товар есть, увеличиваем количество
            existingItem.Quantity += quantity;
        }
        else
        {
            // Если товара нет, добавляем новый
            storeCart.Add(new CartItem { Name = product.Name, Quantity = quantity, Price = product.Price });
        }

        // Обновляем последний магазин, куда был добавлен товар
        LastAddedStoreId = product.StoreId;

        // Добавляем магазин в список добавленных
        AddedStoreIds.Add(product.StoreId);

        // Уменьшаем количество товара в продукте
        product.Quantity -= quantity;
    }

    // Удаление товара из корзины
    public void RemoveFromCart(Product product)
    {
        // Проверяем, есть ли товары в корзине для этого магазина
        if (!_cartItems.TryGetValue(product.StoreId, out var storeCart)) return;

        // Ищем товар в корзине
        var item = storeCart.FirstOrDefault(i => i.Name == product.Name);
        if (item == null) return;  // Если товар не найден, выходим

        // Возвращаем количество товара обратно
        product.Quantity += item.Quantity;

        // Удаляем товар из корзины
        storeCart.Remove(item);

        // Если корзина магазина пуста, удаляем магазин из корзины
        if (!storeCart.Any()) _cartItems.Remove(product.StoreId);

        // Если корзина пуста, очищаем список добавленных магазинов
        if (!_cartItems.Any()) AddedStoreIds.Clear();
    }

    // Получаем общую стоимость по определённому магазину
    public decimal GetTotalCost(int storeId) => _cartItems.TryGetValue(storeId, out var storeItem) ? storeItem.Sum(item => item.Total) : 0;

    // Получаем товары в корзине для конкретного магазина
    public async Task<IEnumerable<CartItem>> LoadCartItemsByStoreAsync(int storeId) =>
        // Simulate async data retrieval (e.g., database, API call, etc.)
        await Task.Run(() => _cartItems.TryGetValue(storeId, out var items) ? items : []);

    // Асинхронное оформление заказа
    public async Task Checkout(int storeId)
    {
        try
        {
            // Загружаем все товары для выбранного магазина
            var cartItemsForStore = (await LoadCartItemsByStoreAsync(storeId)).ToList();

            // Получаем все продукты для выбранного магазина из репозитория
            var products = await _productService.LoadProductsByStoreAsync(storeId);

            // Создаём словарь для быстрого поиска продукта по его имени
            var productDictionary = products.ToDictionary(p => p.Name, p => p);

            // Используем LINQ для создания списка задач асинхронной обработки
            var tasks = cartItemsForStore
                .Where(cartItem => productDictionary.ContainsKey(cartItem.Name)) // Проверка, что продукт существует в словаре
                .Select(cartItem => _productService.AddOrUpdateProductAsync(productDictionary[cartItem.Name], storeId)) // Обновление товара
                .ToList();

            // Ожидаем завершения всех асинхронных операций
            await Task.WhenAll(tasks);

            // Удаляем все товары для данного магазина из корзины
            if (_cartItems.Remove(storeId, out var storeCart))
            {
                // Если товары были удалены, также удаляем магазин из списка добавленных
                AddedStoreIds.Remove(storeId);
            }
        }
        catch (Exception ex)
        {
            // Логирование ошибки (если нужно, можно добавить более детальную обработку ошибок)
            Console.WriteLine($"Ошибка при оформлении заказа: {ex.Message}");
        }
    }
    public bool GetAffordableProducts(decimal budget, IEnumerable<Product> availableProducts)
    {
        var remainingAmount = budget;
        bool addedProductThisRound;
        var storeId = 0;

        // Цикл, пока есть деньги на покупку товаров
        do
        {
            addedProductThisRound = false;

            // Проходим по каждому товару, сортируя по цене (от дешевых к дорогим)
            foreach (var product in availableProducts.OrderBy(p => p.Price))
            {
                var price = product.Price;

                // Если есть деньги и товар в наличии
                if (remainingAmount < price || product.Quantity <= 0) continue;
                // Проверяем, есть ли этот товар в корзине

                AddToCart(product, 1);
                storeId = product.StoreId;
                // Уменьшаем оставшуюся сумму
                remainingAmount -= price;
                addedProductThisRound = true; // Признак того, что товар был добавлен в корзину
            }

            // Повторяем цикл, пока есть деньги и товары, которые можно добавить
        } while (remainingAmount > 0 && addedProductThisRound);

        if (_cartItems.TryGetValue(storeId, out var storeCartItems))
        {
            // Если список товаров существует, проверяем его количество
            return storeCartItems.Count != 0;
        }

        // Если ключа нет в словаре (например, магазин ещё не добавлен в корзину)
        return false;
    }
}

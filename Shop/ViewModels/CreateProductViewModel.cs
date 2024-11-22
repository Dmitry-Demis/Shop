using MathCore.ViewModels;
using MathCore.WPF.Commands;
using Microsoft.EntityFrameworkCore;
using Shop.DAL.Models;
using Shop.DAL.Models.Builders;
using Shop.DAL.Repositories;
using Shop.ViewModels.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Shop.ViewModels
{
    public class CreateProductViewModel(IRepository<Product> productRepository, IUserDialogService userDialog) : ViewModel
    {
        private string _Title = "Создать товар";
        public string Title
        {
            get => _Title;
            set => Set(ref _Title, value);
        }
        // Проперти для данных нового магазина
        private string _productName = null!;
        public string ProductName
        {
            get => _productName;
            set => Set(ref _productName, value);
        }

        // Команда для создания товара
        private ICommand? _createProductCommand;
        public ICommand CreateProductCommand => _createProductCommand ??= new LambdaCommandAsync(OnCreateProductExecutedAsync, CanCreateProductExecute);

        private async Task<Product?> FindExistingProductAsync(string normalisedProductName)
        {
            // Загружаем все продукты
            var products = await productRepository.Items.ToListAsync();

            // Асинхронно перебираем продукты
            foreach (var product in products)
            {
                // Сравниваем имена продуктов
                if (product.Name.Trim().ToLower() == normalisedProductName)
                {
                    return product; // Возвращаем найденный продукт
                }

                // Используем await внутри цикла, если нужны дополнительные асинхронные действия
                await Task.Yield(); // Имитация асинхронной работы
            }

            return null; // Продукт не найден
        }

        private async Task OnCreateProductExecutedAsync()
        {
            var normalisedProductName = ProductName.Trim().ToLower();

            // Ищем существующий продукт
            var existingProduct = await FindExistingProductAsync(normalisedProductName);

            if (existingProduct != null)
            {
                userDialog?.ShowInformation($"Товар \"{ProductName}\" уже существует");
                return;
            }

            // Создаём новый продукт
            var newProduct = new ProductBuilder()
                .SetName(ProductName.Trim())
                .Build();

            try
            {
                // Сохраняем новый продукт
                await productRepository.AddAsync(newProduct);

                ProductName = string.Empty;
                userDialog?.ShowInformation($"Товар \"{normalisedProductName}\" успешно добавлен");
                userDialog?.Close();
            }
            catch (Exception ex)
            {
                userDialog?.ShowError($"Ошибка при добавлении товара: {ex.Message}");
            }
        }

        private bool CanCreateProductExecute() => !string.IsNullOrWhiteSpace(ProductName); // Товар можно создать, если название не пустое
    }

}

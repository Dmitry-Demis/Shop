using MathCore.ViewModels;
using MathCore.WPF.Commands;
using Shop.DAL.Models;
using Shop.DAL.Models.Builders;
using Shop.DAL.Repositories;
using Shop.ViewModels.Services;
using Shop.ViewModels.Wrappers;
using System.Windows.Input;

namespace Shop.ViewModels
{
    public class CreateProductViewModel(
         IRepository<Product> productRepository,
         IUserDialogService userDialog
     ) : ViewModel
    {
        // Primary Constructor для управления зависимостями
        private IRepository<Product> ProductRepository { get; } = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        private IUserDialogService UserDialog { get; } = userDialog ?? throw new ArgumentNullException(nameof(userDialog));

        // Свойства для привязки в UI
        public string Title { get; set; } = "Создать продукт";

        private ProductWrapper _selectedProduct = new(new Product());
        public ProductWrapper SelectedProduct
        {
            get => _selectedProduct;
            set => Set(ref _selectedProduct, value);
        }

        private ICommand? _createProductCommand;
        public ICommand CreateProductCommand => _createProductCommand ??= new LambdaCommandAsync(OnCreateProductExecutedAsync, CanCreateProductExecute);

        private bool CanCreateProductExecute() =>
            !string.IsNullOrWhiteSpace(SelectedProduct.Name);

        private async Task OnCreateProductExecutedAsync()
        {
            try
            {
                // Проверка на существующий продукт
                var existingProduct = (await ProductRepository.GetAllAsync())
                    ?.FirstOrDefault(p => p.Name.ToLower() == SelectedProduct.Name.ToLower());

                if (existingProduct is not null)
                {
                    UserDialog.ShowWarning($@"Продукт с названием ""{SelectedProduct.Name}"" уже существует.");
                    return;
                }

                // Создание нового продукта
                var newProduct = ProductBuilder.Create()
                                               .SetName(SelectedProduct.Name)
                                               .Build();

                await ProductRepository.AddAsync(newProduct);
                UserDialog.ShowInformation($"Продукт \"{newProduct.Name}\" успешно создан.");
                UserDialog.Close();
            }
            catch (Exception ex)
            {
                UserDialog.ShowError($"Ошибка при создании продукта: {ex.Message}");
            }
        }
    }
}

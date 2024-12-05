using System.Windows.Input;
using MathCore.ViewModels;
using MathCore.WPF.Commands;
using StoreCatalogBLL;
using StoreCatalogDAL.Model;
using StoreCatalogPresentation.Models;
using StoreCatalogPresentation.ViewModels.Services.Dialogs;

namespace StoreCatalogPresentation.ViewModels
{
    public class CreateStoreViewModel
        (StoreService storeService
        , IUserDialogService userDialog) : ViewModel
    {

        public string Title { get; init; } = "Создать магазин";

        private readonly StoreService _storeService = storeService ?? throw new ArgumentNullException(nameof(storeService));
        private readonly IUserDialogService _userDialog = userDialog ?? throw new ArgumentNullException(nameof(userDialog));

        private StoreWrapper _selectedStore = new(new Store());
        public StoreWrapper SelectedStore
        {
            get => _selectedStore;
            set => Set(ref _selectedStore, value);
        }

        private ICommand? _createStoreCommand;
        public ICommand CreateStoreCommand => _createStoreCommand ??= new LambdaCommandAsync(OnCreateStoreExecutedAsync, CanCreateStoreExecute);

        private bool CanCreateStoreExecute() =>
            Services.Extensions.StringExtensions.IsNotNullOrWhiteSpace(SelectedStore.Name, SelectedStore.Address);

        // Метод, вызываемый при выполнении команды
        private async Task OnCreateStoreExecutedAsync()
        {
            try
            {
                // Если создание прошло успешно
                if (await _storeService.CreateStoreAsync(SelectedStore))
                {
                    _userDialog.ShowInformation($"Магазин \"{SelectedStore.Name}\" успешно создан.");
                }
            }
            catch (ArgumentException ex)
            {
                // Ошибка, если имя или адрес пустые
                _userDialog.ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                // Общая ошибка при создании магазина
                _userDialog.ShowError($"Ошибка при создании магазина: {ex.Message}");
            }
            finally
            {
                // Закрытие диалога (если нужно)
                _userDialog.Close();
            }
        }
    }
}

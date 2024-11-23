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
    public class CreateStoreViewModel(
    IRepository<Store> storeRepository,
    IUserDialogService userDialog
) : ViewModel
    {
        // Primary Constructor, без необходимости в явных полях
        private IRepository<Store> StoreRepository { get; } = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
        private IUserDialogService UserDialog { get; } = userDialog ?? throw new ArgumentNullException(nameof(userDialog));

        // Свойства для привязки в UI
        public string Title { get; set; } = "Создать магазин";

        private StoreWrapper _selectedStore = new(new Store());
        public StoreWrapper SelectedStore
        {
            get => _selectedStore;
            set => Set(ref _selectedStore, value);
        }

        private ICommand? _createStoreCommand;
        public ICommand CreateStoreCommand => _createStoreCommand ??= new LambdaCommandAsync(OnCreateStoreExecutedAsync, CanCreateStoreExecute);

        private bool CanCreateStoreExecute() =>
            !string.IsNullOrWhiteSpace(SelectedStore.Name) && !string.IsNullOrWhiteSpace(SelectedStore.Address);

        private async Task OnCreateStoreExecutedAsync()
        {
            try
            {
                var newStore = StoreBuilder.Create()
                                           .SetName(SelectedStore.Name)
                                           .SetAddress(SelectedStore.Address)
                                           .Build()
                                           ;

                await StoreRepository.AddAsync(newStore);
                UserDialog.ShowInformation("Магазин успешно создан.");
                UserDialog.Close();
            }
            catch (Exception ex)
            {
                UserDialog.ShowError($"Ошибка при создании магазина: {ex.Message}", "Ошибка");
            }
        }
    }
}

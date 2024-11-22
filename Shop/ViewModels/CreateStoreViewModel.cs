using MathCore.ViewModels;
using MathCore.WPF.Commands;
using Shop.DAL.Models;
using Shop.DAL.Models.Builders;
using Shop.DAL.Repositories;
using Shop.ViewModels.Services;
using Shop.ViewModels.Wrappers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Shop.ViewModels
{

    public class CreateStoreViewModel : ViewModel
    {
        private readonly IRepository<Store> _storeRepository;
        private readonly IUserDialogService _userDialog;

        public CreateStoreViewModel(
            IRepository<Store> storeRepository,
            IUserDialogService userDialog)
        {
            _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
            _userDialog = userDialog ?? throw new ArgumentNullException(nameof(userDialog));
            SelectedStore = new StoreWrapper(new Store());
        }

        // Свойства для привязки в UI
        private string _title = "Создать магазин";
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        private StoreWrapper _selectedStore;
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
                // Используем Builder для создания нового магазина
                var newStore = StoreBuilder.Create()
                                           .SetName(SelectedStore.Name)
                                           .SetAddress(SelectedStore.Address)
                                           .Build();

                // Сохраняем магазин в репозитории
                await _storeRepository.AddAsync(newStore);

                // Обновляем коллекцию магазинов в UI
                //Stores.Add(newStore);

                // Уведомляем пользователя об успешном создании
                _userDialog.ShowInformation("Магазин успешно создан.");

                // Закрываем диалог
                _userDialog.Close();
                SelectedStore = new StoreWrapper(new Store());
            }
            catch (Exception ex)
            {
                // Логирование или отображение ошибки пользователю
                _userDialog.ShowError($"Ошибка при создании магазина: {ex.Message}", "Ошибка");
            }
        }
    }
}

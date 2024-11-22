using MathCore.ViewModels;
using MathCore.WPF.Commands;
using Microsoft.EntityFrameworkCore;
using Shop.DAL.Models;
using Shop.DAL.Models.Builders;
using Shop.DAL.Repositories;
using Shop.ViewModels.Services;
using System;
using System.Collections.Generic;
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

            Stores = new ObservableCollection<Store>();
            UniqueStoreNames = new ObservableCollection<string>();

            _ = LoadStoresAsync();
        }

        private string _title = "Создать магазин";
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        private string _storeName = null!;
        public string StoreName
        {
            get => _storeName;
            set => Set(ref _storeName, value);
        }

        private string _storeAddress = null!;
        public string StoreAddress
        {
            get => _storeAddress;
            set => Set(ref _storeAddress, value);
        }

        private ObservableCollection<Store> _stores;
        public ObservableCollection<Store> Stores
        {
            get => _stores;
            set => Set(ref _stores, value);
        }

        private ObservableCollection<string> _uniqueStoreNames;
        public ObservableCollection<string> UniqueStoreNames
        {
            get => _uniqueStoreNames;
            set => Set(ref _uniqueStoreNames, value);
        }

        private ICommand? _createStoreCommand;
        public ICommand CreateStoreCommand => _createStoreCommand ??= new LambdaCommandAsync(OnCreateStoreExecutedAsync, CanCreateStoreExecute);

        private bool CanCreateStoreExecute() =>
            !string.IsNullOrWhiteSpace(StoreName) && !string.IsNullOrWhiteSpace(StoreAddress);

        private async Task OnCreateStoreExecutedAsync()
        {
            // Приводим вводимые данные к нижнему регистру для сравнения
            var storeNameLower = StoreName.ToLower().Trim();
            var storeAddressLower = StoreAddress.ToLower().Trim();

            // Проверяем, существует ли магазин с таким же именем и адресом
            var duplicateStore = Stores.FirstOrDefault(
                s => s.Name.ToLower() == storeNameLower &&
                     s.Address.ToLower() == storeAddressLower
            );

            if (duplicateStore != null)
            {
                _userDialog?.ShowWarning($"Магазин с именем '{StoreName}' и адресом '{StoreAddress}' уже существует.");
                return;
            }

            // Создаём новый магазин
            var newStore = new StoreBuilder()
                .SetName(StoreName.Trim())
                .SetAddress(StoreAddress.Trim())
                .Build();

            // Добавляем магазин в репозиторий
            await _storeRepository.AddAsync(newStore);

            // Добавляем уникальное имя в коллекцию
            if (!UniqueStoreNames.Contains(newStore.Name))
            {
                UniqueStoreNames.Add(newStore.Name);
            }

            // Добавляем магазин в коллекцию для отображения
            Stores.Add(newStore);

            // Сбрасываем значения
            StoreName = string.Empty;
            StoreAddress = string.Empty;

            // Сообщаем об успешном добавлении
            _userDialog?.ShowInformation($"Магазин '{newStore.Name}' успешно создан!");
            _userDialog?.Close();
        }


        private async Task LoadStoresAsync()
        {
            var stores = await _storeRepository.Items.ToListAsync();

            Stores = new ObservableCollection<Store>(stores);
            UniqueStoreNames = new ObservableCollection<string>(
                stores.Select(s => s.Name).Distinct()
            );
        }
    }
}

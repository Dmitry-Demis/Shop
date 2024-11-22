using Shop.DAL.Models;

namespace Shop.ViewModels.Wrappers
{
    public class StoreWrapper(Store store) : Wrapper
    {
        private readonly Store _store = store ?? throw new ArgumentNullException(nameof(store));

        public int Id => _store.Id;
        public string Name
        {
            get => _store.Name;
            set
            {
                if (_store.Name == value) return;
                _store.Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public string Address
        {
            get => _store.Address;
            set
            {
                if (_store.Address == value) return;
                _store.Address = value;
                OnPropertyChanged(nameof(Address));
            }
        }
        public string Code => _store.Code; // Только для чтения
        public override string ToString() => _store.ToString();
        public Store Unwrap() => _store;
    }
}

using StoreCatalogDAL.Model;

namespace StoreCatalogPresentation.Models
{
    public class StoreWrapper(Store store) : Wrapper<Store>(store)
    {
        public int Id => Base.Id;
        public string Code => Base.Code;

        public string Name
        {
            get => Base.Name;
            set => Set(() => Base.Name, v => Base.Name = v, value);
        }

        public string Address
        {
            get => Base.Address;
            set => Set(() => Base.Address, v => Base.Address = v, value);
        }
    }
}

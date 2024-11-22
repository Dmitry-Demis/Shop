using Shop.DAL.Models;

namespace Shop.ViewModels.Wrappers
{
    public class ProductWrapper(Product product) : Wrapper
    {
        private readonly Product _product = product ?? throw new ArgumentNullException(nameof(product));

        public int Id => _product.Id;
        public string Name
        {
            get => _product.Name;
            set
            {
                if (_product.Name == value) return;
                _product.Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public override string ToString() => _product?.ToString();
        public Product Unwrap() => _product;
    }
}

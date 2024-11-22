using MathCore.ViewModels;
using MathCore.WPF.Commands;
using Shop.Views.Windows;
using System.Windows;
using System.Windows.Input;

namespace Shop.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        private string _Title = "Магазин продуктов";
        public string Title
        {
            get => _Title;
            set => Set(ref _Title, value);
        }

        // Универсальный метод для открытия окон
        private void OpenWindow<TWindow>() where TWindow : Window, new()
        {
            var window = new TWindow
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        }

        // Команда для создания магазина
        private ICommand? _createStoreCommand;
        public ICommand CreateStoreCommand => _createStoreCommand ??= new LambdaCommand(() => OpenWindow<CreateStoreWindow>());

        // Команда для создания продукта
        private ICommand? _createProductCommand;
        public ICommand CreateProductCommand => _createProductCommand ??= new LambdaCommand(() => OpenWindow<CreateProductWindow>());

        // Команда для завоза партии товаров
        private ICommand? _stockProductCommand;
        public ICommand StockProductCommand => _stockProductCommand ??= new LambdaCommand(() => OpenWindow<StockProductWindow>());
    }
}

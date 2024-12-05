using System.Windows;

namespace StoreCatalogPresentation.ViewModels.Services.Dialogs
{
    internal class WindowsUserDialogService : IUserDialogService
    {
        private static Window? ActiveWindow => Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
        public void ShowInformation(string information, string caption) => MessageBox.Show(information, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        public void ShowWarning(string message, string caption) => MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
        public void ShowError(string message, string caption) => MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        public bool Confirm(string message, string caption, bool exclamation = false) =>
            MessageBox.Show(
                message,
                caption,
                MessageBoxButton.YesNo,
                exclamation ? MessageBoxImage.Exclamation : MessageBoxImage.Question)
            == MessageBoxResult.Yes;
        public void Close() => ActiveWindow?.Close();
    }
}

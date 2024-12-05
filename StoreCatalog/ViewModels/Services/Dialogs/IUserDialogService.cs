namespace StoreCatalogPresentation.ViewModels.Services.Dialogs
{
    public interface IUserDialogService
    {
        void ShowInformation(string information, string caption = "Информация");
        void ShowWarning(string message, string caption = "Предупреждение");
        void ShowError(string message, string caption = "Ошибка");
        bool Confirm(string message, string caption, bool exclamation = false);
        void Close();
    }
}

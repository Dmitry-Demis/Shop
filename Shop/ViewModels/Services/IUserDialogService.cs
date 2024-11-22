namespace Shop.ViewModels.Services
{
    public interface IUserDialogService
    {
        void ShowInformation(string Information, string Caption = "Информация");

        void ShowWarning(string Message, string Caption = "Предупреждение");

        void ShowError(string Message, string Caption = "Ошибка");

        bool Confirm(string Message, string Caption, bool Exclamation = false);

        void Close();
    }
}

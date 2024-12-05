using Microsoft.Extensions.DependencyInjection;

namespace StoreCatalogPresentation.ViewModels.Services.Dialogs
{
    internal static class WindowsUserDialogRegistration
    {
        public static IServiceCollection AddWindowsUserDialogs(this IServiceCollection services) => services
            .AddSingleton<IUserDialogService, WindowsUserDialogService>()
            ;
    }
}

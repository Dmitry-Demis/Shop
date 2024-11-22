using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shop.DAL.Services.DataAccess;
using Shop.ViewModels.Services;
using System.Windows;

namespace Shop
{
    public partial class App : Application
    {
        private static IHost? __Host;

        public static IHost Host => __Host
            ??= Program.CreateHostBuilder(Environment.GetCommandLineArgs()).Build();

        public static IServiceProvider Services => Host.Services;

        internal static void ConfigureServices(HostBuilderContext host, IServiceCollection services)
        {
            // Получаем конфигурацию
            var configuration = host.Configuration;

            // Вызываем метод расширения AddStorage
            services.AddStorage(configuration);
            services.AddViewModels();
            services.AddTransient<IUserDialogService, WindowsUserDialogService>();
        }


        protected override async void OnStartup(StartupEventArgs e)
        {
            var host = Host;
            base.OnStartup(e);
            await host.StartAsync();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using var host = Host;
            base.OnExit(e);
            await host.StopAsync();
        }
    }
}

using Microsoft.Extensions.Hosting;

namespace StoreCatalogPresentation
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
           .CreateDefaultBuilder(args)
           .ConfigureServices(App.ConfigureServices);
    }
}

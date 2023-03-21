using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PruebaTecnica.ViewModel;

using Syncfusion.Licensing;

using System.Windows;

namespace PruebaTecnica {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        private readonly IHost? AppHost;

        public App() {

            SyncfusionLicenseProvider.RegisterLicense(Constants.KEY);

            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => {

                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<MainWindowViewModel>();

                }).Build();
        }

        protected override async void OnStartup(StartupEventArgs e) {

            await AppHost!.StartAsync();

            var startupWindow = AppHost.Services.GetRequiredService<MainWindow>();
            startupWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e) {

            await AppHost!.StopAsync();

            base.OnExit(e);
        }
    }
}

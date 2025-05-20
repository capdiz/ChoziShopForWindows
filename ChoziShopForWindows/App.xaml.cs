using ChoziShop.Data;
using ChoziShop.Data.Repository;
using ChoziShopForWindows.Data;
using ChoziShopForWindows.MerchantsApi;
using ChoziShopForWindows.Services;
using ChoziShopForWindows.ViewModels;
using ChoziShopForWindows.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ChoziShopForWindows;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddDatabaseContext(DbFileConfig.ConnectionString);
                services.AddScoped<IDataObjects, DataObjects>();
                services.AddScoped<IDialogService, MessageBoxService>();
                services.AddScoped<IAuthTokenProvider, AuthTokenProvider>();
                services.AddHttpClient<ITransactionService, TransactionService>((client) =>
                {
                    client.BaseAddress = new Uri(HttpService.BASE_PAYMENTS_URL);
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                });
                // Add internet monitoring service
                services.AddSingleton<InternetConnectivityMonitorService>();
                var service = services.BuildServiceProvider();
                var statusService = service.GetRequiredService<InternetConnectivityMonitorService>();
                services.AddSingleton<MainWindowViewModel>();
                services.AddSingleton<OrdersViewModel>();


                services.AddTransient<HomeView>(provider => new HomeView
                {
                    DataContext = provider.GetRequiredService<OrdersViewModel>()
                });
                services.AddTransient<EasyPosView>(provider => 
                {                    
                    var easyPosView = new EasyPosView();
                    easyPosView.DataContext = provider.GetRequiredService<OrdersViewModel>();
                    return easyPosView;

                });
                services.AddTransient<OrderCheckoutDialog>(provider =>
                {
                    
                    var ordersViewModel = provider.GetRequiredService<OrdersViewModel>();
                    return new OrderCheckoutDialog(ordersViewModel);
                });
                services.AddTransient<ShopsView>(provider => new ShopsView
                {
                    DataContext = provider.GetRequiredService<OrdersViewModel>()
                });
                services.AddTransient<CreateShopView>(provider => new CreateShopView
                {
                    DataContext = provider.GetRequiredService<OrdersViewModel>()

                });
                services.AddTransient<ShopKeepersView>(provider => new ShopKeepersView
                {
                    DataContext = provider.GetRequiredService<OrdersViewModel>()
                });
                services.AddTransient<OrdersView>(provider => new OrdersView
                {
                    DataContext = provider.GetRequiredService<OrdersViewModel>()
                });
                services.AddTransient<InventoryView>(provider => new InventoryView
                {
                    DataContext = provider.GetRequiredService<OrdersViewModel>()
                });
                services.AddTransient<PaymentsView>(provider => new PaymentsView
                {
                    DataContext = provider.GetRequiredService<OrdersViewModel>()
                });
                services.AddTransient<DiscountsView>(provider => new DiscountsView
                {
                    DataContext = provider.GetRequiredService<OrdersViewModel>()
                });
                services.AddTransient<SettingsView>(provider => new SettingsView
                {
                    DataContext = provider.GetRequiredService<OrdersViewModel>()
                });
                services.AddTransient<PromotionsView>(provider => new PromotionsView
                {
                    DataContext = provider.GetRequiredService<OrdersViewModel>()
                });
                services.AddTransient<ScheduledOrdersView>(provider => new ScheduledOrdersView
                {
                    DataContext = provider.GetRequiredService<OrdersViewModel>()
                });

                services.AddSingleton<DatabaseWatcher>(provider => new DatabaseWatcher(DbFileConfig.ConnectionString, () =>
                provider.GetService<ILogger<DatabaseWatcher>>().LogInformation("Database file changed")));


                services.AddSingleton<MainWindow>(provider =>
                {
                    var window = new MainWindow();
                    window.DataContext = provider.GetService<MainWindowViewModel>();
                    return window;
                });


            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        CheckAndStartService();
        var directoryPath = new DirectoryInfo(DbFileConfig.DbFilePath);
        if (!directoryPath.Exists)
        {
            Directory.CreateDirectory(directoryPath.FullName);
        }

        var security = directoryPath.GetAccessControl();
        security.AddAccessRule(new FileSystemAccessRule("Users",
            FileSystemRights.FullControl,
            InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
            PropagationFlags.None,
            AccessControlType.Allow));
        directoryPath.SetAccessControl(security);


        using (var scope = _host.Services.CreateScope())
        {
            try
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<DatabaseContext>();
                context.Database.Migrate();
                var tables = context.Database.GetAppliedMigrations();
                Debug.WriteLine("Database migrated. Migrations ", string.Join(", ", tables));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Debug.WriteLine("Error message: {message}", ex.Message);
            }
        }

        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    private static void CleanUp(bool disposed)
    {
        // orce garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
    

    protected override async void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        await _host.StopAsync();
        _host.Dispose();
    }

    private void CheckAndStartService()
    {
        try
        {
            var servicePath = ServiceManager.GetServicePath();
            Debug.WriteLine($"Service path resolved to: {servicePath}");

            if (!ServiceManager.IsServiceInstalled())
            {
                Debug.WriteLine("Installing service...");
                ServiceManager.InstallService();
            }

            if (!ServiceManager.IsServiceRunning())
            {
                Debug.WriteLine("Starting service...");
                ServiceManager.StartService();
            }
        }
        catch (Exception ex)
        {
           // MessageBox.Show($"Failed to initialize service: {ex.Message}",
            //    "Service Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            Debug.WriteLine(ex.Message);
           // Current.Shutdown();
        }
    }
}


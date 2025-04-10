using ChoziShop.Data;
using ChoziShop.Data.Repository;
using ChoziShopForWindows.Data;
using ChoziShopForWindows.ViewModels;
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
using System.Security.AccessControl;
using System.Threading.Tasks;
using System.Windows;

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
                services.AddTransient<MainWindowViewModel>();
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
        base.OnStartup(e);

        // CheckAndStartService();
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

    }

    protected override async void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        await _host.StopAsync();
        _host.Dispose();
    }

    private void CheckAndStartService()
    {
        //if (!Services.ServiceManager.IsServiceInstalled())
        //{
        //    Services.ServiceManager.InstallService();
        //}
        //if (!Services.ServiceManager.IsServiceRunning())
        //{
        //    Services.ServiceManager.StartService();
        //}
    }
}


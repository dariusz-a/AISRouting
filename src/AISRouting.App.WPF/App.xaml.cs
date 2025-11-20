using System.Windows;
using System.Windows.Threading;
using AISRouting.App.WPF.Services;
using AISRouting.App.WPF.ViewModels;
using AISRouting.Core.Services.Interfaces;
using AISRouting.Infrastructure.IO;
using AISRouting.Infrastructure.Parsers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AISRouting.App.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            ShowCriticalErrorDialog($"Application failed to start: executable missing or corrupted\n\n{ex.Message}");
            Shutdown(1);
        }

        // Register global exception handler
        DispatcherUnhandledException += OnDispatcherUnhandledException;
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddLogging(builder =>
        {
            builder.AddDebug();
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Infrastructure services
        services.AddSingleton<ISourceDataScanner, SourceDataScanner>();
        services.AddSingleton<IShipStaticDataLoader, ShipStaticDataParser>();
        services.AddSingleton<AISRouting.Infrastructure.Validation.IPathValidator, AISRouting.Infrastructure.Validation.PathValidator>();
        services.AddSingleton<AISRouting.Infrastructure.Parsers.ICsvParser<AISRouting.Core.Models.ShipDataOut>, AISRouting.Infrastructure.Parsers.ShipPositionCsvParser>();
        services.AddSingleton<IShipPositionLoader, AISRouting.Infrastructure.IO.ShipPositionLoader>();
        services.AddSingleton<AISRouting.Core.Services.Interfaces.IPathValidator, AISRouting.Infrastructure.Validation.PathValidator>();
        services.AddSingleton<AISRouting.Core.Services.Interfaces.IRouteExporter, AISRouting.Infrastructure.Persistence.RouteExporter>();

        // Core services
        services.AddSingleton<AISRouting.Core.Services.Interfaces.ITrackOptimizer, AISRouting.Core.Services.Implementations.TrackOptimizer>();
        services.AddSingleton<AISRouting.Core.Services.Interfaces.IPermissionService, AISRouting.Core.Services.Implementations.AlwaysAllowPermissionService>();

        // UI services
        services.AddSingleton<IFolderDialogService, FolderDialogService>();
        services.AddSingleton<IFileConflictDialogService, FileConflictDialogService>();

        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<ShipSelectionViewModel>();

        // Views
        services.AddTransient<MainWindow>();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var logger = _serviceProvider?.GetService<ILogger<App>>();
        logger?.LogCritical(e.Exception, "Unhandled exception occurred");

        ShowCriticalErrorDialog($"An unexpected error occurred:\n\n{e.Exception.Message}");
        e.Handled = true;
        Shutdown(1);
    }

    private void ShowCriticalErrorDialog(string message)
    {
        MessageBox.Show(
            message,
            "Critical Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}


using AISRouting.App.WPF.ViewModels;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using WpfApp = AISRouting.App.WPF.App;
using MainWindow = AISRouting.App.WPF.MainWindow;

namespace AISRouting.Tests.IntegrationTests
{
    [TestFixture]
    [Apartment(System.Threading.ApartmentState.STA)]
    public class ApplicationStartupTests
    {
        [Test]
        [Ignore("Requires WPF Application instance which can cause conflicts in test runner")]
        public void Application_CanBuildServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var app = new WpfApp();

            // Use reflection to call ConfigureServices (it's private)
            var configureServicesMethod = typeof(WpfApp).GetMethod("ConfigureServices", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            configureServicesMethod?.Invoke(app, new object[] { services });
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            serviceProvider.Should().NotBeNull();
            var mainViewModel = serviceProvider.GetService<MainViewModel>();
            mainViewModel.Should().NotBeNull();
        }

        [Test]
        [Ignore("Requires WPF Application instance which can cause conflicts in test runner")]
        public void Application_CanResolveMainWindow()
        {
            // Arrange
            var services = new ServiceCollection();
            var app = new WpfApp();

            var configureServicesMethod = typeof(WpfApp).GetMethod("ConfigureServices",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            configureServicesMethod?.Invoke(app, new object[] { services });
            var serviceProvider = services.BuildServiceProvider();
            var mainWindow = serviceProvider.GetService<MainWindow>();

            // Assert
            mainWindow.Should().NotBeNull();
            mainWindow!.DataContext.Should().BeOfType<MainViewModel>();
        }

        [Test]
        [Ignore("Requires WPF Application instance which can cause conflicts in test runner")]
        public void Application_AllRequiredServicesRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var app = new WpfApp();

            var configureServicesMethod = typeof(WpfApp).GetMethod("ConfigureServices",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            configureServicesMethod?.Invoke(app, new object[] { services });
            var serviceProvider = services.BuildServiceProvider();

            // Assert - verify all critical services can be resolved
            serviceProvider.GetService<Core.Services.Interfaces.ISourceDataScanner>().Should().NotBeNull();
            serviceProvider.GetService<Core.Services.Interfaces.IShipStaticDataLoader>().Should().NotBeNull();
            serviceProvider.GetService<Core.Services.Interfaces.IFolderDialogService>().Should().NotBeNull();
            serviceProvider.GetService<MainViewModel>().Should().NotBeNull();
            serviceProvider.GetService<MainWindow>().Should().NotBeNull();
        }
    }
}

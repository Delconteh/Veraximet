using Beautify.Contracts.Service;
using Beautify.Infrastructure.Service;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.Versioning;
using System.Windows;
using Veraximet.Contracts.Repos;
using Veraximet.Contracts.Services;
using Veraximet.Infrastructure.Persistence;
using Veraximet.Infrastructure.Repos;
using Veraximet.Infrastructure.Services;
using Veraximet.Wpf.Features.Dashboard.ViewModels;
using Veraximet.Wpf.Features.Dashboard.Views;
using Veraximet.Wpf.Features.Instrument.ViewModels;
using Veraximet.Wpf.Features.Instrument.Views;
using Veraximet.Wpf.Features.Splash.ViewModels;
using Veraximet.Wpf.Features.Splash.VIews;
using Veraximet.Wpf.Features.Verification.ViewModels;
using Veraximet.Wpf.Features.Verification.Views;
using Veraximet.Wpf.Shell.Menu;
using Veraximet.Wpf.Shell.ViewModels;
using Veraximet.Wpf.Shell.Views;
using Veraximet.Wpf.Navigation;

namespace Veraximet
{
    [SupportedOSPlatform("windows")]
    public partial class App : Application
    {
        #region Variables
        private ServiceProvider? _serviceProvider;
        #endregion

        #region Methods
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            try
            {
                var services = new ServiceCollection();
                ConfigureServices(services);
                _serviceProvider = services.BuildServiceProvider();
                RegisterViews(_serviceProvider);

                var factory = _serviceProvider.GetRequiredService<IDbContextFactory<VeraximetDbContext>>();
                await using (var db = await factory.CreateDbContextAsync())
                    await db.Database.MigrateAsync();

                if (e.Args.Contains("--seed-demo", StringComparer.OrdinalIgnoreCase))
                    await _serviceProvider.GetRequiredService<IDemoInstrumentSeeder>().SeedAsync();

                var windows = _serviceProvider.GetRequiredService<IWindowService>();
                ShutdownMode = ShutdownMode.OnLastWindowClose;
                await windows.ShowWindowAsync<MainWindowVM>();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Veraximet — Startup error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-1);
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextFactory<VeraximetDbContext>(options =>
            {
                var connectionString = new SqliteConnectionStringBuilder
                {
                    DataSource = SqliteDatabasePath.Resolve(),
                    ForeignKeys = true
                }.ToString();
                options.UseSqlite(connectionString);
            });

            services.AddSingleton<IInstrumentRepo, InstrumentRepo>();
            services.AddTransient<IInstrumentService, InstrumentService>();
            services.AddTransient<IDashboardService, DashboardService>();
            services.AddTransient<IVerificationService, VerificationService>();
            services.AddTransient<IDemoInstrumentSeeder, DemoInstrumentSeeder>();

            services.AddSingleton<IWindowService, WindowService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddTransient<IInstrumentPageActions, InstrumentPageActions>();
            services.AddTransient<MenuProvider>();

            services.AddTransient<MainWindowVM>();
            services.AddTransient<DashboardVM>();
            services.AddTransient<InstrumentsVM>();
            services.AddTransient<VerificationHistoryVM>();
            services.AddTransient<DarkLogoSplashVM>();
            services.AddTransient<InstrumentEditorVM>();
            services.AddTransient<InstrumentDetailVM>();
            services.AddTransient<VerificationEditorVM>();
        }

        private static void RegisterViews(IServiceProvider provider)
        {
            var windows = provider.GetRequiredService<IWindowService>();
            var navigation = provider.GetRequiredService<INavigationService>();

            windows.Register<MainWindowVM, MainWindow>();

            navigation.Register<DarkLogoSplashVM, DarkLogoSplash>();
            navigation.Register<DashboardVM, Dashboard>();
            navigation.Register<InstrumentsVM, Instruments>();
            navigation.Register<VerificationHistoryVM, VerificationHistory>();
            navigation.Register<InstrumentDetailVM, InstrumentDetail>();
            navigation.Register<InstrumentEditorVM, InstrumentEditor>();
            navigation.Register<VerificationEditorVM, VerificationEditor>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
        #endregion
    }
}

using Beautify.Contracts.Service;
using Veraximet.Wpf.Features.Dashboard.ViewModels;
using Veraximet.Wpf.Features.Instrument.ViewModels;
using Veraximet.Wpf.Features.Verification.ViewModels;
using Veraximet.Wpf.Shell.ViewModels;

namespace Veraximet.Wpf.Shell.Menu
{
    public sealed class MenuProvider
    {
        private readonly INavigationService _navigation;

        public MenuProvider(INavigationService navigation)
        {
            _navigation = navigation;
        }

        public IReadOnlyList<MenuItemDefinition> Items { get; } = new List<MenuItemDefinition>
        {
            new MenuItemDefinition("Dashboard", "Dashboard", "MonitorDashboard"),
            new MenuItemDefinition("Instruments", "Instruments", "Ruler"),
            new MenuItemDefinition("Verifications", "Verifications", "PaperCheck")
        };

        public Task NavigateAsync(string key) => key switch
        {
            "Dashboard" =>
                _navigation.NavigateAsync<DashboardVM>(),

            "Instruments" =>
                _navigation.NavigateAsync<InstrumentsVM>(),

            "Verifications" =>
                _navigation.NavigateAsync<VerificationHistoryVM>(),

            _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Unknown menu item.")
        };
    }
}

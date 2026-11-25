using Beautify.Contracts.Service;
using Beautify.Wpf.Mvvm;
using System.Windows.Input;
using Veraximet.Wpf.Features.Splash.ViewModels;
using Veraximet.Wpf.Shell.Menu;

namespace Veraximet.Wpf.Shell.ViewModels
{
    public class MainWindowVM : ObservableObject
    {
        #region Variables
        private readonly IWindowService _windowService;
        private readonly INavigationService _navigation;
        private readonly MenuProvider _menuProvider;
        private object? _currentView;
        #endregion

        #region Properties
        public IReadOnlyList<MenuItemDefinition> MenuItems => _menuProvider.Items;
        public object? CurrentView
        {
            get => _currentView;
            private set => SetProperty(ref _currentView, value);
        }
        #endregion

        #region Commands
        public ICommand CloseWindowCommand { get; }
        public ICommand NavigateCommand { get; }
        #endregion

        #region Constructors
        public MainWindowVM(IWindowService windowService, INavigationService navigation, MenuProvider menuProvider)
        {
            _windowService = windowService;
            _navigation = navigation;
            _menuProvider = menuProvider;
            CloseWindowCommand = new AsyncRelayCommand(CloseWindowAsync);
            NavigateCommand = new AsyncRelayCommand<object>(NavigateAsync);
            _navigation.ViewChanged += (_, e) => CurrentView = e.CurrentView;
        }
        #endregion

        #region Methods
        public async Task InitializeAsync()
        {
            await _navigation.NavigateAsync<DarkLogoSplashVM>();
            CurrentView = _navigation.CurrentView;
        }

        private async Task NavigateAsync(object? parameter)
        {
            if (parameter is MenuItemDefinition item)
                await _menuProvider.NavigateAsync(item.Key);
        }

        private async Task CloseWindowAsync() => await _windowService.CloseWindowAsync(this);
        #endregion
    }
}

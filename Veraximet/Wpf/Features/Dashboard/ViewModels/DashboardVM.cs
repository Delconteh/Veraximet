using Beautify.Contracts.Helper;
using Beautify.Wpf.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Veraximet.Contracts.Services;
using Veraximet.Wpf.Navigation;

namespace Veraximet.Wpf.Features.Dashboard.ViewModels
{
    public sealed class DashboardVM : ObservableObject, IAsyncInitializable
    {
        #region Variables
        private readonly IDashboardService _dashboard;
        private readonly IInstrumentPageActions _pageActions;
        private bool _isLoading;
        private bool _hasError;
        private string? _errorMessage;
        private string _lastUpdatedLabel = "Not loaded";
        private int _totalInstruments;
        private int _availableInstruments;
        private int _overdueInstruments;
        private int _dueSoonInstruments;
        private int _nonconformingInstruments;
        private int _suspendedInstruments;
        private DashboardDeadlineRow? _selectedDeadline;
        #endregion

        #region Properties
        public ObservableCollection<DashboardDeadlineRow> UpcomingDeadlines { get; } = new();
        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }
        public bool HasError
        {
            get => _hasError; private set => SetProperty(ref _hasError, value);
        }
        public string? ErrorMessage
        {
            get => _errorMessage;
            private set => SetProperty(ref _errorMessage, value);
        }
        public string LastUpdatedLabel
        {
            get => _lastUpdatedLabel;
            private set => SetProperty(ref _lastUpdatedLabel, value);
        }
        public int TotalInstruments
        {
            get => _totalInstruments;
            private set => SetProperty(ref _totalInstruments, value);
        }
        public int AvailableInstruments
        {
            get => _availableInstruments;
            private set => SetProperty(ref _availableInstruments, value);
        }

        public int OverdueInstruments
        {
            get => _overdueInstruments;
            private set => SetProperty(ref _overdueInstruments, value);
        }

        public int DueSoonInstruments
        {
            get => _dueSoonInstruments;
            private set => SetProperty(ref _dueSoonInstruments, value);
        }

        public int NonconformingInstruments
        {
            get => _nonconformingInstruments;
            private set => SetProperty(ref _nonconformingInstruments, value);
        }

        public int SuspendedInstruments
        {
            get => _suspendedInstruments;
            private set => SetProperty(ref _suspendedInstruments, value);
        }

        public DashboardDeadlineRow? SelectedDeadline
        {
            get => _selectedDeadline;
            set
            {
                if (SetProperty(ref _selectedDeadline, value))
                    OnPropertyChanged(nameof(CanOpenSelected));
            }
        }

        public bool CanOpenSelected => 
            SelectedDeadline is not null;
        #endregion

        #region Commands
        public ICommand RefreshCommand { get; }
        public ICommand OpenInstrumentsCommand { get; }
        public ICommand OpenSelectedInstrumentCommand { get; }
        #endregion

        #region Constructors
        public DashboardVM(IDashboardService dashboard, IInstrumentPageActions pageActions)
        {
            _dashboard = dashboard;
            _pageActions = pageActions;
            RefreshCommand = new AsyncRelayCommand(InitializeAsync);
            OpenInstrumentsCommand = new AsyncRelayCommand(OpenInstrumentsAsync);
            OpenSelectedInstrumentCommand = new AsyncRelayCommand(OpenSelectedInstrumentAsync);
        }
        #endregion

        #region Methods
        private Task OpenInstrumentsAsync(CancellationToken cancellationToken) =>
            NavigateAsync(() => _pageActions.OpenRegisterAsync(cancellationToken), cancellationToken);

        private Task OpenSelectedInstrumentAsync(CancellationToken cancellationToken) => 
            SelectedDeadline is { } selected ? NavigateAsync(() => _pageActions.OpenDetailsAsync(selected.InstrumentId, cancellationToken), cancellationToken) : Task.CompletedTask;

        private async Task NavigateAsync(Func<Task> navigate, CancellationToken cancellationToken)
        {
            try
            {
                await navigate();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = ex.Message;
            }
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (IsLoading) 
                return;

            IsLoading = true;
            HasError = false;
            ErrorMessage = null;

            try
            {
                var result = await _dashboard.GetSnapshotAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                TotalInstruments = result.TotalInstruments;
                AvailableInstruments = result.AvailableInstruments;
                OverdueInstruments = result.OverdueInstruments;
                DueSoonInstruments = result.DueSoonInstruments;
                NonconformingInstruments = result.NonconformingInstruments;
                SuspendedInstruments = result.SuspendedInstruments;
                SelectedDeadline = null;
                UpcomingDeadlines.Clear();
                foreach (var deadline in result.UpcomingDeadlines)
                    UpcomingDeadlines.Add(DashboardDeadlineRow.FromDto(deadline));


                LastUpdatedLabel = DateTime.Now.ToString("dd MMM yyyy, HH:mm");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                HasError = true;
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion
    }

}

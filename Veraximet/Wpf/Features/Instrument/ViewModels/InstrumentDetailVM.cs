using Beautify.Contracts.Helper;
using Beautify.Wpf.Mvvm;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Veraximet.Contracts.Dto;
using Veraximet.Contracts.Dto.Verification;
using Veraximet.Contracts.Services;
using Veraximet.Domain.Enums;
using Veraximet.Wpf.Navigation;

namespace Veraximet.Wpf.Features.Instrument.ViewModels
{
    public sealed class InstrumentDetailVM : ObservableObject, IAsyncInitializable
    {
        #region Variables
        private readonly IInstrumentService _instrumentService;
        private readonly IInstrumentPageActions _pageActions;
        private readonly IVerificationService _verificationService;
        private readonly SemaphoreSlim _loadGate = new(1, 1);
        private InstrumentDto? _instrument;
        private InstrumentRow? _status;
        private bool _isLoading;
        private bool _hasError;
        private string? _errorMessage;
        #endregion

        #region Properties
        public Guid InstrumentId { get; }
        public ObservableCollection<VerificationDto> VerificationHistory { get; } = new();

        public bool CanEdit => Instrument is not null && !IsLoading;


        public bool CanStartVerification =>
            Instrument is
            {
                OperationalStatus: not OperationalStatus.Archived
            } && !IsLoading;



        public InstrumentDto? Instrument
        {
            get => _instrument;
            private set
            {
                if (SetProperty(ref _instrument, value))
                {
                    OnPropertyChanged(nameof(Heading));
                    OnPropertyChanged(nameof(CanStartVerification));
                    OnPropertyChanged(nameof(CodeLabel));
                    OnPropertyChanged(nameof(VerificationIntervalLabel));
                    OnPropertyChanged(nameof(LastVerificationLabel));
                    OnPropertyChanged(nameof(NextDueLabel));
                    OnPropertyChanged(nameof(CanEdit));
                }
            }
        }

        private InstrumentRow? Status
        {
            set
            {
                _status = value;
                OnPropertyChanged(nameof(OperationalStatusCode));
                OnPropertyChanged(nameof(OperationalStatusLabel));
                OnPropertyChanged(nameof(ConformityStatusCode));
                OnPropertyChanged(nameof(ConformityStatusLabel));
                OnPropertyChanged(nameof(DueStatusCode));
                OnPropertyChanged(nameof(DueStatusLabel));
            }
        }

        public string Heading =>
            Instrument?.Name ?? "Instrument details";



        public string CodeLabel => Instrument?.Code ?? "—";

        public string OperationalStatusCode => _status?.OperationalStatusCode ?? string.Empty;

        public string OperationalStatusLabel =>
            _status?.OperationalStatusLabel ?? "—";

        public string ConformityStatusCode => _status?.ConformityStatusCode ?? string.Empty;

        public string ConformityStatusLabel => _status?.ConformityStatusLabel ?? "—";

        public string DueStatusCode => _status?.DueStatusCode ?? string.Empty;

        public string DueStatusLabel =>
            _status?.DueStatusLabel ?? "—";

        public string VerificationIntervalLabel =>
            Instrument is null ? "—" : $"Every {Instrument.VerificationIntervalMonths} month(s)";

        public string LastVerificationLabel => FormatDate(Instrument?.LastVerificationDate);
        public string NextDueLabel => FormatDate(Instrument?.NextDueDate);

        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    OnPropertyChanged(nameof(CanEdit));
                    OnPropertyChanged(nameof(CanStartVerification));
                }
            }
        }

        public bool HasError
        {
            get => _hasError;
            private set => SetProperty(ref _hasError, value);
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            private set => SetProperty(ref _errorMessage, value);
        }
        #endregion

        #region Commands
        public ICommand RefreshCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand StartVerificationCommand { get; }
        #endregion

        #region Constructors
        public InstrumentDetailVM(Guid instrumentId, IInstrumentService instrumentService, IVerificationService verificationService, IInstrumentPageActions pageActions)
        {
            if (instrumentId == Guid.Empty)
                throw new ArgumentException("An instrument ID is required.", nameof(instrumentId));

            InstrumentId = instrumentId;
            _instrumentService = instrumentService;
            _verificationService = verificationService;
            _pageActions = pageActions;

            RefreshCommand = new AsyncRelayCommand(InitializeAsync);
            EditCommand = new AsyncRelayCommand(EditAsync);
            BackCommand = new AsyncRelayCommand(BackAsync);
            StartVerificationCommand = new AsyncRelayCommand(StartVerificationAsync);
        }
        #endregion

        #region Methods
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            await _loadGate.WaitAsync(cancellationToken);
            try
            {
                IsLoading = true;
                HasError = false;
                ErrorMessage = null;

                var item = await _instrumentService.GetByIdAsync(InstrumentId, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                Instrument = item;
                Status = item is null ? null : InstrumentRow.FromDto(
                    item, DateOnly.FromDateTime(DateTime.Today));

                VerificationHistory.Clear();

                if (item is not null)
                {
                    var records = await _verificationService.GetHistoryAsync(InstrumentId, cancellationToken);
                    foreach (var record in records)
                        VerificationHistory.Add(record);
                }

                if (item is null)
                {
                    HasError = true;
                    ErrorMessage = "This instrument no longer exists. Return to the register and refresh.";
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Unable to load instrument details: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                _loadGate.Release();
            }
        }

        private Task BackAsync(CancellationToken cancellationToken) =>
            NavigateAsync(() => _pageActions.OpenRegisterAsync(cancellationToken), cancellationToken);

        private Task EditAsync(CancellationToken cancellationToken) =>
            CanEdit ? NavigateAsync(() => _pageActions.OpenEditAsync(InstrumentId, true, cancellationToken), cancellationToken) : Task.CompletedTask;

        private Task StartVerificationAsync(CancellationToken cancellationToken) =>
            CanStartVerification && Instrument is { } instrument ? NavigateAsync(() => _pageActions.OpenVerificationAsync(instrument, cancellationToken), cancellationToken) : Task.CompletedTask;

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

        private static string FormatDate(DateOnly? date) => 
            date?.ToString("dd MMM yyyy", CultureInfo.CurrentCulture) ?? "Not recorded";
        #endregion
    }
}

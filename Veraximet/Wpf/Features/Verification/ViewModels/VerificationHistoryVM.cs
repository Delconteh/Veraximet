using Beautify.Contracts.Helper;
using Beautify.Wpf.Mvvm;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Veraximet.Contracts.Dto.Verification;
using Veraximet.Contracts.Services;
using Veraximet.Metrology;

namespace Veraximet.Wpf.Features.Verification.ViewModels
{
    public sealed class VerificationHistoryVM : ObservableObject, IAsyncInitializable
    {
        #region Variables
        private readonly IVerificationService _service;
        private readonly SemaphoreSlim _loadGate = new(1, 1);
        private VerificationDto? _selected;
        private string? _errorMessage;
        private bool _isLoading;
        #endregion

        #region Properties
        public ObservableCollection<VerificationDto> Records { get; } = new();
        public ObservableCollection<HistoryPointRow> SelectedPoints { get; } = new();



        public string? ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }


        public bool IsLoading { get => _isLoading; private set => SetProperty(ref _isLoading, value); }

        public VerificationDto? Selected
        {
            get => _selected;
            set
            {
                if (!SetProperty(ref _selected, value))
                    return;

                SelectedPoints.Clear();
                OnPropertyChanged(nameof(RuleNotice));


                if (value?.DecisionRule != VerificationCalculator.DecisionRule)
                    return;

                foreach (var point in value.Points)
                    SelectedPoints.Add(new HistoryPointRow(point));
            }
        }

        public string? RuleNotice => Selected is not null && Selected.DecisionRule != VerificationCalculator.DecisionRule  ? "This historical rule is not supported by the current calculator." : null;
        #endregion

        #region Commands
        public ICommand RefreshCommand { get; }
        #endregion

        #region Constructors
        public VerificationHistoryVM(IVerificationService service)
        {
            _service = service;
            RefreshCommand = new AsyncRelayCommand(InitializeAsync);
        }
        #endregion

        #region Methods
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            await _loadGate.WaitAsync(cancellationToken);
            IsLoading = true;
            ErrorMessage = null;
            try
            {
                var records = await _service.GetHistoryAsync(cancellationToken: cancellationToken);
                var selectedId = Selected?.Id;

                Selected = null;


                Records.Clear();

                foreach (var record in records)
                    Records.Add(record);


                Selected = Records.FirstOrDefault(x => x.Id == selectedId) ?? Records.FirstOrDefault();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unable to load history: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                _loadGate.Release();
            }
        }
        #endregion
    }

    public sealed class HistoryPointRow
    {
        public VerificationPointDto Point { get; }
        public VerificationPointResult Result { get; }
        public MeasurementPlotData Plot { get; }
        public HistoryPointRow(VerificationPointDto point)
        {
            Point = point;
            Result = VerificationCalculator.Calculate(point.ReferenceValue, point.MaximumPermissibleError, point.Measurements);
            Plot = new MeasurementPlotData(point.ReferenceValue, point.MaximumPermissibleError, point.Measurements, Result);
        }
        public string MeanLabel => 
            Result.Mean.ToString("G29", CultureInfo.CurrentCulture);
        public string MeanErrorLabel => 
            Result.MeanError.ToString("G29", CultureInfo.CurrentCulture);

        public string MaxErrorLabel => 
            Result.MaximumAbsoluteError.ToString("G29", CultureInfo.CurrentCulture);
        public string RangeLabel => 
            Result.Range.ToString("G29", CultureInfo.CurrentCulture);


        public string DeviationLabel => 
            Result.StandardDeviation?.ToString("G6", CultureInfo.CurrentCulture) ?? "—";
        public string Outcome => 
            Result.IsWithinTolerance ? "Within limits" : "Outside limits";
    }
}

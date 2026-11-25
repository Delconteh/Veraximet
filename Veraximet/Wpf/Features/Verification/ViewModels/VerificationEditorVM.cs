using Beautify.Wpf.Mvvm;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Veraximet.Contracts.Dto;
using Veraximet.Contracts.Dto.Verification;
using Veraximet.Contracts.Services;
using Veraximet.Wpf.Navigation;

namespace Veraximet.Wpf.Features.Verification.ViewModels
{
    public sealed class VerificationEditorVM : ObservableObject
    {
        #region Variables
        private readonly IVerificationService _service;
        private readonly IInstrumentPageActions _pageActions;
        private bool _saved;
        private string _performedBy = string.Empty;
        private string _unit = string.Empty;
        private string _notes = string.Empty;
        private string _dateText = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        private bool _isSaving;
        private string? _errorMessage;
        #endregion

        #region Properties
        public InstrumentDto Instrument { get; }

        public ObservableCollection<VerificationPointVM> Points { get; } = new();

        public string PerformedBy
        {
            get => _performedBy;
            set => SetProperty(ref _performedBy, value ?? string.Empty);
        }

        public string Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value ?? string.Empty);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value ?? string.Empty);
        }

        public string DateText
        {
            get => _dateText;
            set => SetProperty(ref _dateText, value ?? string.Empty);
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            private set => SetProperty(ref _errorMessage, value);
        }

        public bool IsSaving
        {
            get => _isSaving;
            private set
            {
                if (SetProperty(ref _isSaving, value))
                {
                    OnPropertyChanged(nameof(IsEditable));
                    OnPropertyChanged(nameof(CanCancel));
                }
            }
        }

        public bool IsEditable =>
            !IsSaving && !_saved;

        public bool CanCancel => !IsSaving;

        public bool CanRemovePoint => Points.Count > 1;

        public int CompletedCount =>
            Points.Count(p => p.Result is not null);

        public int PassedCount => Points.Count(p => p.Result?.IsWithinTolerance == true);
        public string Summary => $"{CompletedCount} / {Points.Count} evaluated · {PassedCount} within limits";
        public string Outcome => CompletedCount != Points.Count ? "Incomplete" : PassedCount == Points.Count ? "Within limits" : "Outside limits";
        public string OutcomeCode => CompletedCount != Points.Count ? "NotEvaluated" : PassedCount == Points.Count ? "Conforming" : "Nonconforming";
        #endregion

        #region Commands
        public ICommand AddPointCommand { get; }
        public ICommand RemoveLastPointCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        #endregion

        #region Constructors
        public VerificationEditorVM(InstrumentDto instrument, IVerificationService service, IInstrumentPageActions pageActions)
        {
            Instrument = instrument;
            _service = service;
            _pageActions = pageActions;
            AddPointCommand = new RelayCommand(AddPoint);
            RemoveLastPointCommand = new RelayCommand(() =>
            {
                if (Points.Count <= 1)
                    return;


                Points.RemoveAt(Points.Count - 1);
                UpdateSummary();
            });

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CancelCommand = new AsyncRelayCommand(CancelAsync);

            AddPoint();
        }
        #endregion

        #region Methods
        private void AddPoint()
        {
            var point = new VerificationPointVM(Points.Count + 1, UpdateSummary);
            Points.Add(point);
            UpdateSummary();
        }

        private void UpdateSummary()
        {
            OnPropertyChanged(nameof(CanRemovePoint));
            OnPropertyChanged(nameof(CompletedCount));
            OnPropertyChanged(nameof(PassedCount));
            OnPropertyChanged(nameof(Summary));
            OnPropertyChanged(nameof(Outcome));
            OnPropertyChanged(nameof(OutcomeCode));
        }

        private async Task SaveAsync(CancellationToken cancellationToken)
        {
            if (!IsEditable)
                return;

            ErrorMessage = null;

            if (!DateOnly.TryParseExact(DateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) || date == default || date > DateOnly.FromDateTime(DateTime.Today) || string.IsNullOrWhiteSpace(PerformedBy) || string.IsNullOrWhiteSpace(Unit))
            {
                ErrorMessage = "Enter a valid date (yyyy-MM-dd), operator and unit.";
                return;
            }

            var points = new List<VerificationPointDto>();

            foreach (var point in Points)
            {
                if (!point.TryBuild(out var dto) || point.Result is null)
                {
                    ErrorMessage = "Complete every point with a reference, non-negative MPE and numeric readings.";
                    return;
                }
                points.Add(dto!);
            }

            if (points.Count == 0)
            {
                ErrorMessage = "Add at least one point.";
                return;
            }

            IsSaving = true;
            try
            {
                await _service.CreateAsync(new CreateVerificationRequest(Instrument.Id, date, PerformedBy, Unit, Notes, points), cancellationToken);
                _saved = true;

                await _pageActions.OpenDetailsAsync(Instrument.Id);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                ErrorMessage = _saved ? $"Verification saved, but navigation failed: {ex.Message}" : $"Unable to save verification: {ex.Message}";
            }
            finally
            {
                IsSaving = false;
            }
        }

        private async Task CancelAsync(CancellationToken cancellationToken)
        {
            if (IsSaving)
                return;

            try
            {
                await _pageActions.OpenDetailsAsync(Instrument.Id, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
        #endregion
    }
}

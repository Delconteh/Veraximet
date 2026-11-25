using Beautify.Wpf.Mvvm;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.Versioning;
using System.Windows.Input;
using Veraximet.Contracts.Dto.Verification;
using Veraximet.Metrology;

namespace Veraximet.Wpf.Features.Verification.ViewModels
{

    [SupportedOSPlatform("windows")]
    public sealed class VerificationPointVM : ObservableObject
    {
        #region Variables
        private readonly Action _changed;
        private string _reference = string.Empty;
        private string _description = string.Empty;
        private string _mpe = string.Empty;
        private MeasurementPlotData? _plot;
        private VerificationPointResult? _result;
        private string? _validationMessage;
        #endregion

        #region Properties
        public ObservableCollection<MeasurementVM> Measurements { get; } = new();

        public int Order { get; }

        public bool CanRemoveMeasurement => Measurements.Count > 1;

        public string ReferenceText
        {
            get => _reference;
            set
            {
                if (SetProperty(ref _reference, value ?? string.Empty))
                    Recalculate();
            }
        }

        public string MpeText
        {
            get => _mpe;
            set
            {
                if (SetProperty(ref _mpe, value ?? string.Empty))
                    Recalculate();
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (SetProperty(ref _description, value ?? string.Empty))
                    _changed();
            }
        }

        public VerificationPointResult? Result { get => _result;
            private set => SetProperty(ref _result, value); }

        public MeasurementPlotData? Plot { get => _plot; 
            private set => SetProperty(ref _plot, value); }

        public string? ValidationMessage { get => _validationMessage; 
            private set => SetProperty(ref _validationMessage, value); }

        public string Outcome =>
            Result is null ? "Not evaluated" : Result.IsWithinTolerance ? "Within limits" : "Outside limits";

        public string OutcomeCode => 
            Result is null ? "NotEvaluated" : Result.IsWithinTolerance ? "Conforming" : "Nonconforming";

        public string MeanLabel => Format(Result?.Mean);

        public string MeanErrorLabel => Format(Result?.MeanError);

        public string MaxErrorLabel => Format(Result?.MaximumAbsoluteError);

        public string RangeLabel => Format(Result?.Range);

        public string DeviationLabel => Result?.StandardDeviation?.ToString("G6", CultureInfo.CurrentCulture) ?? "—";
        #endregion

        #region Commands
        public ICommand AddMeasurementCommand { get; }
        public ICommand RemoveLastMeasurementCommand { get; }
        #endregion

        #region Constructors
        public VerificationPointVM(int order, Action changed)
        {
            Order = order;
            _changed = changed;

            AddMeasurementCommand = new RelayCommand(() =>
            {
                Measurements.Add(new MeasurementVM(Measurements.Count + 1, Recalculate));
                Recalculate();
            });

            RemoveLastMeasurementCommand = new RelayCommand(() =>
            {
                if (Measurements.Count <= 1)
                    return;
                Measurements.RemoveAt(Measurements.Count - 1);
                Recalculate();
            });
            for (int i = 0; i < 3; i++)
                Measurements.Add(new MeasurementVM(Measurements.Count + 1, Recalculate));
        }
        #endregion

        #region Methods
        private static string Format(decimal? value) => value?.ToString("G29", CultureInfo.CurrentCulture) ?? "—";

        private static bool TryParse(string text, out decimal value) => 
            decimal.TryParse( text.Trim().Replace(',', '.'), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value);

        public bool TryBuild(out VerificationPointDto? point)
        {
            point = null;
            if (!TryParse(ReferenceText, out var reference) || !TryParse(MpeText, out var mpe) || mpe < 0)
                return false;

            var readings = new decimal[Measurements.Count];

            if (readings.Length == 0)
                return false;

            for (int i = 0; i < readings.Length; i++)
            {
                if (!TryParse(Measurements[i].Value, out readings[i]))
                    return false;
            }
            point = new VerificationPointDto(reference, mpe, Description, readings);


            return true;
        }

        public void Recalculate()
        {
            Result = null;
            Plot = null;
            ValidationMessage = null;


            if (TryBuild(out var point))
            {
                try
                {
                    Result = VerificationCalculator.Calculate(point!.ReferenceValue, point.MaximumPermissibleError, point.Measurements);
                    Plot = new MeasurementPlotData(point.ReferenceValue, point.MaximumPermissibleError, point.Measurements, Result);
                }
                catch (OverflowException)
                {
                    ValidationMessage = "Values exceed the supported numeric range.";
                }
            }




            OnPropertyChanged(nameof(Outcome));
            OnPropertyChanged(nameof(OutcomeCode));
            OnPropertyChanged(nameof(MeanLabel));
            OnPropertyChanged(nameof(MeanErrorLabel));
            OnPropertyChanged(nameof(MaxErrorLabel));
            OnPropertyChanged(nameof(RangeLabel));
            OnPropertyChanged(nameof(DeviationLabel));
            OnPropertyChanged(nameof(CanRemoveMeasurement));
            _changed();
        }
        #endregion
    }
}

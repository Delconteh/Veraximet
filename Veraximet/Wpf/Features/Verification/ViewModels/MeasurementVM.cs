using Beautify.Wpf.Mvvm;

namespace Veraximet.Wpf.Features.Verification.ViewModels
{
    public sealed class MeasurementVM : ObservableObject
    {
        #region Variables
        private string _value = string.Empty;
        private readonly Action _changed;
        #endregion

        #region Properties
        public int Order { get; }

        public string Value
        {
            get => _value;
            set
            {
                if (SetProperty(ref _value, value ?? string.Empty))
                    _changed();
            }
        }
        #endregion

        #region Constructors
        public MeasurementVM(int order, Action changed)
        {
            Order = order;
            _changed = changed;
        }
        #endregion
    }
}

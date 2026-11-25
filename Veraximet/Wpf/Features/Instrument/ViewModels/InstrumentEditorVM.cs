using Beautify.Wpf.Mvvm;
using Veraximet.Wpf.Navigation;
using System.Globalization;
using System.Windows.Input;
using Veraximet.Contracts.Dto;
using Veraximet.Contracts.Exceptions;
using Veraximet.Contracts.Services;

namespace Veraximet.Wpf.Features.Instrument.ViewModels
{

    public sealed class InstrumentEditorVM : ObservableObject
    {
        #region Variables
        private readonly IInstrumentService _instrumentService;
        private readonly Guid? _instrumentId;
        private readonly IInstrumentPageActions _pageActions;
        private readonly bool _returnToDetail;
        private bool _saved;
        private string _code = string.Empty;
        private string _verificationIntervalText = "12";
        private string _name = string.Empty;
        private string _category = string.Empty;
        private string _description = string.Empty;
        private string _manufacturer = string.Empty;
        private string _model = string.Empty;
        private string _department = string.Empty;
        private string _location = string.Empty;
        private string _serialNumber = string.Empty;
        private bool _isSaving;
        private string? _errorMessage;
        private string _notes = string.Empty;
        #endregion

        #region Properties
        public bool CanSave => !IsSaving && !_saved;
        public bool CanCancel =>
            !IsSaving;
        public bool IsEditMode => _instrumentId.HasValue;

        public string Heading => IsEditMode ? "Edit instrument" : "New instrument";

        public string Subheading => IsEditMode ? "Update instrument metadata. The identifier and verification results remain unchanged."
            : "Add a measuring instrument to the register.";

        public string SaveLabel =>
            IsEditMode ? "Save changes" : "Create instrument";

        public string Code
        {
            get => _code;
            set => SetProperty(ref _code, value ?? string.Empty);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value ?? string.Empty);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value ?? string.Empty);
        }

        public string VerificationIntervalText
        {
            get => _verificationIntervalText;
            set => SetProperty(ref _verificationIntervalText, value ?? string.Empty);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value ?? string.Empty);
        }

        public string Manufacturer
        {
            get => _manufacturer;
            set => SetProperty(ref _manufacturer, value ?? string.Empty);
        }

        public string Model
        {
            get => _model;
            set => SetProperty(ref _model, value ?? string.Empty);
        }

        public string SerialNumber
        {
            get => _serialNumber;
            set => SetProperty(ref _serialNumber, value ?? string.Empty);
        }

        public string Department
        {
            get => _department;
            set => SetProperty(ref _department, value ?? string.Empty);
        }

        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value ?? string.Empty);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value ?? string.Empty);
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
                    OnPropertyChanged(nameof(CanSave));
                    OnPropertyChanged(nameof(CanCancel));
                }
            }
        }
        #endregion

        #region Commands
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        #endregion

        #region Constructors
        public InstrumentEditorVM(IInstrumentService instrumentService, IInstrumentPageActions pageActions)
        {
            _instrumentService = instrumentService;
            _pageActions = pageActions;
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CancelCommand = new AsyncRelayCommand(CancelAsync);
        }

        public InstrumentEditorVM(IInstrumentService instrumentService, IInstrumentPageActions pageActions, InstrumentDto instrument, bool returnToDetail) : this(instrumentService, pageActions)
        {
            _instrumentId = instrument.Id;
            _returnToDetail = returnToDetail;
            Code = instrument.Code;
            Name = instrument.Name;
            Category = instrument.Category;
            VerificationIntervalText = instrument.VerificationIntervalMonths.ToString(CultureInfo.InvariantCulture);
            Description = instrument.Description ?? string.Empty;
            Manufacturer = instrument.Manufacturer ?? string.Empty;
            Model = instrument.Model ?? string.Empty;
            SerialNumber = instrument.SerialNumber ?? string.Empty;
            Department = instrument.Department ?? string.Empty;
            Location = instrument.Location ?? string.Empty;
            Notes = instrument.Notes ?? string.Empty;
        }
        #endregion

        #region Methods
        private async Task SaveAsync(CancellationToken cancellationToken)
        {
            if (!CanSave)
                return;

            ErrorMessage = null;
            if (!TryBuildDetails(out var details))
                return;

            IsSaving = true;
            try
            {
                if (_instrumentId is { } existingId)
                {
                    if (!await _instrumentService.UpdateAsync(existingId, details!, cancellationToken))
                    {
                        ErrorMessage = "This instrument no longer exists. Reload the register and try again.";
                        return;
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(Code))
                    {
                        ErrorMessage = "Instrument code is required.";
                        return;
                    }
                    await _instrumentService.CreateAsync(new CreateInstrumentRequest(Code, details!), cancellationToken);
                }

                _saved = true;
                OnPropertyChanged(nameof(CanSave));

                await ReturnAsync(CancellationToken.None);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {

            }
            catch (DuplicateInstrumentCodeException)
            {
                ErrorMessage = "This instrument code already exists. Choose a different code.";
            }
            catch (ArgumentException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                ErrorMessage = _saved ? $"Instrument saved, but navigation failed: {ex.Message}" : $"Could not save the instrument: {ex.Message}";
            }
            finally
            {
                IsSaving = false;
            }
        }

        private bool TryBuildDetails(out InstrumentDetailsDto? details)
        {
            details = null;


            if (string.IsNullOrWhiteSpace(Name))
            {
                ErrorMessage = "Instrument name is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Category))
            {
                ErrorMessage = "Category is required.";
                return false;
            }

            if (!int.TryParse(VerificationIntervalText, NumberStyles.None, CultureInfo.InvariantCulture, out var months) || months is < 1 or > 1200)
            {
                ErrorMessage = "Verification interval must be a whole number between 1 and 1200 months.";
                return false;
            }

            details = new InstrumentDetailsDto(Name: Name, Category: Category, VerificationIntervalMonths: months, Description: Description, Manufacturer: Manufacturer, Model: Model, SerialNumber: SerialNumber, Department: Department, Location: Location, Notes: Notes);



            return true;
        }

        private async Task CancelAsync(CancellationToken cancellationToken)
        {
            if (!CanCancel)
                return;

            try
            {
                await ReturnAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private Task ReturnAsync(CancellationToken cancellationToken) =>
            _returnToDetail && _instrumentId is { } id ? _pageActions.OpenDetailsAsync(id, cancellationToken) : _pageActions.OpenRegisterAsync(cancellationToken);
        #endregion
    }

}

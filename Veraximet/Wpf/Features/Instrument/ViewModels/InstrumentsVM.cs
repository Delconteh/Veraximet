using Beautify.Contracts.Helper;
using Beautify.Wpf.Mvvm;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using Veraximet.Contracts.Dto;
using Veraximet.Contracts.Services;
using Veraximet.Wpf.Navigation;

namespace Veraximet.Wpf.Features.Instrument.ViewModels
{
    public sealed class InstrumentsVM : ObservableObject, IAsyncInitializable
    {
        #region Variables
        private const string AllStatuses = "All statuses";
        private const string AllCategories = "All categories";
        private const string AllDepartments = "All departments";
        private readonly IInstrumentService _instrumentService;
        private readonly IInstrumentPageActions _pageActions;
        private readonly ObservableCollection<InstrumentRow> _source = new();
        private readonly SemaphoreSlim _loadGate = new(1, 1);
        private InstrumentRow? _selectedInstrument;
        private string? _infoMessage;
        private string _searchText = string.Empty;
        private string _selectedStatusFilter = AllStatuses;
        private string _selectedCategoryFilter = AllCategories;
        private string _selectedDepartmentFilter = AllDepartments;
        private bool _isLoading;
        private bool _hasError;
        private string? _errorMessage;
        private int _totalCount;
        private int _filteredCount;
        #endregion

        #region Properties
        public ICollectionView Instruments { get; }
        public ObservableCollection<string> CategoryFilters { get; } = new();
        public ObservableCollection<string> DepartmentFilters { get; } = new();

        public IReadOnlyList<string> StatusFilters { get; }

        public string? InfoMessage
        {
            get => _infoMessage;
            private set => SetProperty(ref _infoMessage, value);
        }

        public InstrumentRow? SelectedInstrument
        {
            get => _selectedInstrument;
            set
            {
                if (SetProperty(ref _selectedInstrument, value))
                {
                    OnPropertyChanged(nameof(CanOpenSelected));
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value ?? string.Empty))
                    ApplyFilters();
            }
        }

        public string SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                if (SetProperty(ref _selectedStatusFilter, value ?? AllStatuses))
                    ApplyFilters();
            }
        }

        public string SelectedCategoryFilter
        {
            get => _selectedCategoryFilter;
            set
            {
                if (SetProperty(ref _selectedCategoryFilter, value ?? AllCategories))
                    ApplyFilters();
            }
        }

        public string SelectedDepartmentFilter
        {
            get => _selectedDepartmentFilter;
            set
            {
                if (SetProperty(ref _selectedDepartmentFilter, value ?? AllDepartments))
                    ApplyFilters();
            }
        }

        public bool CanCreate => !IsLoading;

        public bool CanOpenSelected =>
            SelectedInstrument is not null && !IsLoading;

        public int TotalCount
        {
            get => _totalCount;
            private set => SetProperty(ref _totalCount, value);
        }

        public int FilteredCount
        {
            get => _filteredCount;
            private set => SetProperty(ref _filteredCount, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    OnPropertyChanged(nameof(CanCreate));
                    OnPropertyChanged(nameof(CanOpenSelected));
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
        public ICommand ClearFiltersCommand { get; }
        public ICommand CreateInstrumentCommand { get; }
        public ICommand EditSelectedInstrumentCommand { get; }
        public ICommand OpenSelectedInstrumentCommand { get; }
        #endregion

        #region Constructors
        public InstrumentsVM(IInstrumentService instrumentService, IInstrumentPageActions pageActions)
        {
            _instrumentService = instrumentService;
            _pageActions = pageActions;
            Instruments = CollectionViewSource.GetDefaultView(_source);
            Instruments.Filter = Match;

            StatusFilters = new ReadOnlyCollection<string>(new[]
            {
                AllStatuses, "In service", "Suspended", "Archived",
                "Conforming", "Nonconforming", "Not evaluated",
                "Overdue", "Due soon", "Valid", "Not scheduled"
            });

            CategoryFilters.Add(AllCategories);
            DepartmentFilters.Add(AllDepartments);

            RefreshCommand = new AsyncRelayCommand(InitializeAsync);
            ClearFiltersCommand = new RelayCommand(ClearFilters);
            CreateInstrumentCommand = new AsyncRelayCommand(OpenCreateAsync);
            EditSelectedInstrumentCommand = new AsyncRelayCommand(EditSelectedAsync);
            OpenSelectedInstrumentCommand = new AsyncRelayCommand(OpenSelectedDetailsAsync);
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

                var results = new List<InstrumentDto>();
                var pageNumber = 1;

                while (true)
                {
                    var page = await _instrumentService.SearchAsync( new InstrumentSearchCriteria(IncludeArchived: true, Page: pageNumber, PageSize: 200), cancellationToken);

                    results.AddRange(page.Items);

                    if (results.Count >= page.TotalCount || page.Items.Count == 0)
                        break;

                    pageNumber++;
                }

                cancellationToken.ThrowIfCancellationRequested();

                var today = DateOnly.FromDateTime(DateTime.Today);
                var previousSelectionId = SelectedInstrument?.Id;


                SelectedInstrument = null;

                _source.Clear();

                foreach (var instrument in results)
                    _source.Add(InstrumentRow.FromDto(instrument, today));

                TotalCount = results.Count;
                RebuildFilterOptions();
                ApplyFilters();

                if (previousSelectionId is { } selectedId)
                {
                    var restored = _source.FirstOrDefault(x => x.Id == selectedId);

                    if (restored is not null && Instruments.Cast<object>().Contains(restored))
                        SelectedInstrument = restored;
                }
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
                _loadGate.Release();
            }
        }

        private Task OpenCreateAsync(CancellationToken cancellationToken) =>
            RunNavigationAsync(() => _pageActions.OpenCreateAsync(cancellationToken), cancellationToken);

        private Task EditSelectedAsync(CancellationToken cancellationToken)
        {
            if (SelectedInstrument is not { } selected)
                return Task.CompletedTask;

            return RunNavigationAsync(() => _pageActions.OpenEditAsync(selected.Id, cancellationToken: cancellationToken), cancellationToken);
        }

        private Task OpenSelectedDetailsAsync(CancellationToken cancellationToken)
        {
            if (SelectedInstrument is not { } selected)
                return Task.CompletedTask;

            return RunNavigationAsync(() => _pageActions.OpenDetailsAsync(selected.Id, cancellationToken), cancellationToken);
        }

        private async Task RunNavigationAsync(Func<Task> navigate, CancellationToken cancellationToken)
        {
            if (IsLoading)
                return;

            HasError = false;
            ErrorMessage = null;
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

        private void RebuildFilterOptions()
        {
            var categories = _source.Select(r => r.Category)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
            var departments = _source.Select(r => r.Department)
                .Where(x => x != "—" && !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            CategoryFilters.Clear();
            CategoryFilters.Add(AllCategories);
            foreach (var category in categories) CategoryFilters.Add(category);

            DepartmentFilters.Clear();
            DepartmentFilters.Add(AllDepartments);

            foreach (var department in departments)
                DepartmentFilters.Add(department);

            if (!CategoryFilters.Contains(SelectedCategoryFilter)) 
                SelectedCategoryFilter = AllCategories;

            if (!DepartmentFilters.Contains(SelectedDepartmentFilter)) 
                SelectedDepartmentFilter = AllDepartments;
        }

        private bool Match(object item)
        {
            if (item is not InstrumentRow row) return false;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var term = SearchText.Trim();
                if (!row.InstrumentCode.Contains(term, StringComparison.OrdinalIgnoreCase) && !row.InstrumentName.Contains(term, StringComparison.OrdinalIgnoreCase) &&
                    !row.SerialNumber.Contains(term, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            if (SelectedCategoryFilter != AllCategories &&
                !string.Equals(row.Category, SelectedCategoryFilter, StringComparison.OrdinalIgnoreCase))
                return false;

            if (SelectedDepartmentFilter != AllDepartments &&
                !string.Equals(row.Department, SelectedDepartmentFilter, StringComparison.OrdinalIgnoreCase))
                return false;

            return SelectedStatusFilter switch
            {
                "In service" => row.OperationalStatusCode == "InService",
                "Suspended" => row.OperationalStatusCode == "Suspended",
                "Archived" => row.OperationalStatusCode == "Archived",
                "Conforming" => row.ConformityStatusCode == "Conforming",
                "Nonconforming" => row.ConformityStatusCode == "Nonconforming",
                "Not evaluated" => row.ConformityStatusCode == "NotEvaluated",
                "Overdue" => row.DueStatusCode == "Overdue",
                "Due soon" => row.DueStatusCode == "DueSoon",
                "Valid" => row.DueStatusCode == "Valid",
                "Not scheduled" => row.DueStatusCode == "NotScheduled",
                _ => true
            };
        }

        private void ApplyFilters()
        {
            Instruments.Refresh();
            FilteredCount = Instruments.Cast<object>().Count();

            if (SelectedInstrument is not null &&
                !Instruments.Cast<object>().Contains(SelectedInstrument))
                SelectedInstrument = null;
        }

        private void ClearFilters()
        {
            SearchText = string.Empty;
            SelectedStatusFilter = AllStatuses;
            SelectedCategoryFilter = AllCategories;
            SelectedDepartmentFilter = AllDepartments;
        }
        #endregion
    }
}

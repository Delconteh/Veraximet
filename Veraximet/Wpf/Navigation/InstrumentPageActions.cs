using Beautify.Contracts.Service;
using Veraximet.Contracts.Dto;
using Veraximet.Contracts.Services;
using Veraximet.Wpf.Features.Instrument.ViewModels;
using Veraximet.Wpf.Features.Verification.ViewModels;

namespace Veraximet.Wpf.Navigation
{
    public sealed class InstrumentPageActions : IInstrumentPageActions
    {
        #region Variables
        private readonly INavigationService _navigation;
        private readonly IInstrumentService _instruments;
        private readonly IVerificationService _verifications;
        #endregion

        #region Constructors
        public InstrumentPageActions(INavigationService navigation, IInstrumentService instruments, IVerificationService verifications)
        {
            _navigation = navigation;
            _instruments = instruments;
            _verifications = verifications;
        }
        #endregion

        #region Methods
        public async Task OpenRegisterAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _navigation.NavigateAsync<InstrumentsVM>();
        }

        public async Task OpenCreateAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _navigation.NavigateAsync<InstrumentEditorVM>(_instruments, this);
        }

        public async Task OpenEditAsync(Guid id, bool returnToDetail = false, CancellationToken cancellationToken = default)
        {
            var instrument = await _instruments.GetByIdAsync(id, cancellationToken) 
                ?? throw new InvalidOperationException("The selected instrument no longer exists.");

            cancellationToken.ThrowIfCancellationRequested();

            await _navigation.NavigateAsync<InstrumentEditorVM>(_instruments, this, instrument, returnToDetail);
        }

        public async Task OpenDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("An instrument ID is required.", nameof(id));

            cancellationToken.ThrowIfCancellationRequested();
            await _navigation.NavigateAsync<InstrumentDetailVM>(id, _instruments, _verifications, this);
        }

        public async Task OpenVerificationAsync(InstrumentDto instrument, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _navigation.NavigateAsync<VerificationEditorVM>(instrument, _verifications, this);
        }
        #endregion
    }
}

using Veraximet.Contracts.Dto;

namespace Veraximet.Wpf.Navigation
{
    public interface IInstrumentPageActions
    {
        Task OpenRegisterAsync(CancellationToken cancellationToken = default);
        Task OpenCreateAsync(CancellationToken cancellationToken = default);
        Task OpenEditAsync(Guid id, bool returnToDetail = false, CancellationToken cancellationToken = default);
        Task OpenDetailsAsync(Guid id, CancellationToken cancellationToken = default);
        Task OpenVerificationAsync(InstrumentDto instrument, CancellationToken cancellationToken = default);
    }
}

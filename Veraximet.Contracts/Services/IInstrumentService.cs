using Veraximet.Contracts.Dto;
using Veraximet.Domain.Enums;

namespace Veraximet.Contracts.Services
{
    public interface IInstrumentService
    {
        Task<Guid> CreateAsync(CreateInstrumentRequest request, CancellationToken cancellationToken = default);
        Task<InstrumentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedResult<InstrumentDto>> SearchAsync(InstrumentSearchCriteria criteria, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Guid id, InstrumentDetailsDto details, CancellationToken cancellationToken = default);
        Task<bool> ChangeOperationalStatusAsync(Guid id, OperationalStatus status, CancellationToken cancellationToken = default);
        Task<bool> ArchiveAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }


}

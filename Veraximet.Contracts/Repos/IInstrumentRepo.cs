using Veraximet.Contracts.Dto;
using Veraximet.Domain.Entitys;

namespace Veraximet.Contracts.Repos
{
    public interface IInstrumentRepo
    {
        Task<Instrument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedResult<Instrument>> SearchAsync(  InstrumentSearchCriteria criteria, CancellationToken cancellationToken = default);
        Task<Guid> CreateAsync(Instrument instrument, CancellationToken cancellationToken = default);
        Task<bool> UpdateDetailsAsync(Instrument instrument, CancellationToken cancellationToken = default);
        Task<bool> UpdateOperationalStatusAsync(Instrument instrument, CancellationToken cancellationToken = default);

        /// <summary>Physical deletion. 
        /// Probably not something you want to use in a controlled, traced system.
        /// It depends on the implementation
        /// </summary>
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }

}

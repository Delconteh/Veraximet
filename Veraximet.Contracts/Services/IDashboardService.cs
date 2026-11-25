using Veraximet.Contracts.Dto;

namespace Veraximet.Contracts.Services
{
    public interface IDashboardService
    {
        Task<DashboardSnapshotDto> GetSnapshotAsync(CancellationToken cancellationToken = default);
    }
}

using Veraximet.Contracts.Dto.Verification;

namespace Veraximet.Contracts.Services
{
    public interface IVerificationService
    {
        Task<Guid> CreateAsync(CreateVerificationRequest request, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<VerificationDto>> GetHistoryAsync(Guid? instrumentId = null, CancellationToken cancellationToken = default);
    }
}

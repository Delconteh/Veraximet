using System;

namespace Veraximet.Contracts.Dto.Verification
{
    public sealed record CreateVerificationRequest(Guid InstrumentId, DateOnly Date, string PerformedBy, string Unit, string? Notes, IReadOnlyList<VerificationPointDto> Points);

}

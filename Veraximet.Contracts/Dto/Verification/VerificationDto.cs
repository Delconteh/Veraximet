using Veraximet.Domain.Enums;

namespace Veraximet.Contracts.Dto.Verification
{
    public sealed record VerificationDto(Guid Id, Guid InstrumentId, string InstrumentCode,
        string InstrumentName, DateOnly Date, string PerformedBy, string Unit, string? Notes,
        string DecisionRule, ConformityStatus Result, IReadOnlyList<VerificationPointDto> Points);
}

using Veraximet.Domain.Enums;

namespace Veraximet.Contracts.Dto
{
    public sealed record UpcomingDeadlineDto(
        Guid InstrumentId,
        string InstrumentCode,
        string InstrumentName,
        DateOnly DueDate,
        DueStatus DueStatus);
}

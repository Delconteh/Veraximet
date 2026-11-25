using Veraximet.Domain.Enums;

namespace Veraximet.Contracts.Dto
{
    public sealed record InstrumentDto(
        Guid Id,
        string Code,
        string Name,
        string Category,
        int VerificationIntervalMonths,
        string? Description,
        string? Manufacturer,
        string? Model,
        string? SerialNumber,
        string? Department,
        string? Location,
        string? Notes,
        DateOnly? LastVerificationDate,
        DateOnly? NextDueDate,
        OperationalStatus OperationalStatus,
        ConformityStatus ConformityStatus);

}

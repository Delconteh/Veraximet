using System;

namespace Veraximet.Contracts.Dto
{
    public sealed record InstrumentDetailsDto(
        string Name,
        string Category,
        int VerificationIntervalMonths,
        string? Description = null,
        string? Manufacturer = null,
        string? Model = null,
        string? SerialNumber = null,
        string? Department = null,
        string? Location = null,
        string? Notes = null);
}

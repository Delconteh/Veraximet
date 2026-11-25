using Veraximet.Domain.Enums;

namespace Veraximet.Contracts.Dto
{
    public sealed record InstrumentSearchCriteria(
        string? SearchText = null,
        string? Category = null,
        string? Department = null,  
        OperationalStatus? OperationalStatus = null,
        ConformityStatus? ConformityStatus = null,
        DueStatus? DueStatus = null,
        DateOnly? Today = null,
        int DueSoonDays = 30,
        bool IncludeArchived = false,
        int Page = 1,
        int PageSize = 50);
}

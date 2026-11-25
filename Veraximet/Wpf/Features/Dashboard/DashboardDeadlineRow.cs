using Veraximet.Contracts.Dto;
using Veraximet.Domain.Enums;

namespace Veraximet.Wpf.Features.Dashboard
{
    public sealed record DashboardDeadlineRow(Guid InstrumentId, string InstrumentCode, string InstrumentName, DateTime DueDate, string StatusCode, string StatusLabel)
    {
        public static DashboardDeadlineRow FromDto(UpcomingDeadlineDto dto) => new(
            dto.InstrumentId,
            dto.InstrumentCode,
            dto.InstrumentName,
            dto.DueDate.ToDateTime(TimeOnly.MinValue),
            dto.DueStatus.ToString(),
            dto.DueStatus == DueStatus.Overdue ? "Overdue" : "Due soon");
    }

}

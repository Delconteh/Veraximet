using Veraximet.Contracts.Dto;
using Veraximet.Domain.Enums;

namespace Veraximet.Wpf.Features.Instrument
{
    public sealed record InstrumentRow(Guid Id, string InstrumentCode, string InstrumentName, string Category, string Department, string SerialNumber, DateTime? NextDueDate, string DueStatusCode, string DueStatusLabel, string ConformityStatusCode, string ConformityStatusLabel, string OperationalStatusCode, string OperationalStatusLabel)
    {
        public static InstrumentRow FromDto(InstrumentDto dto, DateOnly today, int dueSoonDays = 30)
        {
            var dueStatus = dto.NextDueDate switch
            {
                null => DueStatus.NotScheduled,
                { } due when due < today => DueStatus.Overdue,
                { } due when due <= today.AddDays(dueSoonDays) => DueStatus.DueSoon,
                _ => DueStatus.Valid
            };

            return new InstrumentRow(dto.Id, dto.Code, dto.Name, dto.Category, dto.Department ?? "—", dto.SerialNumber ?? "—", dto.NextDueDate?.ToDateTime(TimeOnly.MinValue), dueStatus.ToString(), dueStatus
                switch
                {
                DueStatus.NotScheduled => "Not scheduled",
                DueStatus.DueSoon => "Due soon",
                DueStatus.Overdue => "Overdue",
                _ => "Valid"
                },
                dto.ConformityStatus.ToString(),
                dto.ConformityStatus switch
                {
                    ConformityStatus.NotEvaluated => "Not evaluated",
                    ConformityStatus.Conforming => "Conforming",
                    _ => "Nonconforming"
                },
                dto.OperationalStatus.ToString(),
                dto.OperationalStatus switch
                {
                    OperationalStatus.InService => "In service",
                    OperationalStatus.Suspended => "Suspended",
                    _ => "Archived"
                });
        }
    }

}

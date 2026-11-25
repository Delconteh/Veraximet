using Veraximet.Contracts.Dto;
using Veraximet.Contracts.Services;
using Veraximet.Domain.Enums;

namespace Veraximet.Infrastructure.Services
{
    public sealed class DashboardService : IDashboardService
    {
        private readonly IInstrumentService _instruments;
        private readonly TimeProvider _clock;
        private readonly int _dueSoonDays;

        public DashboardService(IInstrumentService instruments, TimeProvider? clock = null)
        {
            _instruments = instruments;
            _clock = clock ?? TimeProvider.System;
            _dueSoonDays = 30;
        }

        public async Task<DashboardSnapshotDto> GetSnapshotAsync(CancellationToken cancellationToken = default)
        {
            var all = new List<InstrumentDto>();
            var pageNumber = 1;

            while (true)
            {
                var page = await _instruments.SearchAsync(new InstrumentSearchCriteria(IncludeArchived: true, Page: pageNumber, PageSize: 200),cancellationToken);

                all.AddRange(page.Items);

                if (all.Count >= page.TotalCount || page.Items.Count == 0)
                    break;

                pageNumber++;
            }

            var today = DateOnly.FromDateTime(_clock.GetLocalNow().DateTime);
            var dueSoonEnd = today.AddDays(_dueSoonDays);
            var active = all.Where(i => i.OperationalStatus != OperationalStatus.Archived).ToList();

            var deadlines = active
                .Where(i => i.NextDueDate is { } due && due <= dueSoonEnd)
                .OrderBy(i => i.NextDueDate)
                .ThenBy(i => i.Code, StringComparer.OrdinalIgnoreCase)
                .Take(12)
                .Select(i => new UpcomingDeadlineDto( i.Id, i.Code, i.Name, i.NextDueDate!.Value, i.NextDueDate < today ? DueStatus.Overdue : DueStatus.DueSoon))
                .ToArray();

            return new DashboardSnapshotDto(
                TotalInstruments: all.Count,AvailableInstruments: active.Count(i =>
                    i.OperationalStatus == OperationalStatus.InService &&
                    i.ConformityStatus == ConformityStatus.Conforming &&
                    i.NextDueDate is { } due && due >= today),
                OverdueInstruments: active.Count(i => i.NextDueDate is { } due && due < today),
                DueSoonInstruments: active.Count(i => i.NextDueDate is { } due && due >= today && due <= dueSoonEnd),
                NonconformingInstruments: active.Count(i => i.ConformityStatus == ConformityStatus.Nonconforming),
                SuspendedInstruments: active.Count(i => i.OperationalStatus == OperationalStatus.Suspended),
                UpcomingDeadlines: deadlines
                );
        }
    }
}

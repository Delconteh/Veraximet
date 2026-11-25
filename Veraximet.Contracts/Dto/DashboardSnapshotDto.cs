using System;

namespace Veraximet.Contracts.Dto
{
    public sealed record DashboardSnapshotDto(
        int TotalInstruments,
        int AvailableInstruments,
        int OverdueInstruments,
        int DueSoonInstruments,
        int NonconformingInstruments,
        int SuspendedInstruments,
        IReadOnlyList<UpcomingDeadlineDto> UpcomingDeadlines);
}

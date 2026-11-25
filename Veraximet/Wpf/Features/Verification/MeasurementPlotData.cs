using Veraximet.Metrology;

namespace Veraximet.Wpf.Features.Verification
{
    public sealed record MeasurementPlotData(decimal Reference, decimal Mpe, IReadOnlyList<decimal> Readings, VerificationPointResult Result);
}

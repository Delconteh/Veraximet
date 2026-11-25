using System;

namespace Veraximet.Metrology
{
    public sealed record VerificationPointResult(int Count, decimal Mean, decimal MeanError, decimal MaximumAbsoluteError, decimal Range, double? StandardDeviation, bool IsWithinTolerance);
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Veraximet.Metrology
{
    public static class VerificationCalculator
    {
        public const string DecisionRule = "MaxAbsoluteError.v1";

        public static VerificationPointResult Calculate(decimal referenceValue, decimal maximumPermissibleError, IReadOnlyList<decimal> measurements)
        {
            if (measurements is null)
                throw new ArgumentNullException(nameof(measurements));

            if (measurements.Count == 0)
                throw new ArgumentException("At least one measurement is required.", nameof(measurements));


            if (maximumPermissibleError < 0)
                throw new ArgumentOutOfRangeException(nameof(maximumPermissibleError));

            decimal mean = measurements.Average();
            decimal meanError = mean - referenceValue;


            decimal maximumAbsoluteError = measurements.Max(x => Math.Abs(x - referenceValue));
            decimal range = measurements.Max() - measurements.Min();

            double? standardDeviation = CalculateStandardDeviation(measurements, mean);

            return new VerificationPointResult(measurements.Count, mean, meanError, maximumAbsoluteError, range, standardDeviation, maximumAbsoluteError <= maximumPermissibleError);
        }

        private static double? CalculateStandardDeviation(IReadOnlyList<decimal> measurements, decimal mean)
        {
            if (measurements.Count < 2)
                return null;


            double squaredDeviationSum = 0;

            foreach (decimal measurement in measurements)
            {
                double deviation = (double)(measurement - mean);
                squaredDeviationSum += deviation * deviation;
            }
            return Math.Sqrt(squaredDeviationSum / (measurements.Count - 1));
        }
    }
}

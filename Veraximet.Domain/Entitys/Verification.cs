using Veraximet.Domain.Enums;

namespace Veraximet.Domain.Entitys
{
    public sealed class Verification
    {
        public Guid Id { get; private set; }
        public Guid InstrumentId { get; private set; }
        public DateOnly Date { get; private set; }
        public string PerformedBy { get; private set; } = string.Empty;
        public string Unit { get; private set; } = string.Empty;
        public string? Notes { get; private set; }
        public string DecisionRule { get; private set; } = string.Empty;
        public ConformityStatus Result { get; private set; }
        public List<VerificationPoint> Points { get; private set; } = new();
        private Verification()
        {
        }

        public Verification(Guid instrumentId, DateOnly date, string performedBy, string unit, string? notes, string decisionRule, ConformityStatus result)
        {
            if (instrumentId == Guid.Empty)
                throw new ArgumentException("Instrument ID is required.");

            if (string.IsNullOrWhiteSpace(performedBy))
                throw new ArgumentException("Operator is required.");

            if (string.IsNullOrWhiteSpace(unit))
                throw new ArgumentException("Unit is required.");

            if (string.IsNullOrWhiteSpace(decisionRule))
                throw new ArgumentException("Decision rule is required.");

            if (result is not (ConformityStatus.Conforming or ConformityStatus.Nonconforming))
                throw new ArgumentOutOfRangeException(nameof(result));

            Id = Guid.NewGuid();
            InstrumentId = instrumentId;
            Date = date;
            PerformedBy = performedBy.Trim();
            Unit = unit.Trim();
            Notes = notes?.Trim();
            DecisionRule = decisionRule;
            Result = result;
        }

        public void AddPoint(decimal referenceValue, decimal maximumPermissibleError, string? description, IReadOnlyList<decimal> measurements) =>
            Points.Add(new VerificationPoint(Id, Points.Count + 1, referenceValue, maximumPermissibleError, description, measurements));

    }
}

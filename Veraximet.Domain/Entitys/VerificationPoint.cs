using System;

namespace Veraximet.Domain.Entitys
{
    public sealed class VerificationPoint
    {
        public Guid Id { get; private set; }
        public Guid VerificationId { get; private set; }
        public int Order { get; private set; }
        public decimal ReferenceValue { get; private set; }
        public decimal MaximumPermissibleError { get; private set; }
        public string? Description { get; private set; }
        public List<Measurement> Measurements { get; private set; } = new();
        private VerificationPoint()
        {
        }

        public VerificationPoint(Guid verificationId, int order, decimal referenceValue, decimal maximumPermissibleError, string? description, IReadOnlyList<decimal> measurements)
        {
            if (measurements is null)
                throw new ArgumentNullException(nameof(measurements));

            if (verificationId == Guid.Empty)
                throw new ArgumentException("Verification ID is required.");

            if (order < 1)
                throw new ArgumentOutOfRangeException(nameof(order));

            if (maximumPermissibleError < 0)
                throw new ArgumentOutOfRangeException(nameof(maximumPermissibleError));

            if (measurements.Count == 0)
                throw new ArgumentException("A point requires at least one reading.");

            Id = Guid.NewGuid();

            VerificationId = verificationId;
            Order = order;
            ReferenceValue = referenceValue;
            MaximumPermissibleError = maximumPermissibleError;
            Description = description?.Trim();

            for (int i = 0; i < measurements.Count; i++)
                Measurements.Add(new Measurement(Id, i + 1, measurements[i]));
        }
    }
}

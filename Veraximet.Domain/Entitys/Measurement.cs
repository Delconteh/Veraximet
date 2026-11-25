using System;


namespace Veraximet.Domain.Entitys
{
    public sealed class Measurement
    {
        public Guid Id { get; private set; }
        public Guid VerificationPointId { get; private set; }
        public int Order { get; private set; }
        public decimal Value { get; private set; }

        private Measurement()
        {
        }

        public Measurement(Guid verificationPointId, int order, decimal value)
        {
            if (verificationPointId == Guid.Empty)
                throw new ArgumentException("Point ID is required.");

            if (order < 1)
                throw new ArgumentOutOfRangeException(nameof(order));

            Id = Guid.NewGuid();
            VerificationPointId = verificationPointId;
            Order = order;
            Value = value;
        }
    }
}

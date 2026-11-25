using Veraximet.Domain.Enums;

namespace Veraximet.Domain.Entitys
{
    /// <summary>
    /// Instrument identity and business state.
    /// </summary>
    public sealed class Instrument
    {
        #region Variables

        #endregion

        #region Properties

        public Guid Id { get; private set; }
        public string Code { get; private set; } = string.Empty;



        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? Department { get; set; }
        public string? Location { get; set; }
        public string? Notes { get; set; }
        public int VerificationIntervalMonths { get; set; }
        public DateOnly? LastVerificationDate { get; private set; }
        public DateOnly? NextDueDate { get; private set; }
        public OperationalStatus OperationalStatus { get; private set; } = OperationalStatus.InService;
        public ConformityStatus ConformityStatus { get; private set; } = ConformityStatus.NotEvaluated;
        #endregion

        #region Constructors
        private Instrument()
        {
        }

        public Instrument(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Instrument code is required.", nameof(code));

            Id = Guid.NewGuid();
            Code = code;
        }
        #endregion

        #region Methods
        public void ChangeOperationalStatus(OperationalStatus status)
        {
            if (!Enum.IsDefined(status))
                throw new ArgumentOutOfRangeException(nameof(status));

            OperationalStatus = status;
        }

        public void RegisterVerification(DateOnly date, ConformityStatus result)
        {
            if (result is not (ConformityStatus.Conforming or ConformityStatus.Nonconforming))
                throw new ArgumentOutOfRangeException(nameof(result));

            if (LastVerificationDate is { } lastDate && date < lastDate)
                throw new ArgumentException("Verification date precedes the last verification.", nameof(date));

            if (VerificationIntervalMonths <= 0)
                throw new InvalidOperationException("Invalid verification interval.");

            var nextDueDate = result == ConformityStatus.Conforming ? date.AddMonths(VerificationIntervalMonths) : (DateOnly?)null;
            LastVerificationDate = date;
            NextDueDate = nextDueDate;
            ConformityStatus = result;
        }

        public DueStatus GetDueStatus(DateOnly today, int dueSoonDays = 30)
        {
            if (dueSoonDays < 0)
                throw new ArgumentOutOfRangeException(nameof(dueSoonDays));

            if (NextDueDate is not { } dueDate)
                return DueStatus.NotScheduled;

            if (dueDate < today)
                return DueStatus.Overdue;

            return dueDate <= today.AddDays(dueSoonDays) ? DueStatus.DueSoon : DueStatus.Valid;
        }
        #endregion
    }
}

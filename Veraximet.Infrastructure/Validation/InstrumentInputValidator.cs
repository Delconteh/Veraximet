using Veraximet.Contracts.Dto;

namespace Veraximet.Infrastructure.Validation
{
    /// <summary>Boundary validation/normalization</summary>
    internal static class InstrumentInputValidator
    {
        public static void ValidateSearch(InstrumentSearchCriteria criteria)
        {
            if (criteria is null)
                return;

            if (criteria.Page < 1)
                throw new ArgumentOutOfRangeException(nameof(criteria.Page));

            if (criteria.PageSize is < 1 or > 200)
                throw new ArgumentOutOfRangeException(nameof(criteria.PageSize), "PageSize must be 1–200.");

            if (criteria.DueSoonDays < 0)
                throw new ArgumentOutOfRangeException(nameof(criteria.DueSoonDays));
        }

        public static string NormalizeCode(string? code) =>
            Required(code, nameof(code), 40).ToUpperInvariant();

        public static InstrumentDetailsDto NormalizeDetails(InstrumentDetailsDto details)
        {
            if (details is null)
                throw new ArgumentNullException();

            if (details.VerificationIntervalMonths is < 1 or > 1200)
                throw new ArgumentOutOfRangeException(nameof(details.VerificationIntervalMonths), "Verification interval must be between 1 and 1200 months.");

            return details with
            {
                Name = Required(details.Name, nameof(details.Name), 160),
                Category = Required(details.Category, nameof(details.Category), 80),
                Description = Optional(details.Description, nameof(details.Description), 1000),
                Manufacturer = Optional(details.Manufacturer, nameof(details.Manufacturer), 120),
                Model = Optional(details.Model, nameof(details.Model), 120),
                SerialNumber = Optional(details.SerialNumber, nameof(details.SerialNumber), 100),
                Department = Optional(details.Department, nameof(details.Department), 100),
                Location = Optional(details.Location, nameof(details.Location), 160),
                Notes = Optional(details.Notes, nameof(details.Notes), 2000)
            };
        }

        private static string Required(string? value, string field, int maxLength)
        {
            var normalized = value?.Trim();

            if (string.IsNullOrWhiteSpace(normalized))
                throw new ArgumentException($"{field} is required.", field);

            if (normalized.Length > maxLength)
                throw new ArgumentException($"{field} cannot exceed {maxLength} characters.", field);

            return normalized;
        }

        private static string? Optional(string? value, string field, int maxLength)
        {
            var normalized = value?.Trim();

            if (string.IsNullOrEmpty(normalized))
                return null;

            if (normalized.Length > maxLength)
                throw new ArgumentException($"{field} cannot exceed {maxLength} characters.", field);


            return normalized;
        }
    }
}

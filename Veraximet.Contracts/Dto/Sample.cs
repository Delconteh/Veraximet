using Veraximet.Domain.Enums;

namespace Veraximet.Contracts.Dto
{
    public sealed record Sample(
        string Code, string Name, string Category, string Manufacturer,
        string Model, string SerialNumber, string Department, string Location,
        int IntervalMonths, OperationalStatus OperationalStatus);
}

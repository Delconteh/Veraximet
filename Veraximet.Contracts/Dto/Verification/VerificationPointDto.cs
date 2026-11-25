using System;

namespace Veraximet.Contracts.Dto.Verification
{
    public sealed record VerificationPointDto(decimal ReferenceValue,
       decimal MaximumPermissibleError, string? Description, IReadOnlyList<decimal> Measurements);
}

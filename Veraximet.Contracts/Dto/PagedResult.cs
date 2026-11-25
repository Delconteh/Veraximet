using System;

namespace Veraximet.Contracts.Dto
{
    public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

}

using System.Linq.Expressions;
using CookieBytes.Core.Application.Common;

namespace CookieBytes.Core.Application.Specifications;

/// <summary>
/// Composable query specification for EF-friendly filtering and paging.
/// </summary>
public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }

    IReadOnlyList<Expression<Func<T, object>>> Includes { get; }

    Expression<Func<T, object>>? OrderBy { get; }

    Expression<Func<T, object>>? OrderByDescending { get; }

    PagedQuery? Paging { get; }
}

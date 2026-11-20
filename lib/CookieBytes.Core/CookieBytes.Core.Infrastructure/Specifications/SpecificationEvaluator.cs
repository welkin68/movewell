using CookieBytes.Core.Application.Specifications;
using Microsoft.EntityFrameworkCore;

namespace CookieBytes.Core.Infrastructure.Specifications;

/// <summary>
/// Applies specifications to EF Core queries.
/// </summary>
public static class SpecificationEvaluator
{
    /// <summary>
    /// Applies criteria, includes, ordering, and paging from the specification to the query.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The base queryable.</param>
    /// <param name="specification">The specification to apply.</param>
    public static IQueryable<T> Apply<T>(IQueryable<T> query, ISpecification<T> specification)
        where T : class
    {
        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        query = specification.Includes.Aggregate(
            query,
            (current, include) => current.Include(include));

        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        if (specification.Paging is not null)
        {
            query = query
                .Skip(specification.Paging.Skip)
                .Take(specification.Paging.Take);
        }

        return query;
    }
}

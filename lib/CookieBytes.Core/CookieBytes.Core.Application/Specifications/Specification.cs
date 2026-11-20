using System.Linq.Expressions;
using CookieBytes.Core.Application.Common;

namespace CookieBytes.Core.Application.Specifications;

/// <summary>
/// Base specification with fluent helpers.
/// </summary>
public abstract class Specification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>>? Criteria { get; private set; }

    public IReadOnlyList<Expression<Func<T, object>>> Includes => _includes;

    public Expression<Func<T, object>>? OrderBy { get; private set; }

    public Expression<Func<T, object>>? OrderByDescending { get; private set; }

    public PagedQuery? Paging { get; private set; }

    private readonly List<Expression<Func<T, object>>> _includes = [];

    protected void AddCriteria(Expression<Func<T, bool>> criteria) => Criteria = criteria;

    protected void AddInclude(Expression<Func<T, object>> include) => _includes.Add(include);

    protected void ApplyOrderBy(Expression<Func<T, object>> orderBy) => OrderBy = orderBy;

    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescending) =>
        OrderByDescending = orderByDescending;

    protected void ApplyPaging(int pageNumber, int pageSize) => Paging = new PagedQuery(pageNumber, pageSize);
}

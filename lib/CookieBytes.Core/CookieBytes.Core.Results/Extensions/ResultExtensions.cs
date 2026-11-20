using FluentResults;

namespace CookieBytes.Core.Results.Extensions;

public static class ResultExtensions
{
    public static Result<TDestination> ToResult<TDestination>(this Result source)
    {
        if (source.IsSuccess)
        {
            return Result.Fail<TDestination>("Cannot map a successful result without a value.");
        }

        return Result.Fail<TDestination>(source.Errors);
    }

    public static Result<TDestination> Map<TSource, TDestination>(
        this Result<TSource> source,
        Func<TSource, TDestination> mapper)
    {
        if (source.IsFailed)
        {
            return Result.Fail<TDestination>(source.Errors);
        }

        return Result.Ok(mapper(source.Value));
    }

    public static async Task<Result<TDestination>> MapAsync<TSource, TDestination>(
        this Result<TSource> source,
        Func<TSource, Task<TDestination>> mapper)
    {
        if (source.IsFailed)
        {
            return Result.Fail<TDestination>(source.Errors);
        }

        return Result.Ok(await mapper(source.Value));
    }

    public static Result<T> WithErrorsFrom<T>(this Result<T> target, Result source)
    {
        foreach (var error in source.Errors)
        {
            target.WithError(error);
        }

        return target;
    }
}

namespace WebApi.Extensions;

public static class ListExtensions
{
    private static string[] _directionOptions = ["asc", "desc"];

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> query, object? obj, Func<T, bool> expression)
    {
        return obj is not null ? query.Where(expression) : query;
    }
}
using System.Collections.Generic;
using System.Linq;

namespace MemeManager.Extensions;

public static class LinqExtensions
{
    // https://stackoverflow.com/a/39997157/1687436
    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
    {
        return source.Select((item, index) => (item, index));
    }
}

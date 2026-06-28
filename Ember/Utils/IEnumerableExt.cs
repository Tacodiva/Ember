
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ember.Utils;

public static class IEnumerableExt {
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
        foreach (var item in source) action(item);
    }

    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self) {
        int index = 0;
        foreach (T item in self) yield return (item, index++);
    }
}
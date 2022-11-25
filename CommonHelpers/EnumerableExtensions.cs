using System.Collections.Generic;
using System.Linq;
namespace CommonHelpers;
public static class EnumerableExtensions {
	public static IReadOnlyList<TItem> ToReadOnlyList<TItem>(this IEnumerable<TItem> items) {
		return items.ToList();
	}
	public static IEnumerable<(int, TItem)> Enumerate<TItem>(this IEnumerable<TItem> items) {
		var index = 0;
		foreach (var item in items) {
			yield return (index, item);
			index += 1;
		}
	}
	public static IEnumerable<TItem> WhereNotNull<TItem>(this IEnumerable<TItem?> items) where TItem : class {
		return items.Where(item => item != null)!;
	}
	public static IEnumerable<TItem> WhereHasValue<TItem>(this IEnumerable<TItem?> source) where TItem : struct {
		foreach (var item in source) {
			if (item != null) {
				yield return item.Value;
			}
		}
	}
	public static IEnumerable<(TItem, TItem)> AdjacentPairs<TItem>(this IEnumerable<TItem> items) {
		// could use https://github.com/morelinq/MoreLINQ/blob/v3.3.2/MoreLinq/Window.cs
		using var enumerator = items.GetEnumerator();
		if (enumerator.MoveNext()) {
			var prevItem = enumerator.Current;
			while (enumerator.MoveNext()) {
				var item = enumerator.Current;
				yield return (prevItem, item);
				prevItem = item;
			}
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
namespace CommonHelpers;
public class QueryParameters : IEnumerable {
	readonly OrderedDictionary<string, object?> items = new();
	public IEnumerable<string> Names => items.Keys;
	public IEnumerable<KeyValuePair<string, object?>> NameValuePairs => items;
	public void Add<T>(string name, T value) {
		items.Add(name, value);
	}
	public void Add(QueryParameters parameters) {
		foreach (var (name, value) in parameters.NameValuePairs) {
			items.Add(name, value);
		}
	}
	public object? GetValue(string name) {
		return items[name];
	}
	public override string ToString() {
		return string.Join(", ", NameValuePairs);
	}
	IEnumerator IEnumerable.GetEnumerator() {
		throw new NotSupportedException();
	}
}

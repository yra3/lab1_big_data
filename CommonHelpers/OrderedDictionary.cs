using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
namespace CommonHelpers;
public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue> where TKey : notnull {
	readonly List<TKey> keys;
	readonly Dictionary<TKey, TValue> valueByKey;
	public OrderedDictionary() : this(null) {
	}
	public OrderedDictionary(IEqualityComparer<TKey>? comparer) {
		valueByKey = new(comparer);
		keys = new();
	}
	ICollection<KeyValuePair<TKey, TValue>> valueByKeyAsCollection => valueByKey;
	#region IDictionary
	public TValue this[TKey key] {
		get => valueByKey[key];
		set {
			if (!valueByKey.ContainsKey(key)) {
				keys.Add(key);
			}
			valueByKey[key] = value;
		}
	}
	public ICollection<TKey> Keys => keys;
	ICollection<TValue> IDictionary<TKey, TValue>.Values => throw new NotSupportedException();
	public void Add(TKey key, TValue value) {
		valueByKey.Add(key, value);
		keys.Add(key);
	}
	public bool ContainsKey(TKey key) {
		return valueByKey.ContainsKey(key);
	}
	public bool Remove(TKey key) {
		if (valueByKey.Remove(key)) {
			var index = keys.FindIndex(candidateKey => valueByKey.Comparer.Equals(candidateKey, key));
			keys.RemoveAt(index);
			return true;
		}
		return false;
	}
	public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) {
		return valueByKey.TryGetValue(key, out value);
	}
	#endregion
	#region ICollection
	public int Count => valueByKey.Count;
	bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => valueByKeyAsCollection.IsReadOnly;
	void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) {
		Add(item.Key, item.Value);
	}
	public void Clear() {
		valueByKey.Clear();
		keys.Clear();
	}
	bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) {
		return valueByKeyAsCollection.Contains(item);
	}
	void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
		foreach (var kv in this) {
			array[arrayIndex] = kv;
			arrayIndex += 1;
		}
	}
	bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) {
		throw new NotSupportedException();
	}
	#endregion
	#region IEnumerable
	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
		foreach (var key in keys) {
			yield return new(key, valueByKey[key]);
		}
	}
	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}
	#endregion
	#region IReadOnlyDictionary
	IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => keys;
	public IEnumerable<TValue> Values => this.Select(kv => kv.Value);
	#endregion
}

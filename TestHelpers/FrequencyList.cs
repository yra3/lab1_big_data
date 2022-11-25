using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace TestHelpers;
public class FrequencyList<TValue> {
	readonly IReadOnlyList<TValue> values;
	readonly IReadOnlyList<long> weights;
	readonly Lazy<WeightedRandomIndexGenerator> indexGenerator;
	FrequencyList(IReadOnlyList<TValue> values, IReadOnlyList<long> weights) {
		if (values.Count != weights.Count) {
			throw new InvalidOperationException();
		}
		this.values = values;
		this.weights = weights;
		indexGenerator = new Lazy<WeightedRandomIndexGenerator>(() => new(weights));
	}
	public IReadOnlyList<TValue> Values => values;
	public IReadOnlyList<long> Weights => weights;
	public IEnumerable<Item> Items => values.Zip(weights, (value, weight) => new Item(value, weight));
	WeightedRandomIndexGenerator IndexGenerator => indexGenerator.Value;
	public TValue NextValue(Random random) {
		var index = IndexGenerator.NextIndex(random);
		return values[index];
	}
	public readonly record struct Item(TValue value, long weight);
	public class Builder : IEnumerable {
		readonly List<TValue> values = new();
		readonly List<long> weights = new();
		public void Add(TValue value, long weight) {
			values.Add(value);
			weights.Add(weight);
		}
		public void Add((TValue, int) item) {
			Add(item.Item1, item.Item2);
		}
		public void Add(IEnumerable<(TValue, int)> items) {
			foreach (var item in items) {
				Add(item);
			}
		}
		public void Add(IEnumerable<Item> items) {
			foreach (var item in items) {
				Add(item.value, item.weight);
			}
		}
		public void Add(FrequencyList<TValue> list) {
			Add(list.Items);
		}
		public FrequencyList<TValue> Build() {
			return new FrequencyList<TValue>(values.ToList(), weights.ToList());
		}
		IEnumerator IEnumerable.GetEnumerator() {
			throw new NotSupportedException();
		}
	}
}

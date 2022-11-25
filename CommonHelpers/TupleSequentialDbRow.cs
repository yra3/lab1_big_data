using System;
namespace CommonHelpers;
class TupleSequentialDbRow : SequentialDbRow {
	object?[] values;
	int lastSequentialIndex = -1;
	public TupleSequentialDbRow() : this(Array.Empty<object?>()) {
	}
	public TupleSequentialDbRow(object?[] values) {
		this.values = values;
	}
	public void SetValues(object?[] values) {
		this.values = values;
		lastSequentialIndex = -1;
	}
	public override object ReadObjectValue() {
		var value = ReadObjectValueOrNull();
		if (value == null) {
			throw new ArgumentNullException($"{lastSequentialIndex}", "Unexpected DBNull");
		}
		return value;
	}
	public override object? ReadObjectValueOrNull() {
		lastSequentialIndex += 1;
		var value = values[lastSequentialIndex];
		if (value == DBNull.Value) {
			throw new ArgumentNullException($"{lastSequentialIndex}", "Unexpected DBNull");
		}
		return value;
	}
}

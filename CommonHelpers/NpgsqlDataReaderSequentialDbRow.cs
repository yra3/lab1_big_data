using Npgsql;
using System;
namespace CommonHelpers;
class NpgsqlDataReaderSequentialDbRow : SequentialDbRow {
	readonly NpgsqlDataReader reader;
	int lastSequentialIndex = -1;
	public NpgsqlDataReaderSequentialDbRow(NpgsqlDataReader reader) {
		this.reader = reader;
	}
	public void FinishRow() {
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
		var value = reader.GetValue(lastSequentialIndex);
		if (value == null) {
			throw new ArgumentNullException($"at {lastSequentialIndex}");
		}
		if (value == DBNull.Value) {
			return null;
		}
		return value;
	}
}

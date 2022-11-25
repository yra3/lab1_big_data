using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
namespace CommonHelpers;
class NpgsqlDataReaderDbRow : DbRow {
	readonly NpgsqlDataReader reader;
	Dictionary<string, int>? columnIndexes;
	object[]? bufferedValues;
	bool bufferedValuesAreDirty = true;
	public NpgsqlDataReaderDbRow(NpgsqlDataReader reader) {
		this.reader = reader;
	}
	public void FinishRow() {
		bufferedValuesAreDirty = true;
	}
	public override bool HasColumn(string name) {
		EnsureBuffered();
		return columnIndexes.ContainsKey(name);
	}
	public override object GetObjectValue(string name) {
		var value = GetObjectValueOrNull(name);
		if (value == null) {
			throw new ArgumentNullException(name, "Unexpected DBNull");
		}
		return value;
	}
	public override object? GetObjectValueOrNull(string name) {
		EnsureBuffered();
		var index = columnIndexes[name];
		var value = bufferedValues[index];
		if (value == DBNull.Value) {
			return null;
		}
		return value;
	}
	[MemberNotNull(nameof(columnIndexes), nameof(bufferedValues))]
	void EnsureBuffered() {
		if (columnIndexes == null) {
			columnIndexes = GetColumnIndexes(reader);
		}
		if (bufferedValues == null) {
			bufferedValues = new object[reader.FieldCount];
		}
		if (bufferedValuesAreDirty) {
			FillBufferedValues(bufferedValues, reader);
			bufferedValuesAreDirty = false;
		}
	}
	static Dictionary<string, int> GetColumnIndexes(NpgsqlDataReader reader) {
		var fieldCount = reader.FieldCount;
		var indexes = new Dictionary<string, int>(fieldCount);
		for (var i = 0; i < fieldCount; i++) {
			var name = reader.GetName(i);
			indexes.Add(name, i);
		}
		return indexes;
	}
	static void FillBufferedValues(object[] bufferedValues, NpgsqlDataReader reader) {
		var n = bufferedValues.Length;
		if (reader.FieldCount != n) {
			throw new InvalidOperationException($"{nameof(reader.FieldCount)} changed from {n} to {reader.FieldCount}");
		}
		for (var i = 0; i < bufferedValues.Length; i++) {
			var bufferedValue = reader.GetValue(i);
			if (bufferedValue == null) {
				throw new ArgumentNullException($"at {i}");
			}
			bufferedValues[i] = bufferedValue;
		}
	}
}

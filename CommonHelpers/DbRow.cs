using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
namespace CommonHelpers;
public class DbRow {
	readonly NpgsqlDataReader reader;
	Dictionary<string, int>? columnIndexes;
	object[]? bufferedValues;
	bool bufferedValuesAreDirty = true;
	int lastSequentialIndex = -1;
	public DbRow(NpgsqlDataReader reader) {
		this.reader = reader;
	}
	internal void FinishRow() {
		bufferedValuesAreDirty = true;
		lastSequentialIndex = -1;
	}
	public bool HasColumn(string name) {
		EnsureBuffered();
		return columnIndexes.ContainsKey(name);
	}
	public object GetObjectValue(string name) {
		var value = GetObjectValueOrNull(name);
		if (value == null) {
			throw new ArgumentNullException(name, "Unexpected DBNull");
		}
		return value;
	}
	public object? GetObjectValueOrNull(string name) {
		EnsureBuffered();
		var index = columnIndexes[name];
		var value = bufferedValues[index];
		if (value == DBNull.Value) {
			return null;
		}
		return value;
	}
	public object ReadObjectValue() {
		var value = ReadObjectValueOrNull();
		if (value == null) {
			throw new ArgumentNullException($"{lastSequentialIndex}", "Unexpected DBNull");
		}
		return value;
	}
	public object? ReadObjectValueOrNull() {
		if (!bufferedValuesAreDirty) {
			throw new InvalidOperationException("Buffered reading is already started");
		}
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
	[MemberNotNull(nameof(columnIndexes), nameof(bufferedValues))]
	void EnsureBuffered() {
		if (lastSequentialIndex != -1) {
			throw new InvalidOperationException("Sequential reading is already started");
		}
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
	public bool GetBool(string name) {
		return (bool)GetObjectValue(name);
	}
	public bool? GetBoolOrNull(string name) {
		return (bool?)GetObjectValueOrNull(name);
	}
	public int GetInt(string name) {
		return (int)GetObjectValue(name);
	}
	public int? GetIntOrNull(string name) {
		return (int?)GetObjectValueOrNull(name);
	}
	public long GetLong(string name) {
		return (long)GetObjectValue(name);
	}
	public long? GetLongOrNull(string name) {
		return (long?)GetObjectValueOrNull(name);
	}
	public double GetDouble(string name) {
		return (double)GetObjectValue(name);
	}
	public double? GetDoubleOrNull(string name) {
		return (double?)GetObjectValueOrNull(name);
	}
	public string GetString(string name) {
		return (string)GetObjectValue(name);
	}
	public string? GetStringOrNull(string name) {
		return (string?)GetObjectValueOrNull(name);
	}
	public Guid GetGuid(string name) {
		return (Guid)GetObjectValue(name);
	}
	public Guid? GetGuidOrNull(string name) {
		return (Guid?)GetObjectValueOrNull(name);
	}
	public DateTime GetUtcDateTime(string name) {
		return GetUtcDateTimeOrNull(name)!.Value; // throw on null
	}
	public DateTime? GetUtcDateTimeOrNull(string name) {
		var value = (DateTime?)GetObjectValueOrNull(name);
		if (value.HasValue && value.Value.Kind != DateTimeKind.Utc) {
			throw new InvalidOperationException($"Invalid {nameof(DateTime.Kind)} ({value.Value.Kind}), expected {DateTimeKind.Utc}");
		}
		return value;
	}
	public DateTime GetUnspecifiedDateTimeAsUtc(string name) {
		return GetUnspecifiedDateTimeAsUtcOrNull(name)!.Value; // throw on null
	}
	public DateTime? GetUnspecifiedDateTimeAsUtcOrNull(string name) {
		var value = (DateTime?)GetObjectValueOrNull(name);
		return value?.UnspecifiedToUtc();
	}
	public TimeSpan GetTimeSpan(string name) {
		return (TimeSpan)GetObjectValue(name);
	}
	public TimeSpan? GetTimeSpanOrNull(string name) {
		return (TimeSpan?)GetObjectValueOrNull(name);
	}
	public byte[] GetByteArray(string name) {
		return (byte[])GetObjectValue(name);
	}
	public byte[]? GetByteArrayOrNull(string name) {
		return (byte[]?)GetObjectValueOrNull(name);
	}
}

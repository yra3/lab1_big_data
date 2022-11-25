using System;
using System.Collections.Generic;
namespace CommonHelpers;
public abstract class DbRow {
	public abstract bool HasColumn(string name);
	public abstract object GetObjectValue(string name);
	public abstract object? GetObjectValueOrNull(string name);
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
	public TItem GetTuple<TItem>(string name, Func<SequentialDbRow, TItem> getItem) {
		var rowValues = (object?[])GetObjectValue(name);
		return getItem(new TupleSequentialDbRow(rowValues));
	}
	public TItem GetTupleOrNull<TItem>(string name, Func<SequentialDbRow?, TItem> getItem) {
		var rowValues = (object?[]?)GetObjectValueOrNull(name);
		return getItem(rowValues == null ? null : new TupleSequentialDbRow(rowValues));
	}
	public IReadOnlyList<TItem> GetTupleArray<TItem>(string name, Func<SequentialDbRow, TItem> getItem) {
		var arrayValue = (object?[]?)GetObjectValueOrNull(name);
		if (arrayValue == null) {
			return Array.Empty<TItem>();
		}
		var items = new List<TItem>();
		var dbRow = new TupleSequentialDbRow();
		for (var i = 0; i < arrayValue.Length; i++) {
			var values = (object?[]?)arrayValue[i];
			TItem item;
			if (values == null) {
				throw new InvalidOperationException($"Unexpected null at {i}");
			}
			dbRow.SetValues(values);
			item = getItem(dbRow);
			items.Add(item);
		}
		return items;
	}
}

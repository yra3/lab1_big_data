using System;
namespace CommonHelpers;
public abstract class SequentialDbRow {
	public abstract object ReadObjectValue();
	public abstract object? ReadObjectValueOrNull();
	public bool ReadBool() {
		return (bool)ReadObjectValue();
	}
	public bool? ReadBoolOrNull() {
		return (bool?)ReadObjectValueOrNull();
	}
	public int ReadInt() {
		return (int)ReadObjectValue();
	}
	public int? ReadIntOrNull() {
		return (int?)ReadObjectValueOrNull();
	}
	public long ReadLong() {
		return (long)ReadObjectValue();
	}
	public long? ReadLongOrNull() {
		return (long?)ReadObjectValueOrNull();
	}
	public double ReadDouble() {
		return (double)ReadObjectValue();
	}
	public double? ReadDoubleOrNull() {
		return (double?)ReadObjectValueOrNull();
	}
	public string ReadString() {
		return (string)ReadObjectValue();
	}
	public string? ReadStringOrNull() {
		return (string?)ReadObjectValueOrNull();
	}
	public Guid ReadGuid() {
		return (Guid)ReadObjectValue();
	}
	public Guid? ReadGuidOrNull() {
		return (Guid?)ReadObjectValueOrNull();
	}
	public DateTime ReadUtcDateTime() {
		return ReadUtcDateTimeOrNull()!.Value; // throw on null
	}
	public DateTime? ReadUtcDateTimeOrNull() {
		var value = (DateTime?)ReadObjectValueOrNull();
		if (value.HasValue && value.Value.Kind != DateTimeKind.Utc) {
			throw new InvalidOperationException($"Invalid {nameof(DateTime.Kind)} ({value.Value.Kind}), expected {DateTimeKind.Utc}");
		}
		return value;
	}
	public DateTime ReadUnspecifiedDateTimeAsUtc() {
		return ReadUnspecifiedDateTimeAsUtcOrNull()!.Value; // throw on null
	}
	public DateTime? ReadUnspecifiedDateTimeAsUtcOrNull() {
		var value = (DateTime?)ReadObjectValueOrNull();
		return value?.UnspecifiedToUtc();
	}
	public TimeSpan ReadTimeSpan() {
		return (TimeSpan)ReadObjectValue();
	}
	public TimeSpan? ReadTimeSpanOrNull() {
		return (TimeSpan?)ReadObjectValueOrNull();
	}
	public byte[] ReadByteArray() {
		return (byte[])ReadObjectValue();
	}
	public byte[]? ReadByteArrayOrNull() {
		return (byte[]?)ReadObjectValueOrNull();
	}
}

using System;
namespace CommonHelpers;
public static class DateTimeExtensions {
	public static DateTime UtcToUnspecified(this DateTime dt) {
		if (dt.Kind != DateTimeKind.Utc) {
			throw new InvalidOperationException($"Invalid {nameof(DateTime.Kind)} ({dt.Kind}), expected {DateTimeKind.Utc}");
		}
		return DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
	}
	public static DateTime UnspecifiedToUtc(this DateTime dt) {
		if (dt.Kind != DateTimeKind.Unspecified) {
			throw new InvalidOperationException($"Invalid {nameof(DateTime.Kind)} ({dt.Kind}), expected {DateTimeKind.Unspecified}");
		}
		return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
	}
}

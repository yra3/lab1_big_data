using System;
using System.Globalization;
namespace CommonHelpers;
public static class DateTimeHelper {
	public static DateTime ParseUtcIso(string s) {
		return DateTime.ParseExact(s, "s", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
	}
}

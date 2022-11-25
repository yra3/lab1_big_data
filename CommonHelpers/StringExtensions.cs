namespace CommonHelpers;
public static class StringExtensions {
	public static string ToLiteral(this string s) {
		return '\'' + s.Replace("'", "''") + '\'';
	}
}

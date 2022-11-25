using System;
namespace CommonHelpers;
public class Logger {
	public static void Log(params object?[] values) {
		Console.WriteLine(string.Join(" ", values));
	}
}

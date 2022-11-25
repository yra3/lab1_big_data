using System;
namespace CommonHelpers;
public class Logger {
	public static T Log<T>(T value) {
		Console.WriteLine(AddEmptyLine(SerializeValue(value)));
		return value;
	}
	static string SerializeValue(object? value) {
		if (value is string stringValue) {
			return stringValue;
		}
		return Serializer.ToJson(value);
	}
	static string AddEmptyLine(string message) {
		if (message.Contains('\n')) {
			message = "\n" + message;
		}
		return message;
	}
}

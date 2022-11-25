using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
namespace TestHelpers;
public static class Check {
	public static void IsTrue([DoesNotReturnIf(false)] bool condition, [CallerArgumentExpression("condition")] string? conditionMessage = null) {
		if (condition) {
			return;
		}
		throw new Exception(conditionMessage);
	}
}

using CommonHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
namespace TestHelpers;
public static class BuiltInFrequencyLists {
	public static FrequencyList<string> Get(string resourceName) {
		var text = ReadTextFileResource(resourceName);
		var lines = text.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		var list = new FrequencyList<string>.Builder();
		foreach (var line in lines) {
			Exception MakeError(string message) {
				throw new InvalidOperationException($"{message} in {resourceName.ToLiteral()}");
			}
			var match = lineRegex.Match(line);
			if (!match.Success) {
				throw MakeError($"Bad line {line.ToLiteral()}");
			}
			var word = match.Groups[1].Value.Replace("\\n", "\n");
			var frequencyString = match.Groups[2].Value;
			if (!long.TryParse(frequencyString, NumberStyles.None, CultureInfo.InvariantCulture, out var frequency) || frequency < 0) {
				throw MakeError($"Bad frequency {frequencyString.ToLiteral()}");
			}
			list.Add(word, frequency);
		}
		return list.Build();
	}
	static readonly Regex lineRegex = new(
		@"\A(.*?) (\d+)\z",
		RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline
	);
	public static IReadOnlyDictionary<string, FrequencyList<string>> GetByPattern(string regexPattern) {
		var listByName = new Dictionary<string, FrequencyList<string>>();
		foreach (var name in GetResourceNamesByPattern(regexPattern)) {
			listByName.Add(name, Get(name));
		}
		return listByName;
	}
	static IEnumerable<string> GetResourceNamesByPattern(string regexPattern) {
		var regex = new Regex(@"\A" + regexPattern + @"\z");
		foreach (var name in GetResourceNames()) {
			if (regex.IsMatch(name)) {
				yield return name;
			}
		}
	}
	static readonly Assembly assembly = typeof(BuiltInFrequencyLists).Assembly;
	static readonly string resourceNamePrefix = $"{typeof(BuiltInFrequencyLists).Namespace}.frequency_lists.";
	static readonly string resourceNameSuffix = $".txt";
	static IEnumerable<string> GetResourceNames() {
		var names = assembly.GetManifestResourceNames();
		foreach (var name in names) {
			if (
				resourceNamePrefix.Length + resourceNameSuffix.Length < name.Length
				&& name.StartsWith(resourceNamePrefix, StringComparison.Ordinal)
				&& name.EndsWith(resourceNameSuffix, StringComparison.Ordinal)
			) {
				yield return name[resourceNamePrefix.Length..^resourceNameSuffix.Length];
			}
		}
	}
	static string ReadTextFileResource(string resourceName) {
		var fullResourceName = $"{resourceNamePrefix}{resourceName}{resourceNameSuffix}";
		using var stream = assembly.GetManifestResourceStream(fullResourceName);
		if (stream == null) {
			throw new InvalidOperationException($"Resource {fullResourceName} not found, available:\n {string.Join("\n ", assembly.GetManifestResourceNames())}");
		}
		using var streamReader = new StreamReader(stream, System.Text.Encoding.UTF8);
		return streamReader.ReadToEnd();
	}
}

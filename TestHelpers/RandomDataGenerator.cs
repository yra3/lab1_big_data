using CommonHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
namespace TestHelpers;
public record RandomDataGenerator(Random Random) {
	public static readonly DateTime DefaultCurrentDateTime = DateTimeHelper.ParseUtcIso("2021-10-15T10:00:00");
	public DateTime CurrentDateTime { get; init; } = DefaultCurrentDateTime;
	public RandomDataGenerator(int seed) : this(new Random(seed)) {
	}
	static readonly Data data = new();
	public TItem NextArrayElement<TItem>(IReadOnlyList<TItem> array) {
		return array[Random.Next(array.Count)];
	}
	public IEnumerable<TItem> NextArrayElements<TItem>(IEnumerable<TItem> items, double probabilityOfInclusion) {
		foreach (var item in items) {
			if (NextBool(probabilityOfInclusion)) {
				yield return item;
			}
		}
	}
	public bool NextBool(double probabilityOfTrue) {
		return Random.NextDouble() < probabilityOfTrue;
	}
	public Guid NextGuid() {
		Span<byte> bytes = stackalloc byte[16];
		Random.NextBytes(bytes);
		return new Guid(bytes);
	}
	static readonly TimeSpan defaultAverageDelta = TimeSpan.FromSeconds(1);
	public DateTime NextUtcDateTime(TimeSpan? averageDelta = null) {
		var deltaTicks = (averageDelta ?? TimeSpan.FromDays(30)).Ticks * 2;
		return CurrentDateTime - TimeSpan.FromTicks(Random.NextInt64(deltaTicks));
	}
	public string NextHandle(int averageLength = 10, int minLength = 4) {
		var length = Math.Max(minLength, Random.Next(0, averageLength * 2));
		return string.Concat(Enumerable.Range(0, length).Select(_ => data.HandleCharacters.NextValue(Random)));
	}
	public string NextName(int averageWordCount = 2) {
		var wordCount = Random.Next(1, averageWordCount * 2);
		return string.Join(" ", Enumerable.Range(0, wordCount).Select(_ => ToTitleCase(data.Words.NextValue(Random))));
	}
	public string NextWord() {
		return data.Words.NextValue(Random);
	}
	public string NextText(
		int? averageMinimumLength = null,
		int minimumLength = 0,
		int? averageMinimumWordCountInSentence = null,
		int minimumWordCountInsentence = 0,
		int minimumSentenceLength = 0
	) {
		var minLength = minimumLength;
		if (averageMinimumLength != null) {
			minLength = Math.Max(minLength, Random.Next(averageMinimumLength.Value * 2));
		}
		var sb = new StringBuilder();
		while (true) {
			sb.Append(NextSentence(
				averageMinimumWordCount: averageMinimumWordCountInSentence,
				minimumWordCount: minimumWordCountInsentence,
				minimumLength: minimumSentenceLength,
				withEnding: true
			));
			if (sb.Length > minLength) {
				break;
			}
			sb.Append(data.SentenceSeparators.NextValue(Random));
		}
		return sb.ToString();
	}
	public string NextSentence(
		int? averageMinimumWordCount = 0,
		int minimumWordCount = 0,
		int minimumLength = 0,
		bool withEnding = false
	) {
		var minWordCount = minimumWordCount;
		if (averageMinimumWordCount != null) {
			minWordCount = Math.Max(minWordCount, Random.Next(averageMinimumWordCount.Value * 2));
		}
		var sb = new StringBuilder();
		var language = data.SentenceLanguages.NextValue(Random);
		var words = data.WordsByLanguage[language];
		sb.Append(ToTitleCase(words.NextValue(Random)));
		var ending = withEnding ? data.SentenceEndings.NextValue(Random) : "";
		var wordCount = 1;
		while (wordCount < minWordCount || sb.Length + ending.Length < minimumLength) {
			sb.Append(data.WordSeparators.NextValue(Random));
			sb.Append(words.NextValue(Random));
			wordCount += 1;
		}
		sb.Append(ending);
		return sb.ToString();
	}
	static string ToTitleCase(string s) {
		return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s);
	}
	class Data {
		public readonly FrequencyList<string> SentenceLanguages = BuiltInFrequencyLists.Get("sentence-languages");
		public readonly IReadOnlyDictionary<string, FrequencyList<string>> WordsByLanguage = BuiltInFrequencyLists.GetByPattern("words-.*");
		public readonly FrequencyList<string> Words;
		public readonly FrequencyList<string> WordSeparators = BuiltInFrequencyLists.Get("word-separators");
		public readonly FrequencyList<string> SentenceSeparators = BuiltInFrequencyLists.Get("sentence-separators");
		public readonly FrequencyList<string> SentenceEndings = BuiltInFrequencyLists.Get("sentence-endings");
		public readonly FrequencyList<char> HandleCharacters = new FrequencyList<char>.Builder {
			new[]{
				Enumerable.Range('a', 'z' - 'a' + 1).Select(code => (char)code).Select(c => (c, 10)),
				Enumerable.Range('0', '9' - '0' + 1).Select(code => (char)code).Select(c => (c, 2)),
				new[] { ('-', 1) },
			}.SelectMany(item => item),
		}.Build();
		public Data() {
			Words = new FrequencyList<string>.Builder() {
				WordsByLanguage.OrderBy(kv => kv.Key, StringComparer.Ordinal).SelectMany(kv => kv.Value.Items)
			}.Build();
		}
	}
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace CommonHelpers;
public class Tracer {
	readonly ScopeStatsBuilder globalStatsBuilder = new("Total");
	readonly ConcurrentDictionary<string, ScopeStatsBuilder> statsBuilderByName = new();
	public async Task PrintStatsContinuously(int interval, CancellationToken ct = default) {
		while (!ct.IsCancellationRequested) {
			PrintStats(false);
			await Task.Delay(interval, ct);
		}
	}
	public async Task RunAction(string name, Func<Task> action) {
		await Run(name, async () => {
			await action();
			return 0;
		});
	}
	public async Task<TResult> Run<TResult>(string name, Func<Task<TResult>> action) {
		var statsBuilder = statsBuilderByName.GetOrAdd(name, _ => new ScopeStatsBuilder(name));
		var startTimestamp = GetTimestamp();
		Exception? caughtException = null;
		bool SetException(Exception e) {
			caughtException = e;
			return false;
		}
		try {
			return await action();
		}
		catch (Exception e) when (SetException(e)) {
		}
		finally {
			var endTimestamp = GetTimestamp();
			statsBuilder.AddCompleted(startTimestamp, endTimestamp, caughtException);
			globalStatsBuilder.AddCompleted(startTimestamp, endTimestamp, caughtException);
		}
		throw new InvalidOperationException();
	}
	static double GetTimestamp() {
		return (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency;
	}
	record Column(string Name, int Width, Func<ScopeStats, string> GetText);
	static readonly IReadOnlyList<Column> columns = new Column[]{
		new ("Name", -30, s => s.Name),
		new ("AvgTime,ms", 10, s => DurationToString(s.AvgDuration)),
		new ("[min, max]", 15, s => $"[{DurationToString(s.MinDuration)} {DurationToString(s.MaxDuration)}]"),
		new ("TotTasks", 10, s => FormattableString.Invariant($"{s.TotalCount}")),
		new ("TotTime", 10, s => DurationToString(s.TotalDuration)),
		new ("TotErrors", 10, s => FormattableString.Invariant($"{s.NumberOfErrors}")),
		new ("TotTasks/s", 10, s => FormattableString.Invariant($"{s.TotalCountPerSecond:F1}")),
		new ("Tasks/s", 10, s => FormattableString.Invariant($"{s.TasksPerSecond:F1}")),
	};
	static readonly IReadOnlyList<Column> headerColumns = columns.Select(column => column with {
		GetText = _ => column.Name,
	}).ToArray();
	static string DurationToString(double duration) {
		return FormattableString.Invariant($"{duration * 1000:F1}");
	}
	void PrintStats(bool isFinal) {
		Logger.Log(string.Join("\n", GetStatsRows(isFinal)));
	}
	IEnumerable<string> GetStatsRows(bool isFinal) {
		yield return FormattableString.Invariant($"{(isFinal ? "Final stats" : "Stats")} at {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
		var globalStats = globalStatsBuilder.GetStats();
		yield return FormatRow(headerColumns, globalStats, 0);
		yield return FormatRow(columns, globalStats, 1);
		foreach (var statsBuilder in statsBuilderByName.Values.OrderBy(stats => stats.Name)) {
			var stats = statsBuilder.GetStats();
			yield return FormatRow(columns, stats, 2);
		}
	}
	static string FormatRow(IReadOnlyList<Column> columns, ScopeStats stats, int firstColumnIndent) {
		return string.Join(" ", columns.Select((column, index) => {
			var indentString = "";
			if (index == 0 && firstColumnIndent > 0) {
				indentString += new string('-', firstColumnIndent) + " ";
			}
			var width = Math.Abs(column.Width);
			var cellText = TruncateRight(indentString + column.GetText(stats), width);
			if (column.Width < 0) {
				return cellText.PadRight(width);
			}
			else {
				return cellText.PadLeft(width);
			}
		}));
	}
	static string TruncateRight(string value, int maxLength) {
		if (value.Length <= maxLength) {
			return value;
		}
		return value[..Math.Min(value.Length, maxLength)];
	}
	record ScopeStatsBuilder(string Name) {
		readonly object locker = new();
		readonly double totalStartTimestamp = GetTimestamp();
		readonly StatsAccumulator totalDurations = new();
		long totalNumberOfErrors;
		double perSecondStartTimestamp = GetTimestamp();
		readonly StatsAccumulator perSecondDurations = new();
		long perSecondNumberOfErrors;
		public void AddCompleted(double startTimestamp, double endTimestamp, Exception? exception) {
			lock (locker) {
				var duration = endTimestamp - startTimestamp;
				var numberOfErrors = exception == null ? 0 : 1;
				totalDurations.Add(duration);
				totalNumberOfErrors += numberOfErrors;
				perSecondDurations.Add(duration);
				perSecondNumberOfErrors += numberOfErrors;
			}
		}
		public ScopeStats GetStats() {
			lock (locker) {
				var currentTimestamp = GetTimestamp();
				var totalDuration = currentTimestamp - totalStartTimestamp;
				var perSecondDuration = currentTimestamp - perSecondStartTimestamp;
				var stats = new ScopeStats {
					Name = Name,
					TotalCount = totalDurations.Count,
					TotalCountPerSecond = totalDurations.Count / totalDuration,
					MinDuration = totalDurations.Min,
					AvgDuration = totalDurations.Avg,
					MaxDuration = totalDurations.Max,
					TotalDuration = totalDurations.Sum,
					NumberOfErrors = totalNumberOfErrors,
					TasksPerSecond = perSecondDurations.Count / perSecondDuration,
					ErrorsPerSecond = perSecondNumberOfErrors / perSecondDuration,
				};
				if (perSecondDurations.Count >= 1) {
					ResetPerSecondStats(currentTimestamp);
				}
				return stats;
			}
		}
		void ResetPerSecondStats(double currentTimestamp) {
			perSecondStartTimestamp = currentTimestamp;
			perSecondDurations.Reset();
			perSecondNumberOfErrors = 0;
		}
	}
	public record ScopeStats {
		public string Name { get; init; } = "";
		public long TotalCount { get; init; }
		public double TotalCountPerSecond { get; init; }
		public double TotalDuration { get; init; }
		public double MinDuration { get; init; }
		public double AvgDuration { get; init; }
		public double MaxDuration { get; init; }
		public long NumberOfErrors { get; init; }
		public double TasksPerSecond { get; init; }
		public double ErrorsPerSecond { get; init; }
	}
}

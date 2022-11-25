using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace TestHelpers;
public class ParallelRunner {
	public record Options {
		public TimeSpan? MaxTotalDuration { get; init; }
		public int MaxDegreeOfParallelism { get; init; } = -1;
		public bool PrintExceptions { get; init; }
		public CancellationToken CancellationToken { get; init; }
	}
	public static async Task Run(Options options, Func<Task> action) {
		await Run(options, async (callId, ct) => {
			await action();
		});
	}
	public static async Task Run(Options options, Func<long, Task> action) {
		await Run(options, async (callId, ct) => {
			await action(callId);
		});
	}
	public static async Task Run(Options options, Func<CancellationToken, Task> action) {
		await Run(options, async (callId, ct) => {
			await action(ct);
		});
	}
	public static async Task Run(Options options, Func<long, CancellationToken, Task> action) {
		var timeoutCts = new CancellationTokenSource(options.MaxTotalDuration ?? TimeSpan.FromMilliseconds(-1));
		var cts = CancellationTokenSource.CreateLinkedTokenSource(options.CancellationToken, timeoutCts.Token);
		var parallelOptions = new ParallelOptions() {
			MaxDegreeOfParallelism = options.MaxDegreeOfParallelism,
			CancellationToken = cts.Token,
		};
		bool CaptureException(Exception e) {
			if (options.PrintExceptions) {
				Console.Error.Write(e);
			}
			return false;
		}
		try {
			await Parallel.ForEachAsync(MakeInfiniteSequence(), parallelOptions, async (callId, ct) => {
				try {
					await action(callId, ct);
				}
				catch (Exception e) when (e is not TaskCanceledException && CaptureException(e)) {
				}
			});
		}
		catch (Exception e) when (e is TaskCanceledException) {
		}
	}
	static IEnumerable<long> MakeInfiniteSequence() {
		var i = 0;
		while (true) {
			yield return i;
			i++;
		}
	}
}

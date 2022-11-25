using CommonHelpers;
using System;
using System.Threading.Tasks;
using TestHelpers;
namespace AppTestScenarios;
public static class ParallelRunnerBenchmarks {
	public static async Task Run() {
		var tracer = new Tracer();
		var statsTask = tracer.PrintStatsContinuously(1000);
		var mainTask = ParallelRunner.Run(new() {
			MaxTotalDuration = TimeSpan.FromSeconds(10),
			MaxDegreeOfParallelism = -1,
			PrintExceptions = false,
		}, async () => {
			await tracer.RunAction("do nothing", async () => {
				await Task.CompletedTask;
			});
		});
		await mainTask;
	}
}

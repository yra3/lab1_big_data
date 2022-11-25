using AppTestScenarios;
using CommonHelpers;
using System.Threading.Tasks;
using TestHelpers;
namespace AppEntry;
class Program {
	public static async Task Main() {
		await Task.CompletedTask;
		if (!true) { Db.EnableTracing(); }
		var db = new Db("Host=127.0.0.2;Port=5433;Database=db1;Search Path=public;Username=postgres;Password=qwe123");
		if (!true) { await DbExamples.RunAll(db); }
		if (!true) { await ParallelRunnerBenchmarks.Run(); }
		if (true) { await DbTools.CleanAndAddRandomData(db, new RandomDataGenerator(123)); };
	}
}

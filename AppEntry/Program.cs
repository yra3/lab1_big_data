using AppTestScenarios;
using CommonHelpers;
using System;
using System.Globalization;
using System.Threading.Tasks;
using TestHelpers;
namespace AppEntry;
class Program {
	public static async Task Main() {
		CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
		CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
		Console.OutputEncoding = System.Text.Encoding.UTF8;
		if (true) { Console.WriteLine("Проверка Unicode: 👍"); }
		if (!true) { Db.EnableTracing(); }
		var db = new Db("Host=127.0.0.2;Port=5433;Database=db1;Search Path=public;Username=postgres;Password=qwe123");
		if (true) { await DbExamples.RunAll(db); }
		if (!true) { await ParallelRunnerBenchmarks.Run(); }
		if (!true) { await AppDbTools.TruncateAllTables(db); }
		if (!true) { await AppDbTools.AddRandomData(db, new RandomDataGenerator(123)); }
		if (true) { await AppScenarios.CheckGetArticleList(db); };
		if (true) { await AppScenarios.CheckAddNewArticle(db); };
	}
}

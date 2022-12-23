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
		var db = new Db("Host=10.0.2.15;Port=11004;Database=postgres;Search Path=public;Username=postgres;Password=qwe123");
		if (!true) { await DbExamples.RunAll(db); }
		if (!true) { await ParallelRunnerBenchmarks.Run(); }
		if (!true) { await AppDbTools.TruncateAllTables(db); }
		if (!true) { await AppDbTools.AddRandomData(db, new RandomDataGenerator(123)); }
		if (!true) { await AppScenarios.CheckGetArticleList(db); };
		if (!true) { await AppScenarios.CheckAddNewArticle(db); };
		if (true) { await AppScenarios.CheckSetCommentVotes(db); };
		if (true) { await AppScenarios.CheckCompanySubscription(db); };
		if (true) { await AppScenarios.CheckUsersKarma(db); };
	}
}

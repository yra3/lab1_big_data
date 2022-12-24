using CommonHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestHelpers;
namespace AppTestScenarios;
public static class AppDbTools {
	public static async Task TruncateAllTables(Db db) {
		await using var cursor = await db.Connect(readWrite: true);
		var tableNames = await GetTableNames(cursor);
		foreach (var tableName in tableNames) {
			await TruncateTable(cursor, tableName);
		}
		await cursor.Commit();
	}
	public static async Task<IReadOnlyList<string>> GetTableNames(DbCursor cursor) {
		return await cursor.QueryList(
			row => row.GetString("table_name"),
			@"
SELECT
	table_name
FROM
	information_schema.tables
WHERE
	table_schema = 'public'
ORDER BY
	table_name
");
	}
	public static async Task TruncateTable(DbCursor cursor, string tableName) {
		await cursor.Execute($"TRUNCATE TABLE {SqlDmlHelper.QuoteName(tableName)} CASCADE");
	}
	public static async Task AddRandomData(Db db, RandomDataGenerator rg) {
		await using var cursor = await db.Connect(readWrite: true);
		var currentUserId = Guid.Parse("e65e7d2b-49d7-8268-7dc2-bc09022f4127");
		var targetUserId = Guid.Parse("e65e7d2b-49d7-8268-7dc2-bc09022f4129");
		var _articleId = Guid.Parse("3a30ea1c-61dd-2cfc-8930-48048f26e4bb");
		var companies = Enumerable.Range(0, 1000).Select(_ => {
			return new QueryParameters() {
				{ "id", rg.NextGuid() },
				{ "name", rg.NextName() },
				{ "handle", rg.NextHandle() },
			};
		}).ToList();
		await InsertMultiple(cursor, "company", companies);
		var users = Enumerable.Range(0, 1000).Select(_ => {
			return new QueryParameters() {
				{ "id", rg.NextGuid() },
				{ "full_name", rg.NextName() },
				{ "handle", rg.NextHandle() },
			};
		}).ToList();
		users[0] = new QueryParameters() {
				{ "id", currentUserId },
				{ "full_name", rg.NextName() },
				{ "handle", rg.NextHandle() },
			};
		users[1] = new QueryParameters() {
				{ "id", targetUserId },
				{ "full_name", rg.NextName() },
				{ "handle", rg.NextHandle() },
			};
		await InsertMultiple(cursor, "app_user", users);
		var hubs = Enumerable.Range(0, 1000).Select(_ => {
			return new QueryParameters() {
				{ "id", rg.NextGuid() },
				{ "name", rg.NextName() },
				{ "handle", rg.NextHandle() },
			};
		}).ToList();
		await InsertMultiple(cursor, "hub", hubs);
		var articles = Enumerable.Range(0, 1000).Select(_ => {
			return new QueryParameters() {
				{ "id", rg.NextGuid() },
				{ "title", rg.NextSentence(averageMinimumWordCount: 10) },
				{ "content_text", rg.NextText(averageMinimumLength: 5000, averageMinimumWordCountInSentence: 10) },
				{ "author_user_id", rg.NextArrayElement(users).GetValue("id") },
				{ "company_id", rg.NextBool(0.5) ? rg.NextArrayElement(companies).GetValue("id") : null },
				{ "is_published", rg.NextBool(0.8) },
				{ "publication_time", rg.NextUtcDateTime() },
				{ "view_count", rg.Random.NextInt64(1000000) },
			};
		}).ToList();
		articles[0] = new QueryParameters() {
				{ "id", _articleId },
				{ "title", rg.NextSentence(averageMinimumWordCount: 10) },
				{ "content_text", rg.NextText(averageMinimumLength: 5000, averageMinimumWordCountInSentence: 10) },
				{ "author_user_id", targetUserId },
				{ "company_id", rg.NextBool(0.5) ? rg.NextArrayElement(companies).GetValue("id") : null },
				{ "is_published", rg.NextBool(0.8) },
				{ "publication_time", rg.NextUtcDateTime() },
				{ "view_count", rg.Random.NextInt64(1000000) },
			};
		await InsertMultiple(cursor, "article", articles);
		await InsertLinks(cursor, rg, "article_hub_link", articles, "article_id", 0, 5, hubs, "hub_id");
		await InsertLinks(cursor, rg, "user_company_link", users, "user_id", 0, 2, companies, "company_id");
		var polls = new List<QueryParameters>();
		var pollVariants = new List<QueryParameters>();
		foreach (var article in rg.NextArrayElements(articles, 0.1)) {
			foreach (var pollPos in Enumerable.Range(0, rg.Random.Next(1, 5))) {
				var poll = new QueryParameters() {
					{ "id", rg.NextGuid() },
					{ "article_id", article.GetValue("id") },
					{ "pos", pollPos },
					{ "title", rg.NextSentence(averageMinimumWordCount: 10) },
					{ "multiple", rg.NextBool(0.7) }
				};
				polls.Add(poll);
				foreach (var variantPos in Enumerable.Range(0, rg.Random.Next(2, 10))) {
					var variant = new QueryParameters() {
						{ "id", rg.NextGuid() },
						{ "poll_id", poll.GetValue("id") },
						{ "pos", variantPos },
						{ "title", rg.NextSentence(averageMinimumWordCount: 10) },
					};
					pollVariants.Add(variant);
				}
			}
		}
		await InsertMultiple(cursor, "poll", polls);
		await InsertMultiple(cursor, "poll_variant", pollVariants);
		var comments = new List<QueryParameters>();
		foreach (var article in rg.NextArrayElements(articles, 0.9)) {
			var commentCount = rg.Random.Next(100);
			var parentCommentIds = new List<Guid?>() { null };
			for (var i = 0; i < commentCount; i++) {
				var parentCommentId = rg.NextArrayElement(parentCommentIds);
				var commentId = rg.NextGuid();
				var comment = new QueryParameters() {
					{ "id", commentId },
					{ "article_id", article.GetValue("id") },
					{ "user_id", rg.NextArrayElement(users).GetValue("id") },
					{ "parent_comment_id", parentCommentId },
					{ "publication_time", rg.NextUtcDateTime() },
					{ "content", rg.NextText(averageMinimumLength: 300, averageMinimumWordCountInSentence: 10) },
				};
				comments.Add(comment);
				parentCommentIds.Add(commentId);
			}
		}
		await InsertMultiple(cursor, "article_comment", comments);
		var notes = new List<QueryParameters>();
		foreach (var user in rg.NextArrayElements(users, 0.7)) {
			var notesCount = rg.Random.Next(1, 3);
			var targetUserGuid = user.GetValue("id");
			for (var noteNum = 0; noteNum < notesCount; noteNum++) {
				var authorUserGuid = rg.NextArrayElement(users).GetValue("id");
				var note = new QueryParameters() {
					{ "target_user_id", targetUserGuid },
					{ "user_id", authorUserGuid },
					{ "content_text", rg.NextText(50) },
					{ "modification_time", rg.NextUtcDateTime()}
				};
				notes.Add(note);
			}
		}
		await InsertMultiple(cursor, "note_about_user", notes);
		var karmas = Enumerable.Range(0, 50).Select(_ => {
			return new QueryParameters() {
				{ "target_user_id", rg.NextArrayElement(users).GetValue("id") },
				{ "user_id", rg.NextArrayElement(users).GetValue("id") },
				{ "is_upvote", rg.NextBool(0.8) },
				{ "creation_time", rg.NextUtcDateTime() },
			};
		}).ToList();
		karmas[0] = new QueryParameters() {
				{ "target_user_id", targetUserId },
				{ "user_id", currentUserId },
				{ "is_upvote", rg.NextBool(0.8) },
				{ "creation_time", rg.NextUtcDateTime() },
			};
		await InsertMultiple(cursor, "karma_vote", karmas);
		await cursor.Commit();
	}
	public static async Task<int> InsertMultiple(DbCursor cursor, string tableName, IEnumerable<QueryParameters> multipleParameters, string conflictNames = "") {
		Logger.Log($"Inserting into {tableName.ToLiteral()}");
		var totalCount = 0;
		var totalAffected = 0;
		foreach (var parameters in multipleParameters) {
			totalCount += 1;
			var sql = SqlDmlHelper.MakeInsert(tableName, parameters.Names);
			if (conflictNames != "") {
				sql += $"ON CONFLICT ({conflictNames}) DO NOTHING";
			}
			var affected = await cursor.Execute(sql, parameters);
			if (affected != -1) {
				totalAffected += affected;
			}
		}
		Logger.Log($"  Inserted {totalAffected}/{totalCount} rows into {tableName.ToLiteral()}");
		return totalAffected;
	}
	static async Task<IReadOnlyList<QueryParameters>> InsertLinks(
		DbCursor cursor,
		RandomDataGenerator rg,
		string tableName,
		IReadOnlyList<QueryParameters> rows1,
		string key1,
		int minCount1,
		int maxCount1,
		IReadOnlyList<QueryParameters> rows2,
		string key2
	) {
		var links = new List<QueryParameters>();
		foreach (var row1 in rows1) {
			var averageCount = rg.Random.Next(minCount1, maxCount1 - 1);
			for (var i = 0; i < averageCount; i++) {
				var link = new QueryParameters {
					{ key1, row1.GetValue("id") },
					{ key2, rg.NextArrayElement(rows2).GetValue("id") },
				};
				links.Add(link);
			}
		}
		await InsertMultiple(cursor, tableName, links, $"{key1}, {key2}");
		return links;
	}
}

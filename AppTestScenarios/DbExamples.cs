using CommonHelpers;
using System;
using System.Threading.Tasks;
namespace AppTestScenarios;
public class DbExamples {
	public static async Task RunAll(Db db) {
		await RunQueryFirstExample(db);
		await RunQueryListExample(db);
		await RunQueryAsyncEnumerableExample(db);
	}
	public static async Task RunQueryFirstExample(Db db, bool print = true) {
		await using var cursor = await db.Connect();
		var result = await cursor.QueryFirst(
			row => row == null ? throw new Exception() : new {
				BoolValue = row.GetBool("bool_value"),
				IntValue = row.GetInt("int_value"),
				LongValue = row.GetLong("long_value"),
				DoubleValue = row.GetDouble("double_value"),
				StringValue = row.GetString("string_value"),
				GuidValue = row.GetGuid("guid_value"),
				DateTimeValue = row.GetUtcDateTime("date_time_value"),
				TimeSpanValue = row.GetTimeSpan("time_span_value"),
				ByteArrayValue = row.GetByteArray("byte_array_value"),
			},
			@"
SELECT
	FALSE OR :bool_param AS bool_value,
	1 + :int_param AS int_value,
	1 + :long_param AS long_value,
	1.2 + :double_param AS double_value,
	'qwe' || :string_param || :guid_param AS string_value,
	uuid_generate_v4() AS guid_value,
	CURRENT_TIMESTAMP + :time_span_param AS date_time_value,
	:date_time_param - CURRENT_TIMESTAMP AS time_span_value,
	'\x0001'::bytea || :byte_array_param AS byte_array_value
",
			new() {
				{ "bool_param", true },
				{ "int_param", 2 },
				{ "long_param", 2L },
				{ "double_param", 2.3 },
				{ "string_param", "asd" },
				{ "guid_param", Guid.NewGuid() },
				{ "date_time_param", DateTime.UtcNow },
				{ "time_span_param", TimeSpan.FromMinutes(1) },
				{ "byte_array_param", new byte[] { 0xFE, 0xFF } },
			}
		);
		if (print) {
			Logger.Log($"Result: {result} {Convert.ToHexString(result.ByteArrayValue)}");
		}
	}
	static async Task RunQueryListExample(Db db) {
		await using var cursor = await db.Connect();
		var result = await cursor.QueryList(
			row => new {
				Id = row.GetInt("id"),
				Name = row.GetString("name"),
			},
			@"
SELECT
	s.id,
	md5(random() :: TEXT) AS name
FROM
	generate_series(1, 1000000) s(id)
");
		Logger.Log($"Result: {result.Count}");
	}
	static async Task RunQueryAsyncEnumerableExample(Db db) {
		await using var cursor = await db.Connect();
		var rows = cursor.QueryAsyncEnumerable(
			row => new {
				Id = row.GetInt("id"),
				Name = row.GetString("name"),
			},
			@"
SELECT
	s.id,
	md5(random() :: TEXT) AS name
FROM
	generate_series(1, 1000000) s(id)
");
		var idSum = 0;
		await foreach (var row in rows) {
			if (false) {
				Logger.Log(row);
			}
			idSum += row.Id;
		}
		Logger.Log($"{new { idSum }}");
	}
}

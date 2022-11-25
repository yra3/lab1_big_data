using Npgsql;
using Npgsql.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace CommonHelpers;
public record Db {
	public static void EnableTracing() {
		NpgsqlLogManager.Provider = new ConsoleLoggingProvider(NpgsqlLogLevel.Trace, true, true);
	}
	public Db(string connectionString) {
		ConnectionString = connectionString;
	}
	const string DefaultConnectionString = "Include Error Detail=true;Timeout=1024";
	string connectionString = "";
	public string ConnectionString {
		get => connectionString;
		init {
			connectionString = value;
			realConnectionString = new NpgsqlConnectionStringBuilder(
				$"{DefaultConnectionString};{connectionString}"
			).ConnectionString;
		}
	}
	string realConnectionString = DefaultConnectionString;
	public async Task<DbCursor> Connect(bool serializable = true, bool readWrite = false, CancellationToken ct = default) {
		return await ConnectCustom(async connection => {
			var attempts = 10;
			while (attempts > 0) {
				attempts -= 1;
				try {
					await connection.OpenAsync(ct);
					break;
				}
				catch (NpgsqlException e) when (attempts > 0 && e.SqlState == PostgresErrorCodes.TooManyConnections) {
					await Task.Delay(100, ct);
					continue;
				}
			}
			await BeginTransaction(
				connection: connection,
				serializable: serializable,
				readWrite: readWrite,
				ct: ct
			);
		});
	}
	public async Task<DbCursor> ConnectCustom(Func<NpgsqlConnection, Task> setupConnection) {
		var connection = new NpgsqlConnection(realConnectionString);
		await setupConnection(connection);
		return new DbCursor(connection);
	}
	public static async Task BeginTransaction(NpgsqlConnection connection, bool serializable, bool readWrite, CancellationToken ct = default) {
		await using var command = connection.CreateCommand();
		command.CommandText = string.Join(" ", new[] {
			"BEGIN",
			$"ISOLATION LEVEL {(serializable ? "SERIALIZABLE" : "READ COMMITTED")}",
			readWrite ? "READ WRITE": "READ ONLY",
		});
		await command.ExecuteNonQueryAsync(ct);
	}
	public static async Task CommitTransaction(NpgsqlConnection connection, CancellationToken ct = default) {
		await using var command = connection.CreateCommand();
		command.CommandText = "COMMIT";
		await command.ExecuteNonQueryAsync(ct);
	}
}

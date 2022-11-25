using System.Collections.Generic;
using System.Linq;
namespace CommonHelpers;
public static class SqlDmlHelper {
	const string Indent = "\t\t";
	public static string MakeInsert(string tableName, IEnumerable<string> parameterNames) {
		return @$"
INSERT INTO
	{tableName} (
{string.Join(",\n", parameterNames.Select(name => $"{Indent}{name}"))}
	)
VALUES
	(
{string.Join(",\n", parameterNames.Select(name => $"{Indent}:{name}"))}
	)
".Trim();
	}
	public static string MakeUpdateAssignments(IEnumerable<string> parameterNames) {
		return string.Join(",\n", parameterNames.Select(name => $"{Indent}{name} = :{name}"));
	}
	public static string QuoteName(string name) {
		return '\"' + name.Replace("\"", "\"\"") + '\"';
	}
}

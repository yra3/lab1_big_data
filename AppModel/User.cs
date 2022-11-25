using CommonHelpers;
using System;
namespace AppModel;
public record User {
	public static readonly User Empty = new();
	public Guid Id { get; init; }
	public string FullName { get; init; } = "";
	public string Handle { get; init; } = "";
	public static User FromIdFullNameHandleSequentialRow(SequentialDbRow row) {
		return new User {
			Id = row.ReadGuid(),
			FullName = row.ReadString(),
			Handle = row.ReadString(),
		};
	}
}

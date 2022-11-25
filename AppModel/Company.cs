using CommonHelpers;
using System;
namespace AppModel;
public record Company {
	public Guid Id { get; init; }
	public string Name { get; init; } = "";
	public string Handle { get; init; } = "";
	public static Company FromIdNameHandleSequentialRow(SequentialDbRow row) {
		return new Company {
			Id = row.ReadGuid(),
			Name = row.ReadString(),
			Handle = row.ReadString(),
		};
	}
}

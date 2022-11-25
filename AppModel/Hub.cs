using CommonHelpers;
using System;
namespace AppModel;
public record Hub {
	public Guid Id { get; init; }
	public string Name { get; init; } = "";
	public string Handle { get; init; } = "";
	public static Hub FromIdNameHandleSequentialRow(SequentialDbRow row) {
		return new Hub {
			Id = row.ReadGuid(),
			Name = row.ReadString(),
			Handle = row.ReadString(),
		};
	}
}

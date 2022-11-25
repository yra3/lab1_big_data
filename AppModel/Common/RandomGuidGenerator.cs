using System;
namespace AppModel.Common;
public class RandomGuidGenerator : IGuidGenerator {
	public static readonly RandomGuidGenerator Instance = new();
	public Guid GetNextGuid() {
		return Guid.NewGuid();
	}
}

using AppModel.Common;
using CommonHelpers;
using System;
namespace AppModel;
public partial record AppClient(Db db) {
	public IClock Clock { get; init; } = ActualClock.Instance;
	DateTime GetCurrentTime() {
		return Clock.GetCurrentTime();
	}
	public IGuidGenerator GuidGenerator { get; init; } = RandomGuidGenerator.Instance;
	Guid GetNextGuid() {
		return GuidGenerator.GetNextGuid();
	}
	public Guid? CurrentUserId { get; init; }
}

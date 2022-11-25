using System;
namespace AppModel.Common;
public class ActualClock : IClock {
	public static readonly ActualClock Instance = new();
	public DateTime GetCurrentTime() {
		return DateTime.UtcNow;
	}
}

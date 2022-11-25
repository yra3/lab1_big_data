using System;
namespace AppModel.Common;
public interface IGuidGenerator {
	Guid GetNextGuid();
}

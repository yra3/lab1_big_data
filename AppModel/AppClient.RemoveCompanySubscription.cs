using System;
using System.Threading.Tasks;
namespace AppModel;
public partial record AppClient {
	public async Task<bool> RemoveCompanySubscription(Guid companyId) {
		await using var cursor = await db.Connect(readWrite: true);
		var changed = await cursor.Execute(
			@"
DELETE FROM company_subscription
WHERE company_id = :company_id AND user_id = :user_id
",
			new() {
				{ "company_id", companyId },
				{ "user_id", CurrentUserId },
			}
		);
		if (changed != 1) {
			return false;
		}
		await cursor.Commit();
		return true;
	}
}

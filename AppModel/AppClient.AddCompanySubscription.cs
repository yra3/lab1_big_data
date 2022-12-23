using System;
using System.Threading.Tasks;
namespace AppModel;
public partial record AppClient {
	public async Task<bool> AddCompanySubscription(Guid companyId) {
		await using var cursor = await db.Connect(readWrite: true);
		var affected = await cursor.Execute(
			@"
INSERT INTO
	company_subscription (
		company_id,
		user_id
	)
SELECT
	:company_id,
	:user_id
WHERE
	EXISTS (
		SELECT
			1
		FROM
			company c
		WHERE
			c.id = :company_id
	)
	AND
	EXISTS (
		SELECT
			1
		FROM
			app_user au
		WHERE
			au.id = :user_id
	)
	AND
	NOT EXISTS (
		SELECT
			1
		FROM company_subscription cs
		WHERE cs.user_id = :user_id AND cs.company_id = :company_id
	)
",
			new() {
				{ "company_id", companyId },
				{ "user_id", CurrentUserId },
			}
		);
		if (affected != 1) {
			return false;
		}
		await cursor.Commit();
		return true;
	}
}

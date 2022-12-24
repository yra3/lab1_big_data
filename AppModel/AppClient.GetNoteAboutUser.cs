using System;
using System.Threading.Tasks;
namespace AppModel;
public partial record AppClient {
	public async Task<string> GetNoteAboutUser(Guid targetUserId) {
		await using var cursor = await db.Connect();
		var result = await cursor.QueryFirst(
			row => row?.GetString("content_text"),
			@"
SELECT
	nau.content_text
FROM
	note_about_user nau
WHERE
	nau.target_user_id = :target_user_id
	AND nau.user_id = :user_id
			",
			new() {
				{ "target_user_id", targetUserId },
				{ "user_id", CurrentUserId }
			}
		);
		return result ?? "";
	}
}

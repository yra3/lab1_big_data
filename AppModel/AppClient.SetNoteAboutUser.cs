using System;
using System.Threading.Tasks;
namespace AppModel;
public partial record AppClient {
	public async Task<bool> SetNoteAboutUser(Guid targetUserId, string contentText) {
		await using var cursor = await db.Connect(readWrite: true);
		var changed = 0;
		if (contentText == "") {
			changed = await cursor.Execute(
				@"
DELETE FROM
	note_about_user
WHERE
	target_user_id = :target_user_id
	AND user_id = :user_id
				",
				new() {
					{ "target_user_id", targetUserId },
					{ "user_id", CurrentUserId }
				}
			);
		}
		else {
			changed = await cursor.Execute(
				@"
INSERT INTO
	note_about_user (
		target_user_id,
		user_id,
		content_text,
		modification_time
	)
SELECT
	:target_user_id,
	:user_id,
	:content_text,
	:modification_time
WHERE
	EXISTS (
		SELECT
			1
		FROM
			app_user
		WHERE
			id = :target_user_id
	)
	AND EXISTS (
		SELECT
			1
		FROM
			app_user
		WHERE
			id = :user_id
	) ON CONFLICT (target_user_id, user_id) DO
UPDATE
SET
	content_text = excluded.content_text,
	modification_time = excluded.modification_time
				",
				new() {
					{ "target_user_id", targetUserId },
					{ "user_id", CurrentUserId },
					{ "content_text", contentText },
					{ "modification_time", GetCurrentTime() }
				}
			);
		}
		await cursor.Commit();
		return changed != 0;
	}
}

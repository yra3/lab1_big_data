using System.Collections.Generic;
using System.Threading.Tasks;
namespace AppModel;
public partial record AppClient {
	public async Task<IReadOnlyList<NoteAboutUser>> GetNotesAboutUser() {
		await using var cursor = await db.Connect();
		return await cursor.QueryList(
			row => new NoteAboutUser {
				TargetUser = row.GetTuple("target_user", User.FromIdFullNameHandleSequentialRow),
				Content = row.GetString("content_text"),
				ModificationTime = row.GetUtcDateTime("modification_time")
			},
			@"
SELECT
	(
		SELECT
			(au.id, au.full_name, au.handle)
		FROM
			app_user au
		WHERE
			au.id = nau.target_user_id
	) AS target_user,
	nau.content_text,
	nau.modification_time
FROM
	note_about_user nau
WHERE
	nau.user_id = :user_id
ORDER BY
	nau.modification_time
			",
			new() {
				{ "user_id", CurrentUserId },
			}
		);
	}
}

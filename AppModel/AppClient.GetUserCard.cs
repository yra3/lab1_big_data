using System;
using System.Threading.Tasks;
namespace AppModel;
public partial record AppClient {
	public async Task<UserCard?> GetUserCard(Guid userId) {
		await using var cursor = await db.Connect();
		return await cursor.QueryFirst(
			row => row == null ? null : new UserCard {
				Id = row.GetGuid("id"),
				FullName = row.GetString("full_name"),
				Handle = row.GetString("handle"),
				ArticleCount = row.GetInt("article_count"),
				CommentCount = row.GetInt("comment_count"),
			},
			@"
SELECT
	au.id,
	au.full_name,
	au.handle,
	(
		SELECT
			CAST(count(*) AS int)
		FROM
			article a
		WHERE
			a.is_published
			AND a.author_user_id = au.id
	) AS article_count,
	(
		SELECT
			CAST(count(*) AS int)
		FROM
			article_comment ac
			JOIN article a ON a.id = ac.article_id
		WHERE
			a.is_published
			AND ac.user_id = au.id
	) AS comment_count
FROM
	app_user au
WHERE
	au.id = :user_id
",
			new() {
				{ "user_id", userId },
			}
		);
	}
}

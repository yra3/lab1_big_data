using System;
using System.Threading.Tasks;
namespace AppModel;
public partial record AppClient {
	public async Task<bool> SetArticleIsPublished(Guid articleId, bool isPublished) {
		await using var cursor = await db.Connect(readWrite: true);
		var changed = await cursor.Execute(
			@"
WITH user_karma AS (
	SELECT
		COALESCE(SUM(CASE WHEN kv.is_upvote THEN 1 ELSE -1 END), 0) AS karma
	FROM karma_vote kv
	WHERE
		kv.target_user_id = :current_user_id
)
UPDATE
	article a
SET
	is_published = :is_published,
	publication_time = CASE
		WHEN (
			:is_published
			AND NOT is_published
		) THEN :current_time
		ELSE a.publication_time
	END
FROM
	user_karma
WHERE
	a.id = :article_id
	AND a.author_user_id = :current_user_id
	AND (user_karma.karma >= 0 AND :is_published = true OR :is_published = false)
	AND is_published != :is_published
",
			new() {
				{ "article_id", articleId },
				{ "is_published", isPublished },
				{ "current_time", GetCurrentTime() },
				{ "current_user_id", CurrentUserId },
			}
		);
		if (changed != 1) {
			return false;
		}
		await cursor.Commit();
		return true;
	}
}

using System;
using System.Threading.Tasks;

namespace AppModel;
public partial record AppClient {
	public async Task<bool> SetCommentVote(Guid commentId, bool? isUpvote) {
		if (CurrentUserId == null) {
			return false;
		}
		await using var cursor = await db.Connect(readWrite: true);
		int changed;
		if (isUpvote == null) {
			changed = await cursor.Execute(
				@"
DELETE FROM
	comment_vote v
WHERE
	v.comment_id = :comment_id
	AND v.user_id = :current_user_id
	",
				new() {
					{ "comment_id", commentId },
					{ "current_user_id", CurrentUserId },
				}
			);
		}
		else {
			changed = await cursor.Execute(
				@"
INSERT INTO
	comment_vote (
		comment_id,
		user_id,
		is_upvote,
		creation_time
	)
	SELECT
		:comment_id,
		:current_user_id,
		:is_upvote,
		:creation_time
	WHERE EXISTS (
		SELECT 1 FROM app_user WHERE id = :current_user_id
	)
	AND EXISTS (
		SELECT 1 FROM article_comment WHERE id = :comment_id
	)
ON CONFLICT (
	comment_id,
	user_id
)
DO UPDATE SET
	is_upvote = :is_upvote,
	creation_time = :creation_time
	where :is_upvote != comment_vote.is_upvote
	AND comment_vote.comment_id = :comment_id
	AND comment_vote.user_id = :current_user_id
	",
				new() {
					{ "comment_id", commentId },
					{ "current_user_id", CurrentUserId },
					{ "is_upvote", isUpvote },
					{ "creation_time", GetCurrentTime() },
				}
			);
		}
		if (changed != 1) {
			return false;
		}
		await cursor.Commit();
		return true;
	}
}

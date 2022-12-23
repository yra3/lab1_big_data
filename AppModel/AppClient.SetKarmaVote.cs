using System;
using System.Threading.Tasks;
namespace AppModel;
public partial record AppClient {
	public async Task<bool> SetKarmaVote(Guid targetUserId, bool? isUpvote) {
		await using var cursor = await db.Connect(readWrite: true);
		if (CurrentUserId == null) {
			return false;
		}
		if (isUpvote == null) {
			var changed = await cursor.Execute(
							@"
DELETE FROM karma_vote
WHERE target_user_id = :target_user_id AND
user_id = :user_id
",
			new() {
				{ "target_user_id", targetUserId },
				{ "user_id", CurrentUserId },
				}
			);
			if (changed != 1) {
				return false;
			}
			await cursor.Commit();
			return true;
		}
		else {
			var changed = await cursor.Execute(
				@"
INSERT
	INTO
	karma_vote (
		target_user_id,
		user_id,
		is_upvote,
		creation_time
	)
		SELECT
			:target_user_id,
			:user_id,
			:is_upvote,
			:creation_time
		WHERE EXISTS (
			SELECT 1 FROM app_user WHERE id = :target_user_id
		)
		AND EXISTS (
			SELECT 1 FROM app_user WHERE id = :user_id
		)
ON
CONFLICT (
	target_user_id,
	user_id
)
DO UPDATE SET
	is_upvote = :is_upvote,
	creation_time = :creation_time
	WHERE :is_upvote != karma_vote.is_upvote
	AND karma_vote.target_user_id = :target_user_id
	AND karma_vote.user_id = :user_id;
",
			new() {
				{ "target_user_id", targetUserId },
				{ "user_id", CurrentUserId },
				{ "is_upvote", isUpvote },
				{ "creation_time", GetCurrentTime() },
				}
			);
			if (changed != 1) {
				return false;
			}
			await cursor.Commit();
			return true;
		}
	}
}

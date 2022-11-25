using CommonHelpers;
using System;
using System.Threading.Tasks;
namespace AppModel;
public partial record AppClient {
	public async Task<Guid?> AddNewArticle(NewArticle newArticle) {
		await using var cursor = await db.Connect(readWrite: true);
		var articleId = GetNextGuid();
		var affected = await cursor.Execute(
			@"
INSERT INTO
	article (
		id,
		title,
		content_text,
		author_user_id
	)
SELECT
	:article_id,
	:title,
	:content_text,
	:author_user_id
WHERE
	EXISTS (
		SELECT
			1
		FROM
			app_user au
		WHERE
			au.id = :author_user_id
	)
",
			new() {
				{ "article_id", articleId },
				{ "title", newArticle.Title },
				{ "content_text", newArticle.Content },
				{ "author_user_id", CurrentUserId },
			}
		);
		if (affected != 1) {
			return null;
		}
		foreach (var hubId in newArticle.HubIds) {
			await cursor.Insert("article_hub_link", new(){
				{ "article_id", articleId },
				{ "hub_id", hubId },
			});
		}
		foreach (var (pollIndex, poll) in newArticle.Polls.Enumerate()) {
			var pollId = GetNextGuid();
			await cursor.Insert("poll", new(){
				{ "id", pollId },
				{ "article_id", articleId },
				{ "pos", pollIndex },
				{ "title", poll.Title },
				{ "multiple", poll.Multiple },
			});
			foreach (var (pollVariantIndex, pollVariant) in poll.Variants.Enumerate()) {
				var pollVariantId = GetNextGuid();
				await cursor.Insert("poll_variant", new() {
					{ "id", pollVariantId },
					{ "poll_id", pollId },
					{ "pos", pollVariantIndex },
					{ "title", pollVariant.Title },
				});
			}
		}
		await cursor.Commit();
		return articleId;
	}
}

using CommonHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace AppModel;
public partial record AppClient {
	public async Task<Article?> GetArticle(Guid articleId) {
		await using var cursor = await db.Connect();
		return await GetArticle(cursor, articleId);
	}
	async Task<Article?> GetArticle(DbCursor cursor, Guid articleId) {
		var article = await GetArticleById(cursor, articleId);
		if (article == null) {
			return null;
		}
		return article with {
			Polls = await GetArticlePolls(cursor, articleId),
			Comments = await GetArticleComments(cursor, articleId)
		};
	}
	async Task<Article?> GetArticleById(DbCursor cursor, Guid articleId) {
		return await cursor.QueryFirst(
			row => row == null ? null : new Article {
				Id = row.GetGuid("id"),
				Title = row.GetString("title"),
				Content = row.GetString("content_text"),
				Author = row.GetTuple("author", User.FromIdFullNameHandleSequentialRow),
				Company = row.GetTupleOrNull("company", tupleRow => {
					return tupleRow == null ? null : Company.FromIdNameHandleSequentialRow(tupleRow);
				}),
				IsPublished = row.GetBool("is_published"),
				PublicationTime = row.GetUtcDateTime("publication_time"),
				ViewCount = row.GetLong("view_count"),
				Hubs = row.GetTupleArray("hubs", Hub.FromIdNameHandleSequentialRow),
			},
			@"
SELECT
	a.id,
	a.title,
	a.content_text,
	(
		SELECT
			(au.id, au.full_name, au.handle)
		FROM
			app_user au
		WHERE
			au.id = a.author_user_id
	) AS author,
	(
		SELECT
			(c.id, c.name, c.handle)
		FROM
			company c
		WHERE
			c.id = a.company_id
	) AS company,
	a.is_published,
	a.publication_time,
	a.view_count,
	(
		SELECT
			array_agg((t.id, t.name, t.handle))
		FROM
			(
				SELECT
					h.id,
					h.name,
					h.handle
				FROM
					hub h
				WHERE
					EXISTS(
						SELECT
							1
						FROM
							article_hub_link ahl
						WHERE
							a.id = ahl.article_id
							AND ahl.hub_id = h.id
					)
				ORDER BY
					h.name,
					h.id
			) t
	) AS hubs,
	(
		SELECT
			count(*)
		FROM
			article_comment ac
		WHERE
			ac.article_id = a.id
	) AS comment_count
FROM
	article a
WHERE
	a.id = :article_id
	AND (
		a.is_published
		OR a.author_user_id = :current_user_id
	)
",
			new() {
				{ "current_user_id", CurrentUserId },
				{ "article_id", articleId },
			}
		);
	}
	static async Task<IReadOnlyList<Article.Poll>> GetArticlePolls(DbCursor cursor, Guid articleId) {
		var polls = await cursor.QueryList(
			row => new Article.Poll {
				Id = row.GetGuid("id"),
				Title = row.GetString("title"),
				Multiple = row.GetBool("multiple"),
			},
			@"
SELECT
	p.id,
	p.title,
	p.multiple
FROM
	poll p
WHERE
	p.article_id = :article_id
ORDER BY
	p.pos,
	p.id
",
			new() { { "article_id", articleId } }
		);
		var rawPollVariants = await cursor.QueryList(
			row => new {
				PollId = row.GetGuid("poll_id"),
				Id = row.GetGuid("id"),
				Title = row.GetString("title"),
			},
			@"
SELECT
	pv.poll_id,
	pv.id,
	pv.title
FROM
	poll p
	JOIN poll_variant pv ON pv.poll_id = p.id
WHERE
	p.article_id = :article_id
ORDER BY
	pv.poll_id,
	pv.pos,
	pv.id
",
			new() { { "article_id", articleId } }
		);
		var pollVariantsByPollId = rawPollVariants.GroupBy(rpv => rpv.PollId).ToDictionary(
			group => group.Key,
			group => group.Select(rpv => new Article.Poll.Variant {
				Id = rpv.Id,
				Title = rpv.Title,
			}).ToReadOnlyList()
		);
		return polls.Select(poll => poll with {
			Variants = pollVariantsByPollId.GetValueOrDefault(poll.Id) ?? Array.Empty<Article.Poll.Variant>()
		}).ToList();
	}
	async Task<IReadOnlyList<Article.Comment>> GetArticleComments(DbCursor cursor, Guid articleId) {
		var articleComments = await cursor.QueryList(
			row => new {
				ParentCommentId = row.GetGuidOrNull("parent_comment_id"),
				RawComment = new Article.Comment {
					Id = row.GetGuid("id"),
					Author = row.GetTuple("author", User.FromIdFullNameHandleSequentialRow),
					PublicationTime = row.GetUtcDateTime("publication_time"),
					Content = row.GetString("content"),
					UpvoteCount = (int)row.GetLong("upvote_count"),
					DownvoteCount = (int)row.GetLong("downvote_count"),
					IsUpvoted = row.GetBoolOrNull("is_upvoted"),
				}
			},
			@"
SELECT
	ac.id,
	ac.parent_comment_id,
	(
		SELECT
			(au.id, au.full_name, au.handle)
		FROM
			app_user au
		WHERE
			au.id = ac.user_id
	) AS author,
	ac.publication_time,
	ac.content,
	(
		SELECT 
			count(cv.comment_id) 
		FROM 
			comment_vote cv 
		WHERE 
			cv.is_upvote 
			AND cv.comment_id = ac.id
	) AS upvote_count,
	(
		SELECT 
			count(cv.comment_id) 
		FROM 
			comment_vote cv 
		WHERE 
			not cv.is_upvote 
		AND 
			cv.comment_id = ac.id
	) AS downvote_count,
	(
		SELECT
			v.is_upvote AS is_upvoted
		FROM
			comment_vote v
		WHERE
			v.comment_id = ac.id
			AND v.user_id = :user_id
	) AS is_upvoted
FROM
	article_comment ac
WHERE
	ac.article_id = :article_id
",
			new() {
				{ "article_id", articleId },
				{ "user_id", CurrentUserId },
			}
		);
		var articleCommentsByParentId = articleComments
			.GroupBy(articleComment => articleComment.ParentCommentId.GetValueOrDefault())
			.ToDictionary(
				group => group.Key,
				group => group.ToReadOnlyList()
			);
		IReadOnlyList<Article.Comment> GetComments(Guid parentCommentId) {
			var chillren = articleCommentsByParentId.GetValueOrDefault(parentCommentId);
			if (chillren == null) {
				return Array.Empty<Article.Comment>();
			}
			return chillren.Select(child => child.RawComment with {
				Children = GetComments(child.RawComment.Id)
			}).ToList();
		}
		return GetComments(Guid.Empty);
	}
}

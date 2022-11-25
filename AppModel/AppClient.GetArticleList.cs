using CommonHelpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace AppModel;
public partial record AppClient {
	public record GetArticleListArgs {
		public Guid? HubId { get; init; }
		public Guid? CompanyId { get; init; }
		public int StartIndex { get; init; }
		public int MaxCount { get; init; } = 10;
	}
	public async Task<ArticleList> GetArticleList(GetArticleListArgs args) {
		await using var cursor = await db.Connect();
		return new ArticleList {
			Articles = await GetArticlePreviews(cursor, args),
			TotalCount = await GetArticlePreviewTotalCount(cursor),
		};
	}
	async Task<IReadOnlyList<ArticleList.Article>> GetArticlePreviews(DbCursor cursor, GetArticleListArgs args) {
		return await cursor.QueryList(
			row => new ArticleList.Article {
				Id = row.GetGuid("id"),
				Title = row.GetString("title"),
				ContentPreview = row.GetString("content_preview"),
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
	substring(a.content_text, 0, 100) AS content_preview,
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
	(
		a.is_published
		OR a.author_user_id = :current_user_id
	)
	AND (
		:company_id :: uuid IS NULL
		OR EXISTS(
			SELECT
				1
			FROM
				company c
			WHERE
				c.id = :company_id :: uuid
				AND a.company_id = c.id
		)
	)
	AND (
		:hub_id :: uuid IS NULL
		OR EXISTS(
			SELECT
				1
			FROM
				article_hub_link ahl
			WHERE
				a.id = ahl.article_id
				AND ahl.hub_id = :hub_id :: uuid
		)
	)
ORDER BY
	a.publication_time DESC,
	a.id OFFSET :result_offset
LIMIT
	:result_limit
",
			new() {
				{ "current_user_id", CurrentUserId },
				{ "hub_id", args.HubId },
				{ "company_id", args.CompanyId },
				{ "result_offset", args.StartIndex },
				{ "result_limit", args.MaxCount },
			}
		);
	}
	async Task<int> GetArticlePreviewTotalCount(DbCursor cursor) {
		return await cursor.QueryFirst(
			row => row?.GetInt("total_count") ?? throw new InvalidOperationException(),
			@"
SELECT
	CAST(count(*) AS int) AS total_count
FROM
	article a
WHERE
	a.is_published
	OR a.author_user_id = :current_user_id
",
			new() {
				{ "current_user_id", CurrentUserId },
			}
		);
	}
}

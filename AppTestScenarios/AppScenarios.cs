using AppModel;
using CommonHelpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using TestHelpers;
namespace AppTestScenarios;
public static class AppScenarios {
	public static async Task CheckGetArticleList(Db db) {
		var client = new AppClient(db) {
			CurrentUserId = Guid.Parse("d6bd5bde-7c57-3b81-3487-d1f1f2ebe27b"),
		};
		var articleList1 = await client.GetArticleList(new() {
			StartIndex = 0,
			MaxCount = 2,
		});
		Check.IsTrue(articleList1.Articles.Count == 2);
		var articleList2 = await client.GetArticleList(new() {
			StartIndex = 1,
			MaxCount = 3,
		});
		Check.IsTrue(articleList2.Articles.Count == 3);
		Check.IsTrue(articleList1.TotalCount == articleList2.TotalCount);
		Check.IsTrue(Serializer.ToJson(articleList1.Articles[1]) == Serializer.ToJson(articleList2.Articles[0]));
		var hubId = Guid.Parse("2f81e816-f32d-2caf-6bba-673c84915ee2");
		var articleListByHub = await client.GetArticleList(new() {
			HubId = hubId,
			StartIndex = 0,
			MaxCount = 2,
		});
		Check.IsTrue(articleListByHub.Articles.All(article => article.Hubs.Any(hub => hub.Id == hubId)));
		var companyId = Guid.Parse("22378388-6aad-7fcf-f142-d07f53135304");
		var articleListByCompany = await client.GetArticleList(new() {
			CompanyId = companyId,
			StartIndex = 0,
			MaxCount = 2,
		});
		Check.IsTrue(articleListByCompany.Articles.All(article => article.Company?.Id == companyId));
		Logger.Log($"{nameof(AppScenarios)}.{nameof(CheckGetArticleList)}: ok");
	}
	public static async Task CheckAddNewArticle(Db db) {
		var anonymousClient = new AppClient(db);
		var currentUserId = Guid.Parse("d6bd5bde-7c57-3b81-3487-d1f1f2ebe27b");
		var client = anonymousClient with {
			CurrentUserId = currentUserId,
		};
		var anotherClient = anonymousClient with {
			CurrentUserId = Guid.Parse("78661aad-03d9-819a-2145-84cd9b4d1e54"),
		};
		var userCard = await client.GetUserCard(currentUserId);
		Check.IsTrue(userCard != null);
		var userCardJson = Serializer.ToJson(userCard);
		foreach (var c in new[] { anonymousClient, anotherClient }) {
			Check.IsTrue(Serializer.ToJson(await anonymousClient.GetUserCard(currentUserId)) == userCardJson);
		}
		var newArticle = new NewArticle() {
			Title = "t1",
			Content = "content1",
			HubIds = new[]{
				Guid.Parse("2f81e816-f32d-2caf-6bba-673c84915ee2"),
				Guid.Parse("513fccaa-bd89-4c5f-1ef3-130293174cd2"),
			},
			Polls = Enumerable.Range(1, 2).Select(pollIndex => new NewArticle.Poll() {
				Title = $"poll {pollIndex} title",
				Multiple = pollIndex % 2 == 0,
				Variants = Enumerable.Range(1, 3).Select(variantIndex => new NewArticle.Poll.Variant {
					Title = $"poll {pollIndex} variant {variantIndex} title",
				}).ToList(),
			}).ToList()
		};
		Check.IsTrue(await anonymousClient.AddNewArticle(newArticle) == null);
		var maybeArticleId = await client.AddNewArticle(newArticle);
		Check.IsTrue(maybeArticleId != null);
		var articleId = maybeArticleId.Value;
		var article1 = await client.GetArticle(articleId);
		Check.IsTrue(article1 != null);
		foreach (var c in new[] { anonymousClient, anotherClient }) {
			Check.IsTrue(await c.GetArticle(articleId) == null);
			Check.IsTrue(!await c.SetArticleIsPublished(articleId, true));
			Check.IsTrue(!await c.SetArticleIsPublished(articleId, false));
			Check.IsTrue(Serializer.ToJson(await anonymousClient.GetUserCard(currentUserId)) == userCardJson);
		}
		Check.IsTrue(await client.SetArticleIsPublished(articleId, true));
		Check.IsTrue(await client.SetArticleIsPublished(articleId, true));
		foreach (var c in new[] { client, anonymousClient, anotherClient }) {
			Check.IsTrue(await c.GetArticle(articleId) != null);
			var newUserCard = await anonymousClient.GetUserCard(currentUserId);
			Check.IsTrue(Serializer.ToJson(newUserCard) == Serializer.ToJson(userCard with {
				ArticleCount = userCard.ArticleCount + 1,
			}));
		}
		foreach (var c in new[] { anonymousClient, anotherClient }) {
			Check.IsTrue(!await anonymousClient.SetArticleIsPublished(articleId, true));
			Check.IsTrue(!await anonymousClient.SetArticleIsPublished(articleId, false));
		}
		Check.IsTrue(await client.SetArticleIsPublished(articleId, false));
		Check.IsTrue(await client.SetArticleIsPublished(articleId, false));
		foreach (var c in new[] { anonymousClient, anotherClient }) {
			Check.IsTrue(await c.GetArticle(articleId) == null);
			Check.IsTrue(!await c.SetArticleIsPublished(articleId, true));
			Check.IsTrue(!await c.SetArticleIsPublished(articleId, false));
		}
		foreach (var c in new[] { client, anonymousClient, anonymousClient }) {
			Check.IsTrue(Serializer.ToJson(await anonymousClient.GetUserCard(currentUserId)) == userCardJson);
		}
		Logger.Log($"{nameof(AppScenarios)}.{nameof(CheckAddNewArticle)}: ok");
	}

	public static async Task CheckSetCommentVotes(Db db) {
		var client = new AppClient(db);
		var currentUserId = Guid.Parse("d6bd5bde-7c57-3b81-3487-d1f1f2ebe27b");
		client = client with {
			CurrentUserId = currentUserId,
		};
		Check.IsTrue(client != null);
		var articleWithCommentsId = Guid.Parse("5ae0355c-9511-b234-9f22-a164cb138255");
		var userUuids = new System.Collections.Generic.List<string>(){
			"d6bd5bde-7c57-3b81-3487-d1f1f2ebe27b",
			"78661aad-03d9-819a-2145-84cd9b4d1e54",
		};
		Article? article;
		foreach (var userUuid in userUuids) {
			var userId = Guid.Parse(userUuid);
			var userClient = client with {
				CurrentUserId = userId,
			};
			article = await userClient.GetArticle(articleWithCommentsId);
			Check.IsTrue(article != null);
			foreach (var comment in article.Comments) {
				bool? isUpvote = new Random(Seed: 3).Next(3) switch {
					0 => true,
					1 => false,
					_ => null,
				};
				var isUpvoted = comment.IsUpvoted;
				var changeResult = await userClient.SetCommentVote(comment.Id, isUpvote);
				if (isUpvote == isUpvoted) {
					Check.IsTrue(!changeResult);
				}
				else {
					Check.IsTrue(changeResult);
				}
			}
		}
		article = await client.GetArticle(articleWithCommentsId);
		Check.IsTrue(article != null);
		var comments = article.Comments;
		foreach (var comment in comments) {
			Logger.Log(@$"
{nameof(AppScenarios)}.{nameof(CheckSetCommentVotes)}:
comment {comment.Id}: {comment.UpvoteCount} Upvotes
and {comment.DownvoteCount} Downvotes".Trim());
		}
		foreach (var comment in comments) {
			var isUpvoted = comment.IsUpvoted;
			var isUpvotedText = isUpvoted switch {
				true => "Upvoted",
				false => "Downvoted",
				_ => "Not voted",
			};
			Logger.Log(@$"
{nameof(AppScenarios)}.{nameof(CheckSetCommentVotes)}:
comment {comment.Id}: isUpvoted = {isUpvoted}
({isUpvotedText})".Trim());
		}
		var anonymousClient = new AppClient(db);
		Check.IsTrue(anonymousClient != null);
		var commentIds = article.Comments.Select(c => c.Id).ToList();
		Check.IsTrue(!await anonymousClient.SetCommentVote(commentIds[0], true));
		var notRealClient = client with {
			CurrentUserId = Guid.NewGuid(),
		};
		Check.IsTrue(!await notRealClient.SetCommentVote(commentIds[0], true));
	}
}

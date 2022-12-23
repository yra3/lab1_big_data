using System;
namespace AppModel;
public record UserCard {
	public Guid Id { get; init; }
	public string FullName { get; init; } = "";
	public string Handle { get; init; } = "";
	public int ArticleCount { get; init; }
	public int CommentCount { get; init; }
	public int Karma { get; init; }
	public int KarmaVoteCount { get; init; }
}

using System;
using System.Collections.Generic;
namespace AppModel;
public record ArticleList {
	public IReadOnlyList<Article> Articles { get; init; } = Array.Empty<Article>();
	public int TotalCount { get; init; }
	public record Article {
		public Guid Id { get; init; }
		public string Title { get; init; } = "";
		public string ContentPreview { get; init; } = "";
		public User Author { get; init; } = User.Empty;
		public Company? Company { get; init; }
		public bool IsPublished { get; init; }
		public DateTime PublicationTime { get; init; }
		public long ViewCount { get; init; }
		public IReadOnlyList<Hub> Hubs { get; init; } = Array.Empty<Hub>();
	}
}

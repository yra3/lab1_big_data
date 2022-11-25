using System;
using System.Collections.Generic;
namespace AppModel;
public record Article {
	public Guid Id { get; init; }
	public string Title { get; init; } = "";
	public string Content { get; init; } = "";
	public User Author { get; init; } = User.Empty;
	public Company? Company { get; init; }
	public bool IsPublished { get; init; }
	public DateTime PublicationTime { get; init; }
	public long ViewCount { get; init; }
	public IReadOnlyList<Hub> Hubs { get; init; } = Array.Empty<Hub>();
	public IReadOnlyList<Poll> Polls { get; init; } = Array.Empty<Poll>();
	public IReadOnlyList<Comment> Comments { get; init; } = Array.Empty<Comment>();
	public record Poll {
		public Guid Id { get; init; }
		public string Title { get; init; } = "";
		public bool Multiple { get; init; }
		public IReadOnlyList<Variant> Variants { get; init; } = Array.Empty<Variant>();
		public record Variant {
			public Guid Id { get; init; }
			public string Title { get; init; } = "";
		}
	}
	public record Comment {
		public Guid Id { get; init; }
		public User Author { get; init; } = User.Empty;
		public DateTime PublicationTime { get; init; }
		public string Content { get; init; } = "";
		public IReadOnlyList<Comment> Children { get; init; } = Array.Empty<Comment>();
	}
}

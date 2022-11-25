using System;
using System.Collections.Generic;
namespace AppModel;
public record NewArticle {
	public string Title { get; init; } = "";
	public string Content { get; init; } = "";
	public IReadOnlyList<Guid> HubIds { get; init; } = Array.Empty<Guid>();
	public IReadOnlyList<Poll> Polls { get; init; } = Array.Empty<Poll>();
	public record Poll {
		public string Title { get; init; } = "";
		public bool Multiple { get; init; }
		public IReadOnlyList<Variant> Variants = Array.Empty<Variant>();
		public record Variant {
			public string Title { get; init; } = "";
		}
	}
}

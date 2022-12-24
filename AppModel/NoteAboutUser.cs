using System;
namespace AppModel;
public record NoteAboutUser {
	public User TargetUser { get; init; } = User.Empty;
	public string Content { get; init; } = "";
	public DateTime ModificationTime { get; init; }
}

namespace WebSignalR.Common
{
	public interface IObjectState
	{
		State State { get; set; }
	}

	public enum State
	{
		Added,
		Modified,
		Deleted,
		Unchanged
	}
}
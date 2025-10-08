namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager
{
	using Microsoft.Extensions.Logging;

	public partial class LockManager
	{
		internal static class LogEvents
		{
			internal readonly static EventId Locked = new EventId(2000, "Locked");

			internal readonly static EventId Unlocked = new EventId(2001, "Unlocked");
		}
	}
}

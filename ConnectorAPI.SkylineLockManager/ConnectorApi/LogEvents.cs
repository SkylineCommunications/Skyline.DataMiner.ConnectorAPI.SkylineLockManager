namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi
{
	using Microsoft.Extensions.Logging;

	public partial class SkylineLockManagerConnectorApi
	{
		internal static class LogEvents
		{
			internal readonly static EventId LockRequest = new EventId(1000, "LockRequest");

			internal readonly static EventId UnlockRequest = new EventId(1001, "UnlockRequest");
		}
	}
}
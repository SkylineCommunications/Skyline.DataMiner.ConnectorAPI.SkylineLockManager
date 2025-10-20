using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager;

namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Receiving
{
	using Microsoft.Extensions.Logging;
	using Skyline.DataMiner.Net;

	public interface IInterAppReceiver
	{
		void HandleIncomingInterAppMessage(string rawInterAppCall, ILockManager lockManager, IReporter reporter, ILogger logger, IConnection connection);
	}
}
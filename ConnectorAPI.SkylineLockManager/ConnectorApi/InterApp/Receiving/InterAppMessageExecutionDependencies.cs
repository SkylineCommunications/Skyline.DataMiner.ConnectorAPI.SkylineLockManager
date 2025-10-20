namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Receiving
{
	using Microsoft.Extensions.Logging;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager;

	internal class InterAppMessageExecutionDependencies
	{
		public InterAppMessageExecutionDependencies(ILogger logger, ILockManager lockManager, IReporter reporter) 
		{
			Logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			LockManager = lockManager ?? throw new System.ArgumentNullException(nameof(lockManager));
			Reporter = reporter ?? throw new System.ArgumentNullException(nameof(reporter));
		}

		public ILockManager LockManager { get; }

		public IReporter Reporter { get; }

		public ILogger Logger { get; }
	}
}

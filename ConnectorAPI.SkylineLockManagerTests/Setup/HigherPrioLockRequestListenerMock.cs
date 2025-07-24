namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests.Setup
{
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners.HigherPriorityLockRequests;

	internal class HigherPrioLockRequestListenerMock : HigherPriorityLockRequestListenerBase
	{
		private readonly LockManagerMock lockManager;

		public HigherPrioLockRequestListenerMock(LockManagerMock lockManager)
		{
			this.lockManager = lockManager ?? throw new ArgumentNullException(nameof(lockManager));
		}

		public int AmountOfTimesMonitorStarted { get; private set; } = 0;

		public int AmountOfTimesMonitorStopped { get; private set; } = 0;


		protected override void StartMonitor()
		{
			lockManager.HigherPriorityLockRequestReceived += LockManager_HigherPriorityLockRequestReceived;
			AmountOfTimesMonitorStarted++;
		}

		private void LockManager_HigherPriorityLockRequestReceived(object sender, LockObjectRequestEventArgs e)
		{
			ReportHigherPrioLockObjectRequest(e.LockObjectRequest);
		}

		protected override void StopMonitor()
		{
			lockManager.HigherPriorityLockRequestReceived -= LockManager_HigherPriorityLockRequestReceived;
			AmountOfTimesMonitorStopped++;
		}
	}
}

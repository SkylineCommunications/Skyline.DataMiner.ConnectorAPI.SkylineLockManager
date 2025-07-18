namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests.Setup
{
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners.HigherPriorityLockRequests;

	internal class HigherPrioLockRequestListenerMock : HigherPriorityLockRequestListenerBase
	{
		public HigherPrioLockRequestListenerMock(LockManagerMock lockManager)
		{
			LockManager = lockManager ?? throw new ArgumentNullException(nameof(lockManager));
		}

		public int AmountOfTimesMonitorStarted { get; private set; } = 0;

		public int AmountOfTimesMonitorStopped { get; private set; } = 0;

		public LockManagerMock LockManager { get; }

		protected override void StartMonitor()
		{
			LockManager.HigherPriorityLockRequestReceived += LockManager_HigherPriorityLockRequestReceived;
			AmountOfTimesMonitorStarted++;
		}

		private void LockManager_HigherPriorityLockRequestReceived(object sender, LockObjectRequestEventArgs e)
		{
			ReportHigherPrioLockObjectRequest(e.LockObjectRequest);
		}

		protected override void StopMonitor()
		{
			LockManager.HigherPriorityLockRequestReceived -= LockManager_HigherPriorityLockRequestReceived;
			AmountOfTimesMonitorStopped++;
		}
	}
}

namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests.Setup
{
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners.Unlocks;

	internal class UnlockListenerMock : UnlockListenerBase
	{
		public UnlockListenerMock(LockManagerMock lockManagerMock)
		{
			LockManagerMock = lockManagerMock ?? throw new ArgumentNullException(nameof(lockManagerMock));
		}

		public int AmountOfTimesMonitorStarted { get; private set; } = 0;

		public int AmountOfTimesMonitorStopped { get; private set; } = 0;

		public LockManagerMock LockManagerMock { get; }

		protected override void StartMonitor()
		{
			LockManagerMock.ObjectsUnlocked += LockManagerMock_ObjectsUnlocked;
			AmountOfTimesMonitorStarted++;
		}

		protected override void StopMonitor()
		{
			LockManagerMock.ObjectsUnlocked -= LockManagerMock_ObjectsUnlocked;
			AmountOfTimesMonitorStopped++;
		}

		private void LockManagerMock_ObjectsUnlocked(object sender, string[] objectIds)
		{
			ReportUnlockedObjects(objectIds);
		}
	}
}

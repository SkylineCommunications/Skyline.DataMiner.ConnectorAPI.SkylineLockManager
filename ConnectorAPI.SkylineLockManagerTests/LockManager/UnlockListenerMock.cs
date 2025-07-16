namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests.LockManager
{
	using System.Collections.Concurrent;
	using System.Threading.Tasks;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners.Unlocks;

	internal class UnlockListenerMock : UnlockListenerBase
	{
		public int AmountOfTimesMonitorStarted { get; private set; } = 0;

		public int AmountOfTimesMonitorStopped { get; private set; } = 0;

		public ConcurrentDictionary<string, TaskCompletionSource<bool>> TaskCompletionSources => taskCompletionSources;

		protected override void StartMonitor()
		{
			AmountOfTimesMonitorStarted++;
		}

		protected override void StopMonitor()
		{
			AmountOfTimesMonitorStopped++;
		}
	}
}

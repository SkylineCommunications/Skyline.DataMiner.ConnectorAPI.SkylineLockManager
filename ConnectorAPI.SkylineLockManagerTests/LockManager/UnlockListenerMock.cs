namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests.LockManager
{
	using System.Collections.Concurrent;
	using System.Threading.Tasks;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners.Unlocks;

	internal class UnlockListenerMock : Listener, IUnlockListener
	{
		private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> taskCompletionSources = new ConcurrentDictionary<string, TaskCompletionSource<bool>>();

		public int AmountOfTimesMonitorStarted { get; private set; } = 0;

		public int AmountOfTimesMonitorStopped { get; private set; } = 0;

		public ConcurrentDictionary<string, TaskCompletionSource<bool>> TaskCompletionSources => taskCompletionSources;

		public Task StartListeningForUnlock(string objectId)
		{
			var unlockTaskCompletionSource = taskCompletionSources.GetOrAdd(objectId, _ => new TaskCompletionSource<bool>());

			if (!isListening)
			{
				StartListening();
			}

			return unlockTaskCompletionSource.Task;
		}

		public void StopListeningForUnlock(string objectId)
		{
			if (taskCompletionSources.TryRemove(objectId, out var taskCompletionSource))
			{
				// Make sure the task listening to this TaskCompletionSource is canceled.
				taskCompletionSource.TrySetCanceled();
			}

			if (taskCompletionSources.IsEmpty && isListening)
			{
				StopListening();
			}
		}

		protected override void Dispose(bool disposing)
		{
			// Do nothing in this mock implementation.
		}

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

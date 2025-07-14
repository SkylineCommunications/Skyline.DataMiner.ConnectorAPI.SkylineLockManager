namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners.Unlocks
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	/// <inheritdoc cref="IUnlockListener"/>
	public class UnlockListener : Listener, IUnlockListener
	{
		private static readonly int UnlockUpdates_ParameterId = 101;

		/// <summary>
		/// A dictionary that holds TaskCompletionSources for each object ID being listened to.
		/// </summary>
		protected readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> taskCompletionSources = new ConcurrentDictionary<string, TaskCompletionSource<bool>>();

		/// <inheritdoc cref="IUnlockListener.StartListeningForUnlock(string)"/>
		public Task StartListeningForUnlock(string objectId)
		{
			var unlockTaskCompletionSource = taskCompletionSources.GetOrAdd(objectId, _ => new TaskCompletionSource<bool>());

			if (!isListening)
			{
				StartListening();
			}	

			return unlockTaskCompletionSource.Task;
		}

		/// <inheritdoc cref="IUnlockListener.StopListeningForUnlock(string)"/>
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

		/// <summary>
		/// Starts the monitor that listens for unlock events.
		/// </summary>
		protected override void StartMonitor()
		{
			// This method should start the monitor that listens for unlock events
			// and calls the reportUnlock action when an unlock event is detected.
			// Implementation depends on the specific requirements and context of the application.

			Action reportUnlock = () =>
			{
				string unlockedObjectId = Guid.NewGuid().ToString(); // Placeholder for actual object ID

				if (taskCompletionSources.TryGetValue(unlockedObjectId, out var taskCompletionSource))
				{
					taskCompletionSource.SetResult(true);
				}
			};
		}

		/// <summary>
		/// Stops the monitor that listens for unlock events.
		/// </summary>
		protected override void StopMonitor()
		{
			// This method should stop the monitor that listens for unlock events.
			// Implementation depends on the specific requirements and context of the application.
		}


		protected override void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					StopListening();
					foreach (var taskCompletionSource in taskCompletionSources.Values)
					{
						taskCompletionSource.TrySetCanceled();
					}
				}

				disposedValue = true;
			}
		}
	}
}
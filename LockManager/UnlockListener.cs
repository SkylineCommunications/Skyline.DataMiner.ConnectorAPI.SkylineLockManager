namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager
{
	using System;
	using System.Collections.Concurrent;
	using System.Threading.Tasks;

	/// <inheritdoc cref="IUnlockListener"/>
	public class UnlockListener : IUnlockListener
	{
		private static readonly int UnlockUpdates_ParameterId = 101;

		/// <summary>
		/// A dictionary that holds TaskCompletionSources for each object ID being listened to.
		/// </summary>
		protected readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> taskCompletionSources = new ConcurrentDictionary<string, TaskCompletionSource<bool>>();

		private readonly string sourceId = Guid.NewGuid().ToString();

		private bool startingListen;
		private bool stoppingListen;
		private bool isListening;

		private bool disposedValue;

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
			taskCompletionSources.TryRemove(objectId, out _);

			if (taskCompletionSources.IsEmpty && isListening)
			{
				StopListening();
			}
		}

		/// <summary>
		/// Disposes the <see cref="UnlockListener"/> instance, releasing any resources it holds.
		/// </summary>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Starts the monitor that listens for unlock events.
		/// </summary>
		protected virtual void StartMonitor()
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
		protected virtual void StopMonitor()
		{
			// This method should stop the monitor that listens for unlock events.
			// Implementation depends on the specific requirements and context of the application.
		}

		/// <summary>
		/// Releases the resources used by the current instance of the class.
		/// </summary>
		/// <remarks>This method should be called when the instance is no longer needed to ensure proper cleanup of
		/// resources. If the instance is used after calling this method, it may result in undefined behavior.</remarks>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					StopListening();
				}

				disposedValue = true;
			}
		}

		private void StartListening()
		{
			if (isListening || startingListen)
			{
				// Avoid re-entrancy
				return;
			}

			startingListen = true;

			StartMonitor();

			isListening = true;
			startingListen = false;
		}

		private void StopListening()
		{
			if (!isListening || stoppingListen)
			{
				// Avoid re-entrancy
				return;
			}

			stoppingListen = true;

			StopMonitor();

			isListening = false;
			stoppingListen = false;
		}
	}
}
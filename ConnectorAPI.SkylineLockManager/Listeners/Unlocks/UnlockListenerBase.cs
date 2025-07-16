namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners.Unlocks
{
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	/// <summary>
	/// Provides a base implementation for an unlock listener that monitors and manages unlock events for specified
	/// objects. This class is designed to be extended by concrete implementations that define the specific behavior for
	/// starting and stopping the listening process.
	/// </summary>
	/// <remarks>This class manages a collection of tasks associated with unlock events for specific object IDs. It
	/// ensures that tasks are properly created, canceled, or completed as unlock events are handled. Derived classes must
	/// implement the actual mechanisms for starting and stopping the listening process by overriding the <see
	/// cref="Listener.StartListening"/> and <see cref="Listener.StopListening"/> methods.</remarks>
	public abstract class UnlockListenerBase : Listener, IUnlockListener
	{
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

		/// <inheritdoc cref="IUnlockListener.StartListeningForUnlocks(ICollection{string})"/>
		public ICollection<Task> StartListeningForUnlocks(ICollection<string> objectIds)
		{
			var tasks = new List<Task>();

			foreach (var objectId in objectIds)
			{
				tasks.Add(StartListeningForUnlock(objectId));
			}

			return tasks;
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

		/// <inheritdoc cref="IUnlockListener.StopListeningForUnlocks(ICollection{string})"/>
		public void StopListeningForUnlocks(ICollection<string> objectIds)
		{
			foreach (var objectId in objectIds)
			{
				StopListeningForUnlock(objectId);
			}
		}

		/// <inheritdoc cref="Listener.Dispose(bool)"/>
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
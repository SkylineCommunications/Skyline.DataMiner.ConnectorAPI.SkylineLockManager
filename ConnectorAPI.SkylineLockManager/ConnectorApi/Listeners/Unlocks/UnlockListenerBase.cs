namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners.Unlocks
{
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Microsoft.Extensions.Logging;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners;

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
		/// <summary>
		/// A thread-safe dictionary that maps string keys to <see cref="TaskCompletionSource{TResult}"/> instances
		/// representing asynchronous operations.
		/// </summary>
		/// <remarks>This dictionary is used to manage and track the completion of asynchronous tasks identified by
		/// unique string keys. It ensures thread-safe access and modification of the task completion sources.</remarks>
		protected readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> taskCompletionSources = new ConcurrentDictionary<string, TaskCompletionSource<bool>>();

		/// <summary>
		/// Initializes a new instance of the <see cref="UnlockListenerBase"/> class with an optional logger.
		/// </summary>
		/// <param name="logger">An optional <see cref="ILogger"/> instance used for logging. If null, no logging will be performed.</param>
		protected UnlockListenerBase(ILogger logger = null) : base(logger)
		{
		}

		/// <inheritdoc cref="IUnlockListener.StartListeningForUnlock(string)"/>
		public Task<bool> StartListeningForUnlock(string objectId)
		{
			var unlockTaskCompletionSource = taskCompletionSources.GetOrAdd(objectId, _ => new TaskCompletionSource<bool>());

			if (!isListening)
			{
				StartListening();
			}

			return unlockTaskCompletionSource.Task;
		}

		/// <inheritdoc cref="IUnlockListener.StartListeningForUnlocks(ICollection{string})"/>
		public IReadOnlyDictionary<string, Task<bool>> StartListeningForUnlocks(ICollection<string> objectIds)
		{
			var tasks = new Dictionary<string, Task<bool>>();

			foreach (var objectId in objectIds)
			{
				tasks.Add(objectId, StartListeningForUnlock(objectId));
			}

			return tasks;
		}

		/// <inheritdoc cref="IUnlockListener.StopListeningForUnlock(string)"/>
		public void StopListeningForUnlock(string objectId)
		{
			if (taskCompletionSources.TryRemove(objectId, out var taskCompletionSource))
			{
				// Make sure the task listening to this TaskCompletionSource is completed.
				taskCompletionSource.TrySetResult(result: false);
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

		/// <summary>
		/// Reports the specified unlocked objects by completing their associated tasks.
		/// </summary>
		/// <remarks>This method iterates through the provided identifiers and attempts to resolve each one to a task.
		/// If a task is found, it is marked as completed with a result of <see langword="true"/>. Identifiers that do not
		/// have an associated task are ignored.</remarks>
		/// <param name="unlockedObjectIds">A collection of identifiers representing the unlocked objects. Each identifier is used to locate and complete the
		/// corresponding task.</param>
		protected void ReportUnlockedObjects(ICollection<string> unlockedObjectIds)
		{
			foreach (var unlockedObjectId in unlockedObjectIds)
			{
				if (taskCompletionSources.TryRemove(unlockedObjectId, out var taskCompletionSource))
				{
					// Remove the TaskCompletionSource from the dictionary when it is finished. This allows for new entries for the same objectId to be created later.
					taskCompletionSource.SetResult(result: true);
				}
			}

			if (taskCompletionSources.IsEmpty && isListening)
			{
				StopListening();
			}
		}

		/// <inheritdoc cref="Listener.Dispose(bool)"/>
		protected override void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					StopListeningForUnlocks(taskCompletionSources.Keys);
					StopListening();
				}

				disposedValue = true;
			}
		}
	}
}
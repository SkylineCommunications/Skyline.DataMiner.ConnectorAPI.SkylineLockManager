namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners.HigherPriorityLockRequests
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Locking;

	/// <summary>
	/// Provides a base class for listeners that monitor and handle lock requests with higher priority.
	/// </summary>
	/// <remarks>This abstract class implements the <see cref="IHigherPriorityLockRequestListener"/> interface and
	/// provides functionality for managing subscriptions to higher-priority lock requests. It allows derived classes to
	/// listen for and stop listening to lock requests with priorities higher than specified thresholds. The class also
	/// raises the <see cref="HigherPriorityLockRequestReceived"/> event when a higher-priority lock request is
	/// detected.</remarks>
	public abstract class HigherPriorityLockRequestListenerBase : Listener, IHigherPriorityLockRequestListener
	{
		/// <summary>
		/// Represents a thread-safe collection of object IDs and their associated priorities.
		/// </summary>
		protected readonly ConcurrentDictionary<string, HashSet<int>> objectIdsAndPriorities = new ConcurrentDictionary<string, HashSet<int>>();

		/// <summary>
		/// Initializes a new instance of the <see cref="HigherPriorityLockRequestListenerBase"/> class.
		/// </summary>
		/// <param name="logger">An optional <see cref="ILogger"/> instance used for logging. If <see langword="null"/>, no logging will be
		/// performed.</param>
		protected HigherPriorityLockRequestListenerBase(ILogger logger = null) : base(logger)
		{

		}

		/// <inheritdoc/>
		public event EventHandler<LockObjectRequestEventArgs> HigherPriorityLockRequestReceived;

		/// <inheritdoc/>
		public void ListenForLockRequestsWithHigherPriorityThan(string objectId, int priority)
		{
			ListenForLockRequestsWithHigherPriorityThan(new Dictionary<string, ICollection<int>>()
			{
				{ objectId, new List<int>() { priority } }
			});
		}

		/// <inheritdoc/>
		public void ListenForLockRequestsWithHigherPriorityThan(ICollection<KeyValuePair<string, ICollection<int>>> objectIdsAndPrioritiesToStartListeningFor)
		{
			if (objectIdsAndPrioritiesToStartListeningFor == null)
			{
				throw new ArgumentNullException(nameof(objectIdsAndPrioritiesToStartListeningFor));
			}

			if (objectIdsAndPrioritiesToStartListeningFor.Count == 0)
			{
				return;
			}

			if (objectIdsAndPrioritiesToStartListeningFor.Any(kvp => kvp.Value == null))
			{
				throw new ArgumentException("One or more key-value pairs have null values", nameof(objectIdsAndPrioritiesToStartListeningFor));
			}

			foreach (var objectIdAndPrioritiesToListenFor in objectIdsAndPrioritiesToStartListeningFor)
			{
				var priorities = objectIdsAndPriorities.GetOrAdd(objectIdAndPrioritiesToListenFor.Key, _ => new HashSet<int>());

				priorities.UnionWith(objectIdAndPrioritiesToListenFor.Value);
			}

			if (!objectIdsAndPriorities.IsEmpty && !isListening)
			{
				StartListening();
			}
		}

		/// <inheritdoc/>
		public void StopListeningForLockRequestsWithHigherPriorityThan(string objectId, int priority)
		{
			StopListeningForLockRequestsWithHigherPriorityThan(new Dictionary<string, ICollection<int>>()
			{
				{ objectId, new List<int>() { priority } }
			});
		}

		/// <inheritdoc/>
		public void StopListeningForLockRequestsWithHigherPriorityThan(ICollection<KeyValuePair<string, ICollection<int>>> objectIdsAndPrioritiesToStopListeningFor)
		{
			if (objectIdsAndPrioritiesToStopListeningFor == null)
			{
				throw new ArgumentNullException(nameof(objectIdsAndPrioritiesToStopListeningFor));
			}

			if (objectIdsAndPrioritiesToStopListeningFor.Count == 0)
			{
				return;
			}

			if (objectIdsAndPrioritiesToStopListeningFor.Any(kvp => kvp.Value == null))
			{
				throw new ArgumentException("One or more key-value pairs have null values", nameof(objectIdsAndPrioritiesToStopListeningFor));
			}

			foreach (var objectIdAndPriorities in objectIdsAndPrioritiesToStopListeningFor)
			{
				if (objectIdsAndPriorities.TryGetValue(objectIdAndPriorities.Key, out var priorities))
				{
					priorities.ExceptWith(objectIdAndPriorities.Value);

					if (priorities.Count == 0)
					{
						objectIdsAndPriorities.TryRemove(objectIdAndPriorities.Key, out _);
					}
				}
			}

			if (objectIdsAndPriorities.IsEmpty && isListening)
			{
				StopListening();
			}
		}

		/// <summary>
		/// Reports a higher-priority lock object request and triggers the appropriate action if a conflict is detected.
		/// </summary>
		/// <remarks>This method checks if the specified lock object request has a higher priority than any existing
		/// lock requests for the same object. If a conflict is detected, it invokes the necessary action to handle the
		/// higher-priority request.</remarks>
		/// <param name="lockObjectRequest">The lock object request containing the object ID and priority to be evaluated.</param>
		protected void ReportHigherPrioLockObjectRequest(LockObjectRequest lockObjectRequest)
		{
			string objectIdFromLockRequest = lockObjectRequest.ObjectId;
			var priorityFromLockRequest = lockObjectRequest.Priority;
			
			if (objectIdsAndPriorities.Any(idAndPrios => idAndPrios.Key == objectIdFromLockRequest && idAndPrios.Value.Any(prio => priorityFromLockRequest < prio /*lower value means higher priority*/)))
			{
				InvokeHigherPriorityLockRequestReceived(lockObjectRequest);
			}	
		}

		/// <summary>
		/// Invokes the <see cref="HigherPriorityLockRequestReceived"/> event.
		/// </summary>
		/// <remarks>This method triggers the <see cref="HigherPriorityLockRequestReceived"/> event, passing the
		/// provided  <paramref name="lockObjectRequest"/> wrapped in a <see cref="LockObjectRequestEventArgs"/> instance. 
		/// Subscribers to the event can use this information to handle the higher priority lock request.</remarks>
		/// <param name="lockObjectRequest">The lock request object containing details about the higher priority lock request.</param>
		protected void InvokeHigherPriorityLockRequestReceived(LockObjectRequest lockObjectRequest)
		{
			HigherPriorityLockRequestReceived?.Invoke(this, new LockObjectRequestEventArgs(lockObjectRequest));
		}

		/// <inheritdoc/>
		protected override void Dispose(bool disposing)
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
	}
}

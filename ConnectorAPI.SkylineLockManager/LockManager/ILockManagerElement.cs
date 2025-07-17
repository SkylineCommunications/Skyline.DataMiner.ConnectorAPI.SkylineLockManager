namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners.HigherPriorityLockRequests;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Locking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Unlocking;

	/// <summary>
	/// Represents an API to communicate with an element running the Skyline Lock Manager connector.
	/// </summary>
	public interface ILockManagerElement : IDisposable
	{
		/// <summary>
		/// Occurs when a higher-priority lock request is received.
		/// </summary>
		/// <remarks>This event is triggered to notify subscribers that a lock request with a higher priority has been
		/// made. Subscribers can use this event to handle scenarios where priority-based lock contention needs to be
		/// resolved.</remarks>
		event EventHandler<LockObjectRequestEventArgs> HigherPriorityLockRequestReceived;

		/// <summary>
		/// Attempts to lock the given object. Optionally, it can wait for a specified time until the lock is granted.
		/// </summary>
		/// <param name="request">Represents the requests to lock object.</param>
		/// <param name="maxWaitingTime">Optional timespan to wait until lock gets granted.</param>
		/// <returns>A <see cref="ILockInfo"/> object, containing info about the obtained lock.</returns>
		ILockObjectResult LockObject(LockObjectRequest request, TimeSpan? maxWaitingTime = null);

		/// <summary>
		/// Attempts to lock the given objects.
		/// </summary>
		/// <param name="requests">Represents the requests to lock objects.</param>
		/// <param name="maxWaitingTime">Optional timespan to wait until lock gets granted.</param>
		/// <returns>A collection of <see cref="ILockInfo"/>, containing info about the obtained locks.</returns>
		ILockObjectsResult LockObjects(IEnumerable<LockObjectRequest> requests, TimeSpan? maxWaitingTime = null);

		/// <summary>
		/// Attempts to unlock the given object.
		/// </summary>
		/// <param name="request">Represents the requests to unlock the object.</param>
		void UnlockObject(UnlockObjectRequest request);

		/// <summary>
		/// Attempts to unlock the given objects.
		/// </summary>
		/// <param name="requests">Represents the requests to unlock objects.</param>
		void UnlockObjects(IEnumerable<UnlockObjectRequest> requests);

		/// <summary>
		/// Returns a task that will complete when a higher priority lock request is made for the specified object ID and priority.
		/// </summary>
		void ListenForLockRequestsWithHigherPriorityThan(params ObjectIdAndPriority[] objectIdAndPriorities);

		/// <summary>
		/// Stops listening for higher priority lock requests for a specific object ID and priority.
		/// </summary>
		void StopListeningForLockRequestsWithHigherPriorityThan(params ObjectIdAndPriority[] objectIdAndPriorities);
	}
}

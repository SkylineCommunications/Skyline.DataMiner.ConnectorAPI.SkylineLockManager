namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners.HigherPriorityLockRequests;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Locking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Unlocking;

	/// <summary>
	/// Represents an API to communicate with an element running the Skyline Lock Manager connector.
	/// </summary>
	public interface ISkylineLockManagerConnectorApi : IDisposable
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
		ILockObjectsResult LockObject(LockObjectRequest request, TimeSpan? maxWaitingTime = null);

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
		/// Listens for lock requests on the specified object that have a higher priority than the given value.
		/// </summary>
		/// <remarks>This method enables monitoring of lock requests with higher priority levels, which can be useful
		/// for managing resource contention or implementing custom priority-based locking mechanisms.</remarks>
		/// <param name="objectId">The unique identifier of the object for which to monitor lock requests.</param>
		/// <param name="priority">The priority threshold. Only lock requests with a priority greater than this value will be considered.</param>
		void ListenForLockRequestsWithHigherPriorityThan(string objectId, int priority);

		/// <summary>
		/// Starts listening for lock requests with a higher priority than the specified priorities for the given object IDs.
		/// </summary>
		/// <remarks>This method enables monitoring of lock requests with higher priority levels for specific objects.
		/// It is useful in scenarios where priority-based lock management is required.</remarks>
		/// <param name="objectIdsAndPrioritiesToStartListeningFor">A collection of key-value pairs where each key represents an object ID, and the associated value is a collection of priority levels. The method will listen for lock requests with priorities higher than those specified for each object ID.</param>
		void ListenForLockRequestsWithHigherPriorityThan(ICollection<KeyValuePair<string, ICollection<int>>> objectIdsAndPrioritiesToStartListeningFor);

		/// <summary>
		/// Stops listening for lock requests with a priority higher than the specified value for the given object.
		/// </summary>
		/// <remarks>This method is used to filter out lock requests based on their priority, allowing the caller to
		/// focus on lower-priority requests.</remarks>
		/// <param name="objectId">The identifier of the object for which to stop listening for lock requests.</param>
		/// <param name="priority">The priority threshold. Lock requests with a priority higher than this value will no longer be listened to.</param>
		void StopListeningForLockRequestsWithHigherPriorityThan(string objectId, int priority);

		/// <summary>
		/// Stops listening for lock requests with a higher priority than the specified values.
		/// </summary>
		/// <remarks>This method is used to cease monitoring lock requests that exceed the specified priority levels
		/// for the given object IDs. Ensure that the provided collection is not null and contains valid object IDs and priority levels.</remarks>
		/// <param name="objectIdsAndPrioritiesToStopListeningFor">A collection of key-value pairs where each key represents an object ID, and the value is a collection of priority levels. The method stops listening for lock requests with priorities higher than those specified for each object ID.</param>
		void StopListeningForLockRequestsWithHigherPriorityThan(ICollection<KeyValuePair<string, ICollection<int>>> objectIdsAndPrioritiesToStopListeningFor);
	}
}

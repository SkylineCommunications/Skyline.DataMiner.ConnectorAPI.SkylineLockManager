namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners.HigherPriorityLockRequests
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Defines a contract for listening to and handling higher-priority lock requests.
	/// </summary>
	/// <remarks>This interface provides mechanisms to listen for and respond to lock requests with higher priority
	/// than a specified threshold. It is designed for scenarios where priority-based lock contention needs to be managed.
	/// Implementers of this interface should ensure proper resource cleanup by calling <see cref="IDisposable.Dispose"/>
	/// when the listener is no longer needed.</remarks>
	public interface IHigherPriorityLockRequestListener : IDisposable
	{
		/// <summary>
		/// Occurs when a higher-priority lock request is received.
		/// </summary>
		/// <remarks>This event is triggered to notify subscribers that a lock request with a higher priority has been made. Subscribers can use this event to handle scenarios where priority-based lock contention needs to be resolved.</remarks>
		event EventHandler<LockObjectRequestEventArgs> HigherPriorityLockRequestReceived;

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
namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners.HigherPriorityLockRequests
{
	using System;

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
		/// <remarks>This event is triggered to notify subscribers that a lock request with a higher priority has been
		/// made. Subscribers can use this event to handle scenarios where priority-based lock contention needs to be
		/// resolved.</remarks>
		event EventHandler<ObjectIdAndPriorityEventArgs> HigherPriorityLockRequestReceived;

		/// <summary>
		/// Returns a task that will complete when a higher priority lock request is made for the specified object ID and priority.
		/// </summary>
		/// <param name="objectId">The object ID.</param>
		/// <param name="priorityToCompareWith">The priority to compare with.</param>
		void ListenForLockRequestsWithHigherPriorityThan(string objectId, Priority priorityToCompareWith);

		/// <summary>
		/// Stops listening for higher priority lock requests for a specific object ID and priority.
		/// </summary>
		/// <param name="objectId">The object ID.</param>
		/// <param name="priorityToCompareWith">The priority to compare with.</param>
		void StopListeningForLockRequestsWithHigherPriorityThan(string objectId, Priority priorityToCompareWith);
	}
}
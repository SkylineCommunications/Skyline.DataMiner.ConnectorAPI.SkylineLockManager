namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners.HigherPriorityLockRequests;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Locking;
	using Skyline.DataMiner.Net.ToolsSpace.Collections;

	public abstract class HigherPriorityLockRequestListenerBase : Listener, IHigherPriorityLockRequestListener
	{
		protected readonly ConcurrentHashSet<ObjectIdAndPriority> objectIdsAndPriorities = new ConcurrentHashSet<ObjectIdAndPriority>();

		/// <inheritdoc cref="IHigherPriorityLockRequestListener.HigherPriorityLockRequestReceived"/>
		public event EventHandler<LockObjectRequestEventArgs> HigherPriorityLockRequestReceived;

		/// <inheritdoc cref="IHigherPriorityLockRequestListener.ListenForLockRequestsWithHigherPriorityThan(ObjectIdAndPriority[])"/>
		public void ListenForLockRequestsWithHigherPriorityThan(params ObjectIdAndPriority[] objectIdAndPriorities)
		{
			if (!objectIdAndPriorities.Any())
			{
				return;
			}

			foreach (var objectIdAndPriority in objectIdAndPriorities)
			{
				objectIdsAndPriorities.TryAdd(objectIdAndPriority);
			}

			if (!isListening)
			{
				StartListening();
			}
		}

		/// <inheritdoc cref="IHigherPriorityLockRequestListener.StopListeningForLockRequestsWithHigherPriorityThan(ObjectIdAndPriority[])"/>
		public void StopListeningForLockRequestsWithHigherPriorityThan(params ObjectIdAndPriority[] objectIdAndPriorities)
		{
			if (!objectIdAndPriorities.Any())
			{
				return;
			}

			foreach (var objectIdAndPriority in objectIdAndPriorities)
			{
				objectIdsAndPriorities.TryRemove(objectIdAndPriority);
			}

			if (objectIdsAndPriorities.IsEmpty && isListening)
			{
				StopListening();
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

		/// <inheritdoc cref="Listener.Dispose(bool)"/>
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

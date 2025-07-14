namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners.HigherPriorityLockRequests;
	using Skyline.DataMiner.Net.ToolsSpace.Collections;

	/// <summary>
	/// Represents a listener that listens for higher priority lock requests.
	/// </summary>
	public partial class HigherPriorityLockRequestListener : Listener, IHigherPriorityLockRequestListener
	{
		private static readonly int HigherPrioLockRequests_ParameterId = 101;

		protected ConcurrentHashSet<ObjectIdAndPriority> objectIdsAndPriorities = new ConcurrentHashSet<ObjectIdAndPriority>();

		/// <inheritdoc cref="IHigherPriorityLockRequestListener.HigherPriorityLockRequestReceived"/>
		public event EventHandler<ObjectIdAndPriorityEventArgs> HigherPriorityLockRequestReceived;

		/// <summary>
		/// Starts listening for higher priority lock requests for a specific object ID and priority.
		/// </summary>
		/// <param name="objectId"></param>
		/// <param name="priorityToCompareWith"></param>
		/// <returns></returns>
		public void ListenForLockRequestsWithHigherPriorityThan(string objectId, Priority priorityToCompareWith)
		{
			if (String.IsNullOrWhiteSpace(objectId))
			{
				throw new ArgumentNullException(nameof(objectId));
			}

			objectIdsAndPriorities.TryAdd(new ObjectIdAndPriority(objectId, priorityToCompareWith));

			if (!isListening)
			{
				StartListening();
			}
		}

		/// <summary>
		/// Stops listening for higher priority lock requests for a specific object ID and priority.
		/// </summary>
		/// <param name="objectId"></param>
		/// <param name="priorityToCompareWith"></param>
		public void StopListeningForLockRequestsWithHigherPriorityThan(string objectId, Priority priorityToCompareWith)
		{
			if (String.IsNullOrWhiteSpace(objectId))
			{
				throw new ArgumentNullException(nameof(objectId));
			}

			objectIdsAndPriorities.TryRemove(new ObjectIdAndPriority(objectId, priorityToCompareWith));

			if (objectIdsAndPriorities.IsEmpty && isListening)
			{
				StopListening();
			}
		}

		/// <inheritdoc cref="Listener.StartMonitor"/>
		protected override void StartMonitor()
		{
			// This method should start the monitor that listens for unlock events
			// and calls the reportUnlock action when an unlock event is detected.
			// Implementation depends on the specific requirements and context of the application.

			Action reportHigherPrioLockRequest = () =>
			{
				string objectIdFromLockRequest = Guid.NewGuid().ToString(); // Placeholder for actual object ID
				var priorityFromLockRequest = Priority.High; // Placeholder for actual priority to compare with

				var lowerPriorities = Enum.GetValues(typeof(Priority)).Cast<Priority>().Where(prio => (int)prio < (int)priorityFromLockRequest).ToList();

				foreach (var lowerPriority in lowerPriorities)
				{
					var objectIdAndPriority = new ObjectIdAndPriority(objectIdFromLockRequest, lowerPriority);

					if (objectIdsAndPriorities.Contains(objectIdAndPriority))
					{
						InvokeHigherPrioLockRequestReceived(new ObjectIdAndPriority(objectIdFromLockRequest, priorityFromLockRequest));
					}
				}
			};
		}

		/// <summary>
		/// Invokes the <see cref="HigherPriorityLockRequestReceived"/> event with the specified object ID and priority.
		/// </summary>
		/// <param name="objectIdAndPriority">An object containing the ID and priority of the lock request to be passed to the event.</param>
		protected void InvokeHigherPrioLockRequestReceived(ObjectIdAndPriority objectIdAndPriority)
		{
			HigherPriorityLockRequestReceived?.Invoke(this, new ObjectIdAndPriorityEventArgs(objectIdAndPriority.ObjectId, objectIdAndPriority.Priority));
		}

		/// <inheritdoc cref="Listener.StopMonitor"/>
		protected override void StopMonitor()
		{
			// This method should stop the monitor that listens for unlock events.
			// Implementation depends on the specific requirements and context of the application.
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

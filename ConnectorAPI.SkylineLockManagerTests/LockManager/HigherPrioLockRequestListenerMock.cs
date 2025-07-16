namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests.LockManager
{
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners.HigherPriorityLockRequests;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Locking;
	using Skyline.DataMiner.Net.ToolsSpace.Collections;

	internal class HigherPrioLockRequestListenerMock : Listener, IHigherPriorityLockRequestListener
	{
		private readonly ConcurrentHashSet<ObjectIdAndPriority> objectIdsAndPriorities = new ConcurrentHashSet<ObjectIdAndPriority>();

		public int AmountOfTimesMonitorStarted { get; private set; } = 0;

		public int AmountOfTimesMonitorStopped { get; private set; } = 0;

		/// <inheritdoc cref="IHigherPriorityLockRequestListener.HigherPriorityLockRequestReceived"/>
		public event EventHandler<LockObjectRequestEventArgs> HigherPriorityLockRequestReceived;

		public void SpoofLockRequest(LockObjectRequest lockObjectRequest)
		{
			var lowerPriorities = Enum.GetValues(typeof(Priority)).Cast<Priority>().Where(p => (int)p < (int)lockObjectRequest.Priority).ToList();

			foreach (var lowerPriority in lowerPriorities)
			{
				if (objectIdsAndPriorities.Contains(new ObjectIdAndPriority(lockObjectRequest.ObjectId, lowerPriority)))
				{
					HigherPriorityLockRequestReceived?.Invoke(this, new LockObjectRequestEventArgs(lockObjectRequest));
					break; // No need to check further if we found a match
				}
			}
		}

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

		protected override void StartMonitor()
		{
			AmountOfTimesMonitorStarted++;
		}

		protected override void StopMonitor()
		{
			AmountOfTimesMonitorStopped++;
		}

		protected override void Dispose(bool disposing)
		{
			// Nothing to dispose of in this mock
		}
	}
}

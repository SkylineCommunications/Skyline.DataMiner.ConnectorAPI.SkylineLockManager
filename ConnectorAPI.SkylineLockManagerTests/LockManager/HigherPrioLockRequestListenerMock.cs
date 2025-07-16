namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests.LockManager
{
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Locking;

	internal class HigherPrioLockRequestListenerMock : HigherPriorityLockRequestListenerBase
	{
		public int AmountOfTimesMonitorStarted { get; private set; } = 0;

		public int AmountOfTimesMonitorStopped { get; private set; } = 0;

		public void SpoofLockRequest(LockObjectRequest lockObjectRequest)
		{
			var lowerPriorities = Enum.GetValues(typeof(Priority)).Cast<Priority>().Where(p => (int)p < (int)lockObjectRequest.Priority).ToList();

			foreach (var lowerPriority in lowerPriorities)
			{
				if (objectIdsAndPriorities.Contains(new ObjectIdAndPriority(lockObjectRequest.ObjectId, lowerPriority)))
				{
					InvokeHigherPriorityLockRequestReceived(lockObjectRequest);
					break; // No need to check further if we found a match
				}
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
	}
}

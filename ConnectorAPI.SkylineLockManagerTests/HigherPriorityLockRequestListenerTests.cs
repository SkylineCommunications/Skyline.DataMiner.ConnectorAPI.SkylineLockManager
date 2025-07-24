namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests
{
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Locking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests.Setup;

	[TestClass]
	public class HigherPriorityLockRequestListenerTests
	{
		[TestMethod()]
		public void Dispose_MonitorStopped()
		{
			// Arrange
			var lockManagerMock = new LockManagerMock();
			var higherPrioLockRequestListenerMock = new HigherPrioLockRequestListenerMock(lockManagerMock);

			// Act
			higherPrioLockRequestListenerMock.ListenForLockRequestsWithHigherPriorityThan("objectId", priority: 1);

			higherPrioLockRequestListenerMock.Dispose();

			// Assert
			Assert.AreEqual(1, higherPrioLockRequestListenerMock.AmountOfTimesMonitorStopped);
		}

		[TestMethod]
		public void StartAndStopListening_SameObjectIdAndPriority_MonitorStartedAndStopped()
		{
			// Arrange
			var lockManagerMock = new LockManagerMock();
			var higherPrioLockRequestListenerMock = new HigherPrioLockRequestListenerMock(lockManagerMock);

			string objectId = "objectId";
			int priority = 1;

			// Act
			higherPrioLockRequestListenerMock.ListenForLockRequestsWithHigherPriorityThan(objectId, priority);
			higherPrioLockRequestListenerMock.StopListeningForLockRequestsWithHigherPriorityThan(objectId, priority);

			// Assert
			Assert.AreEqual(1, higherPrioLockRequestListenerMock.AmountOfTimesMonitorStarted);
			Assert.AreEqual(1, higherPrioLockRequestListenerMock.AmountOfTimesMonitorStopped);
		}

		[TestMethod]
		public void StartAndStopListening_DifferentObjectIdAndPriority_MonitorStarted()
		{
			// Arrange
			var lockManagerMock = new LockManagerMock();
			var higherPrioLockRequestListenerMock = new HigherPrioLockRequestListenerMock(lockManagerMock);

			string objectId = "objectId";
			int priority = 1;
			int differentPriority = 3;

			// Act
			higherPrioLockRequestListenerMock.ListenForLockRequestsWithHigherPriorityThan(objectId, priority);
			higherPrioLockRequestListenerMock.StopListeningForLockRequestsWithHigherPriorityThan(objectId, differentPriority);

			// Assert
			Assert.AreEqual(1, higherPrioLockRequestListenerMock.AmountOfTimesMonitorStarted);
			Assert.AreEqual(0, higherPrioLockRequestListenerMock.AmountOfTimesMonitorStopped);
		}

		[TestMethod()]
		public void ListenForHigherPriorityLockRequests_HigherPrioLockRequestCameIn()
		{
			// Arrange
			var lockManagerMock = new LockManagerMock();

			var higherPrioLockRequestListenerMock = new HigherPrioLockRequestListenerMock(lockManagerMock);

			string objectId = "objectId";
			int lowPriority = 5;

			LockObjectRequest receivedLockObjectRequest = null;
			higherPrioLockRequestListenerMock.HigherPriorityLockRequestReceived += (sender, args) =>
			{
				receivedLockObjectRequest = args.LockObjectRequest;
			};

			// Act
			higherPrioLockRequestListenerMock.ListenForLockRequestsWithHigherPriorityThan(objectId, lowPriority);

			// Simulate a lock request with a higher priority than the lowPriority threshold
			lockManagerMock.RequestLock(new LockObjectRequest
			{
				ObjectId = objectId,
				Priority = 1,
			});

			// Assert
			Assert.IsNotNull(receivedLockObjectRequest);
			Assert.AreEqual(objectId, receivedLockObjectRequest.ObjectId);
			Assert.IsTrue(receivedLockObjectRequest.Priority < lowPriority);
		}

		[TestMethod()]
		public void ListenForHigherPriorityLockRequests_SamePrioLockRequestCameIn()
		{
			// Arrange
			var lockManagerMock = new LockManagerMock();

			var higherPrioLockRequestListenerMock = new HigherPrioLockRequestListenerMock(lockManagerMock);

			string objectId = "objectId";
			int lowPriority = 5;

			LockObjectRequest receivedLockObjectRequest = null;
			higherPrioLockRequestListenerMock.HigherPriorityLockRequestReceived += (sender, args) =>
			{
				receivedLockObjectRequest = args.LockObjectRequest;
			};

			// Act
			higherPrioLockRequestListenerMock.ListenForLockRequestsWithHigherPriorityThan(objectId, lowPriority);

			// Simulate a lock request with the same priority as the lowPriority threshold
			lockManagerMock.RequestLock(new LockObjectRequest
			{
				ObjectId = objectId,
				Priority = lowPriority,
			});

			// Assert
			Assert.IsNull(receivedLockObjectRequest);
		}

		[TestMethod()]
		public void ListenForHigherPriorityLockRequests_LowerPrioLockRequestCameIn()
		{
			// Arrange
			var lockManagerMock = new LockManagerMock();

			var higherPrioLockRequestListenerMock = new HigherPrioLockRequestListenerMock(lockManagerMock);

			string objectId = "objectId";
			int lowPriority = 5;

			LockObjectRequest receivedLockObjectRequest = null;
			higherPrioLockRequestListenerMock.HigherPriorityLockRequestReceived += (sender, args) =>
			{
				receivedLockObjectRequest = args.LockObjectRequest;
			};

			// Act
			higherPrioLockRequestListenerMock.ListenForLockRequestsWithHigherPriorityThan(objectId, lowPriority);

			// Simulate a lock request with a lower priority than the lowPriority threshold
			lockManagerMock.RequestLock(new LockObjectRequest
			{
				ObjectId = objectId,
				Priority = 10,
			});

			// Assert
			Assert.IsNull(receivedLockObjectRequest);
		}
	}
}

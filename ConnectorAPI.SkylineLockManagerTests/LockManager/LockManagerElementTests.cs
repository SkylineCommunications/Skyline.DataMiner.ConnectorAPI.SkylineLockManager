namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager.Tests
{
	using System.Threading.Tasks;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners.HigherPriorityLockRequests;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Locking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Unlocking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests.LockManager.Setup;

	[TestClass()]
	public class LockManagerElementTests
	{
		[TestMethod()]
		public void ListenForHigherPriorityLockRequests_HigherPrioLockRequestCameIn()
		{
			// Arrange
			var lockManagerMock = new LockManagerMock();

			var higherPrioLockRequestListenerMock = new HigherPrioLockRequestListenerMock(lockManagerMock);
			var unlockListenerMock = new UnlockListenerMock(lockManagerMock);
			var interappHandlerMock = new InterAppHandlerMock(lockManagerMock);

			var lockManagerConnectorApi = new SkylineLockManagerConnectorApi(interappHandlerMock, unlockListenerMock, higherPrioLockRequestListenerMock);

			// Act
			lockManagerConnectorApi.ListenForLockRequestsWithHigherPriorityThan(new ObjectIdAndPriority("objectId", Priority.Medium));

			bool higherPrioLockRequestCameIn = false;
			lockManagerConnectorApi.HigherPriorityLockRequestReceived += (sender, args) =>
			{
				if (args.LockObjectRequest.ObjectId == "objectId" && args.LockObjectRequest.Priority > Priority.Medium)
				{
					higherPrioLockRequestCameIn = true;
				}
			};

			lockManagerMock.InvokeHigherPriorityLockRequestReceived(new LockObjectRequest
			{
				ObjectId = "objectId",
				Priority = Priority.High,
			});

			// Assert
			Assert.IsTrue(higherPrioLockRequestCameIn);
		}

		[TestMethod()]
		public void ListenForHigherPriorityLockRequests_NoHigherPrioLockRequestCameIn()
		{
			// Arrange
			var lockManagerMock = new LockManagerMock();

			var higherPrioLockRequestListenerMock = new HigherPrioLockRequestListenerMock(lockManagerMock);
			var unlockListenerMock = new UnlockListenerMock(lockManagerMock);
			var interappHandlerMock = new InterAppHandlerMock(lockManagerMock);

			var lockManagerConnectorApi = new SkylineLockManagerConnectorApi(interappHandlerMock, unlockListenerMock, higherPrioLockRequestListenerMock);

			// Act
			lockManagerConnectorApi.ListenForLockRequestsWithHigherPriorityThan(new ObjectIdAndPriority("objectId", Priority.Medium));

			bool higherPrioLockRequestCameIn = false;
			lockManagerConnectorApi.HigherPriorityLockRequestReceived += (sender, args) =>
			{
				if (args.LockObjectRequest.ObjectId == "objectId" && args.LockObjectRequest.Priority > Priority.Medium)
				{
					higherPrioLockRequestCameIn = true;
				}
			};

			lockManagerMock.InvokeHigherPriorityLockRequestReceived(new LockObjectRequest
			{
				ObjectId = "objectId",
				Priority = Priority.Medium,
			});

			// Assert
			Assert.IsFalse(higherPrioLockRequestCameIn);
		}

		[TestMethod()]
		public void LockObject_WithoutWait_NoSubscriptionMade()
		{
			// Arrange
			var lockManagerMock = new LockManagerMock();

			var higherPrioLockRequestListenerMock = new HigherPrioLockRequestListenerMock(lockManagerMock);
			var unlockListenerMock = new UnlockListenerMock(lockManagerMock);
			var interappHandlerMock = new InterAppHandlerMock(lockManagerMock);

			var lockManagerConnectorApi = new SkylineLockManagerConnectorApi(interappHandlerMock, unlockListenerMock, higherPrioLockRequestListenerMock);

			// Act
			var lockObjectRequest = new LockObjectRequest
			{
				ObjectId = "objectId",
			};

			lockManagerConnectorApi.LockObject(lockObjectRequest, maxWaitingTime: null);

			// Assert
			Assert.AreEqual(0, unlockListenerMock.AmountOfTimesMonitorStarted);
			Assert.AreEqual(0, unlockListenerMock.AmountOfTimesMonitorStopped);
		}

		[TestMethod()]
		public void LockObject_WithWait_SubscriptionMade()
		{
			// Arrange
			var lockManagerMock = new LockManagerMock(new Dictionary<string, LockedObject> { { "objectId", new LockedObject { ObjectId = "objectId" } } });

			var higherPrioLockRequestListenerMock = new HigherPrioLockRequestListenerMock(lockManagerMock);
			var unlockListenerMock = new UnlockListenerMock(lockManagerMock);
			var interappHandlerMock = new InterAppHandlerMock(lockManagerMock);

			var lockManagerConnectorApi = new SkylineLockManagerConnectorApi(interappHandlerMock, unlockListenerMock, higherPrioLockRequestListenerMock);

			// Act
			var lockObjectRequest = new LockObjectRequest
			{
				ObjectId = "objectId",
			};

			lockManagerConnectorApi.LockObject(lockObjectRequest, maxWaitingTime: TimeSpan.FromSeconds(1));

			// Assert
			Assert.AreEqual(1, unlockListenerMock.AmountOfTimesMonitorStarted);
			Assert.AreEqual(1, unlockListenerMock.AmountOfTimesMonitorStopped);
		}

		[TestMethod()]
		public async Task MultipleContextsWaitingForSameLock_AllContextsWaitForMaxWaitingTime()
		{
			// Arrange
			var lockManagerMock = new LockManagerMock();

			var higherPrioLockRequestListenerMock = new HigherPrioLockRequestListenerMock(lockManagerMock);
			var unlockListenerMock = new UnlockListenerMock(lockManagerMock);
			var interappHandlerMock = new InterAppHandlerMock(lockManagerMock);

			var lockManagerConnectorApi = new SkylineLockManagerConnectorApi(interappHandlerMock, unlockListenerMock, higherPrioLockRequestListenerMock);

			// Act
			var lockObjectRequest = new LockObjectRequest
			{
				ObjectId = "objectId",
			};

			// Context A locks the object
			lockManagerConnectorApi.LockObject(lockObjectRequest);
	
			// Contexts B,C and D try to lock the same object with a wait
			var maxWaitingTime = TimeSpan.FromSeconds(10);

			var taskToLockServiceFromContextB = Task.Run(() => lockManagerConnectorApi.LockObject(lockObjectRequest, maxWaitingTime));
			var taskToLockServiceFromContextC = Task.Run(() => lockManagerConnectorApi.LockObject(lockObjectRequest, maxWaitingTime));
			var taskToLockServiceFromContextD = Task.Run(() => lockManagerConnectorApi.LockObject(lockObjectRequest, maxWaitingTime));

			// Context A does some stuff while context B, C and D are waiting
			Task.Delay(TimeSpan.FromSeconds(2)).Wait();

			// Context A unlocks the service
			lockManagerConnectorApi.UnlockObject(new UnlockObjectRequest
			{
				ObjectId = "objectId",
			});

			// Context B, C and D finish waiting for the lock
			Task.WaitAll(taskToLockServiceFromContextB, taskToLockServiceFromContextC, taskToLockServiceFromContextD);
			var lockObjectResultFromContextB = taskToLockServiceFromContextB.Result;
			var lockObjectResultFromContextC = taskToLockServiceFromContextC.Result;
			var lockObjectResultFromContextD = taskToLockServiceFromContextD.Result;

			Assert.AreEqual(maxWaitingTime.TotalMilliseconds, lockObjectResultFromContextB.TotalWaitingTime.TotalMilliseconds, 20 /* Accurate up to 20 milliseconds */);
			Assert.AreEqual(maxWaitingTime.TotalMilliseconds, lockObjectResultFromContextC.TotalWaitingTime.TotalMilliseconds, 20 /* Accurate up to 20 milliseconds */);
			Assert.AreEqual(maxWaitingTime.TotalMilliseconds, lockObjectResultFromContextD.TotalWaitingTime.TotalMilliseconds, 20 /* Accurate up to 20 milliseconds */);
		}
	}
}
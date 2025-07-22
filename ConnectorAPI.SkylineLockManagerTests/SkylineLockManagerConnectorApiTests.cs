using Skyline.DataMiner.ConnectorAPI.SkylineLockManager;

namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests
{
	using System.Threading.Tasks;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners.HigherPriorityLockRequests;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Locking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Unlocking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests.Setup;

	[TestClass()]
	public class SkylineLockManagerConnectorApiTests
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

			lockManagerConnectorApi.LockObject(lockObjectRequest, maxWaitingTime: TimeSpan.FromSeconds(10));

			// Assert
			Assert.AreEqual(1, unlockListenerMock.AmountOfTimesMonitorStarted);
			Assert.AreEqual(1, unlockListenerMock.AmountOfTimesMonitorStopped);
		}

		[TestMethod()]
		public void MultipleContextsWaitingForSameLock_AllContextsWaitForMaxWaitingTime()
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

			// Context A unlocks the object
			lockManagerConnectorApi.UnlockObject(new UnlockObjectRequest
			{
				ObjectId = "objectId",
			});

			// Context B, C and D finish waiting for the lock
			Task.WaitAll(taskToLockServiceFromContextB, taskToLockServiceFromContextC, taskToLockServiceFromContextD);
			var lockObjectResultFromContextB = taskToLockServiceFromContextB.Result;
			var lockObjectResultFromContextC = taskToLockServiceFromContextC.Result;
			var lockObjectResultFromContextD = taskToLockServiceFromContextD.Result;

			var lockInfoFromContextThatGotTheLock = new[] {lockObjectResultFromContextB, lockObjectResultFromContextC, lockObjectResultFromContextD}
				.Where(x => x.LockInfosPerObjectId.ContainsKey("objectId") && x.LockInfosPerObjectId["objectId"].IsGranted);

			Assert.AreEqual(1, lockInfoFromContextThatGotTheLock.Count(), "Only one context should have gotten the lock.");
			Assert.AreEqual(2, lockInfoFromContextThatGotTheLock.Single().TotalWaitingTime.TotalSeconds, 0.1 /* Accurate up to 100 milliseconds */);

			var lockInfoFromContextsThatDidNotGetTheLock = new[] {lockObjectResultFromContextB, lockObjectResultFromContextC, lockObjectResultFromContextD}
				.Where(x => x.LockInfosPerObjectId.ContainsKey("objectId") && !x.LockInfosPerObjectId["objectId"].IsGranted);

			Assert.AreEqual(2, lockInfoFromContextsThatDidNotGetTheLock.Count(), "Two contexts should not have gotten the lock.");

			Assert.AreEqual(maxWaitingTime.TotalMilliseconds, lockInfoFromContextsThatDidNotGetTheLock.First().TotalWaitingTime.TotalMilliseconds, 20 /* Accurate up to 20 milliseconds */);
			Assert.AreEqual(maxWaitingTime.TotalMilliseconds, lockInfoFromContextsThatDidNotGetTheLock.Last().TotalWaitingTime.TotalMilliseconds, 20 /* Accurate up to 20 milliseconds */);
		}

		[TestMethod()]
		public void WaitForLock_WhileLinkedObjectIsLocked()
		{
			// Arrange
			var lockManagerMock = new LockManagerMock();

			var higherPrioLockRequestListenerMock = new HigherPrioLockRequestListenerMock(lockManagerMock);
			var unlockListenerMock = new UnlockListenerMock(lockManagerMock);
			var interappHandlerMock = new InterAppHandlerMock(lockManagerMock);

			var lockManagerConnectorApi = new SkylineLockManagerConnectorApi(interappHandlerMock, unlockListenerMock, higherPrioLockRequestListenerMock);

			// Act
			var lockLinkedObjectRequest = new LockObjectRequest
			{
				ObjectId = "linkedObjectId",
			};

			var lockObjectRequest = new LockObjectRequest
			{
				ObjectId = "objectId",
				LinkedObjectRequests = new List<LockObjectRequest>
				{
					lockLinkedObjectRequest,
				}
			};

			// Context A locks the linked object
			lockManagerConnectorApi.LockObject(lockLinkedObjectRequest);

			// Contexts B tries to lock an object with the linked object with a wait
			var taskToLockServiceFromContextB = Task.Run(() => lockManagerConnectorApi.LockObject(lockObjectRequest, TimeSpan.FromSeconds(10)));

			// Context A does some stuff while context B is waiting
			Task.Delay(TimeSpan.FromSeconds(2)).Wait();

			// Context A unlocks the linked object
			lockManagerConnectorApi.UnlockObject(new UnlockObjectRequest
			{
				ObjectId = "linkedObjectId",
			});

			// Context B finishes waiting for the lock
			taskToLockServiceFromContextB.Wait();
			var lockObjectsResultFromContextB = taskToLockServiceFromContextB.Result;

			// Assert
			Assert.AreEqual(2, lockObjectsResultFromContextB.LockInfosPerObjectId.Keys.Count());
			Assert.IsTrue(lockObjectsResultFromContextB.LockInfosPerObjectId.ContainsKey("objectId"));
			Assert.IsTrue(lockObjectsResultFromContextB.LockInfosPerObjectId.ContainsKey("linkedObjectId"));

			Assert.IsTrue(lockObjectsResultFromContextB.LockInfosPerObjectId["objectId"].IsGranted);
			Assert.IsTrue(lockObjectsResultFromContextB.LockInfosPerObjectId["linkedObjectId"].IsGranted);
		}
	}
}
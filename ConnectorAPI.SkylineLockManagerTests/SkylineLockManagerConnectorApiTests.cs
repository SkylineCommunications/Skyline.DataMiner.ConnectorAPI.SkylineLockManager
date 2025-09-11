namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests
{
	using System.Threading.Tasks;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Locking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Unlocking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests.Setup;

	[TestClass()]
	public class SkylineLockManagerConnectorApiTests
	{
		[TestMethod()]
		public void LockObjects_DuplicateObjectIds_ThrowsException()
		{
			// Arrange
			var lockManagerMock = new LockManagerMock();

			var higherPrioLockRequestListenerMock = new HigherPrioLockRequestListenerMock(lockManagerMock);
			var unlockListenerMock = new UnlockListenerMock(lockManagerMock);
			var interappHandlerMock = new InterAppHandlerMock(lockManagerMock);

			var lockManagerConnectorApi = new SkylineLockManagerConnectorApi(interappHandlerMock, unlockListenerMock, higherPrioLockRequestListenerMock);

			// Act
			var lockObjectsRequests = new[]
			{
				new LockObjectRequest { ObjectId = "objectId" },
				new LockObjectRequest { ObjectId = "objectId" } // Duplicate object ID
			};
			
			// Assert
			Assert.ThrowsException<ArgumentException>(() => lockManagerConnectorApi.LockObjects(lockObjectsRequests));
		}

		[TestMethod]
		public void Dispose_AllMonitorsStopped()
		{
			// Arrange
			var lockManagerMock = new LockManagerMock();

			var higherPrioLockRequestListenerMock = new HigherPrioLockRequestListenerMock(lockManagerMock);
			var unlockListenerMock = new UnlockListenerMock(lockManagerMock);
			var interappHandlerMock = new InterAppHandlerMock(lockManagerMock);

			var lockManagerConnectorApi = new SkylineLockManagerConnectorApi(interappHandlerMock, unlockListenerMock, higherPrioLockRequestListenerMock);

			// Act

			higherPrioLockRequestListenerMock.ListenForLockRequestsWithHigherPriorityThan("objectId", priority: 1);
			unlockListenerMock.StartListeningForUnlock("objectId");

			lockManagerConnectorApi.Dispose();

			// Assert
			Assert.AreEqual(1, unlockListenerMock.AmountOfTimesMonitorStopped);
			Assert.AreEqual(1, higherPrioLockRequestListenerMock.AmountOfTimesMonitorStopped);
		}

		[TestMethod()]
		public void LockObject_NullWait_NoMonitorStarted()
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
		public void LockObject_NegativeWait_NoMonitorStarted()
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

			lockManagerConnectorApi.LockObject(lockObjectRequest, maxWaitingTime: TimeSpan.FromSeconds(-30));

			// Assert
			Assert.AreEqual(0, unlockListenerMock.AmountOfTimesMonitorStarted);
			Assert.AreEqual(0, unlockListenerMock.AmountOfTimesMonitorStopped);
		}

		[TestMethod()]
		public void LockObject_ZeroWait_NoMonitorStarted()
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

			lockManagerConnectorApi.LockObject(lockObjectRequest, maxWaitingTime: TimeSpan.Zero);

			// Assert
			Assert.AreEqual(0, unlockListenerMock.AmountOfTimesMonitorStarted);
			Assert.AreEqual(0, unlockListenerMock.AmountOfTimesMonitorStopped);
		}

		[TestMethod()]
		public void LockObject_PositiveWait_MonitorStartedAndStopped()
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

			lockManagerConnectorApi.LockObject(lockObjectRequest, maxWaitingTime: TimeSpan.FromSeconds(2));

			// Assert
			Assert.AreEqual(1, unlockListenerMock.AmountOfTimesMonitorStarted);
			Assert.AreEqual(1, unlockListenerMock.AmountOfTimesMonitorStopped);
		}

		[TestMethod()]
		public void LockObject_WithWait_MonitorStartedAndStopped()
		{
			// Arrange
			string objectId = "objectId";

			var lockManagerMock = new LockManagerMock(new Dictionary<string, LockedObject> { { objectId, new LockedObject { ObjectId = objectId, LinkedObjectIds = new List<string>() } } });

			var higherPrioLockRequestListenerMock = new HigherPrioLockRequestListenerMock(lockManagerMock);
			var unlockListenerMock = new UnlockListenerMock(lockManagerMock);
			var interappHandlerMock = new InterAppHandlerMock(lockManagerMock);

			var lockManagerConnectorApi = new SkylineLockManagerConnectorApi(interappHandlerMock, unlockListenerMock, higherPrioLockRequestListenerMock);

			// Act
			var lockObjectRequest = new LockObjectRequest
			{
				ObjectId = objectId,
			};

			var taskToWaitFor = Task.Run(() => lockManagerConnectorApi.LockObject(lockObjectRequest, maxWaitingTime: TimeSpan.FromSeconds(10)));

			Task.Delay(TimeSpan.FromSeconds(2)).Wait();

			lockManagerMock.UnlockObject(objectId, unlockLinkedObjects: true);

			bool taskCompleted = taskToWaitFor.Wait(TimeSpan.FromSeconds(1));
			var lockObjectResult = taskToWaitFor.Result;

			// Assert
			Assert.IsTrue(taskCompleted, "Task should have completed within the wait time.");
			Assert.IsNotNull(lockObjectResult);
			Assert.IsTrue(lockObjectResult.LockInfosPerObjectId.Single().Value.IsGranted);
			Assert.AreEqual(1, unlockListenerMock.AmountOfTimesMonitorStarted);
			Assert.AreEqual(1, unlockListenerMock.AmountOfTimesMonitorStopped);
		}

		[TestMethod()]
		public void MultipleContextsWaitingForSameLock_AllContextsWaitForMaxWaitingTime()
		{
			// Arrange
			var lockManagerMock = new LockManagerMock();

			var higherPrioLockRequestListenerMockForContextA = new HigherPrioLockRequestListenerMock(lockManagerMock);
			var unlockListenerMockForContextA = new UnlockListenerMock(lockManagerMock);
			var interappHandlerMockForContextA = new InterAppHandlerMock(lockManagerMock);
			var lockManagerConnectorApiForContextA = new SkylineLockManagerConnectorApi(interappHandlerMockForContextA, unlockListenerMockForContextA, higherPrioLockRequestListenerMockForContextA);

			var higherPrioLockRequestListenerMockForContextB = new HigherPrioLockRequestListenerMock(lockManagerMock);
			var unlockListenerMockForContextB = new UnlockListenerMock(lockManagerMock);
			var interappHandlerMockForContextB = new InterAppHandlerMock(lockManagerMock);
			var lockManagerConnectorApiForContextB = new SkylineLockManagerConnectorApi(interappHandlerMockForContextB, unlockListenerMockForContextB, higherPrioLockRequestListenerMockForContextB);

			var higherPrioLockRequestListenerMockForContextC = new HigherPrioLockRequestListenerMock(lockManagerMock);
			var unlockListenerMockForContextC = new UnlockListenerMock(lockManagerMock);
			var interappHandlerMockForContextC = new InterAppHandlerMock(lockManagerMock);
			var lockManagerConnectorApiForContextC = new SkylineLockManagerConnectorApi(interappHandlerMockForContextC, unlockListenerMockForContextC, higherPrioLockRequestListenerMockForContextC);

			var higherPrioLockRequestListenerMockForContextD = new HigherPrioLockRequestListenerMock(lockManagerMock);
			var unlockListenerMockForContextD = new UnlockListenerMock(lockManagerMock);
			var interappHandlerMockForContextD = new InterAppHandlerMock(lockManagerMock);
			var lockManagerConnectorApiForContextD = new SkylineLockManagerConnectorApi(interappHandlerMockForContextD, unlockListenerMockForContextD, higherPrioLockRequestListenerMockForContextD);

			string objectId = "objectId";

			// Act
			var lockObjectRequest = new LockObjectRequest
			{
				ObjectId = objectId,
			};

			// Context A locks the object
			lockManagerConnectorApiForContextA.LockObject(lockObjectRequest);
	
			// Contexts B,C and D try to lock the same object with a wait
			var maxWaitingTime = TimeSpan.FromSeconds(10);

			var taskToLockServiceFromContextB = Task.Run(() => lockManagerConnectorApiForContextB.LockObject(lockObjectRequest, maxWaitingTime));
			var taskToLockServiceFromContextC = Task.Run(() => lockManagerConnectorApiForContextC.LockObject(lockObjectRequest, maxWaitingTime));
			var taskToLockServiceFromContextD = Task.Run(() => lockManagerConnectorApiForContextD.LockObject(lockObjectRequest, maxWaitingTime));

			// Context A does some stuff while context B, C and D are waiting
			Task.Delay(TimeSpan.FromSeconds(2)).Wait();

			// Context A unlocks the object
			lockManagerConnectorApiForContextA.UnlockObject(new UnlockObjectRequest
			{
				ObjectId = objectId,
			});

			// Context B, C and D finish waiting for the lock
			Task.WaitAll(taskToLockServiceFromContextB, taskToLockServiceFromContextC, taskToLockServiceFromContextD);
			var lockObjectResultFromContextB = taskToLockServiceFromContextB.Result;
			var lockObjectResultFromContextC = taskToLockServiceFromContextC.Result;
			var lockObjectResultFromContextD = taskToLockServiceFromContextD.Result;

			// Assert
			Assert.AreEqual(1 /*Context A*/ + 3 /*Initial attempt from contexts B,C,D*/ + 3/*Second attempt (after starting listening) from contexts B,C,D*/ + 3/*Third attempt (after unlock detected) from contexts B,C,D*/, lockManagerMock.ReceivedLockObjectRequests.Count);

			var lockInfosFromContextThatGotTheLock = new[] { lockObjectResultFromContextB, lockObjectResultFromContextC, lockObjectResultFromContextD }
				.Where(x => x.LockInfosPerObjectId.ContainsKey(objectId) && x.LockInfosPerObjectId[objectId].IsGranted).ToList();

			Assert.AreEqual(1, lockInfosFromContextThatGotTheLock.Count, "Only one context should have gotten the lock.");
			Assert.AreEqual(2, lockInfosFromContextThatGotTheLock.Single().TotalWaitingTime.TotalSeconds, 1 /* Accurate up to 1 second */);

			var lockInfoFromContextsThatDidNotGetTheLock = new[] {lockObjectResultFromContextB, lockObjectResultFromContextC, lockObjectResultFromContextD}
				.Where(x => x.LockInfosPerObjectId.TryGetValue(objectId, out var lockInfo) && !lockInfo.IsGranted).ToList();

			Assert.AreEqual(2, lockInfoFromContextsThatDidNotGetTheLock.Count, "Two contexts should not have gotten the lock.");

			Assert.AreEqual(maxWaitingTime.TotalSeconds, lockInfoFromContextsThatDidNotGetTheLock.First().TotalWaitingTime.TotalSeconds, 1 /* Accurate up to 1 second */, "Context that does not get the lock is expected to wait for the max waiting time.");
			Assert.AreEqual(maxWaitingTime.TotalSeconds, lockInfoFromContextsThatDidNotGetTheLock.Last().TotalWaitingTime.TotalSeconds, 1 /* Accurate up to 1 second */, "Context that does not get the lock is expected to wait for the max waiting time.");
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
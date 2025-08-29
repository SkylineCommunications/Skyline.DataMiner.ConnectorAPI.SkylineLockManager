namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests
{
	using System;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Locking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests.Setup;

	[TestClass]
	public class UnlockListenerTests
	{
		[TestMethod]
		public void Dispose_WhenListening_MonitorStopped()
		{
			// Arrange
			var lockManagerMock = new LockManagerMock();
			var unlockListenerMock = new UnlockListenerMock(lockManagerMock);

			// Act
			unlockListenerMock.StartListeningForUnlock("objectId");

			unlockListenerMock.Dispose();

			// Assert
			Assert.AreEqual(1, unlockListenerMock.AmountOfTimesMonitorStopped);
		}

		[TestMethod]
		public void Dispose_AllTasksCompletedWithNegativeResult()
		{
			// Arrange
			var lockManagerMock = new LockManagerMock();
			var unlockListenerMock = new UnlockListenerMock(lockManagerMock);

			// Act
			var firstTask = unlockListenerMock.StartListeningForUnlock("objectId");
			var secondTask = unlockListenerMock.StartListeningForUnlock("different objectId");

			unlockListenerMock.Dispose();

			// Assert
			Assert.IsTrue(firstTask.IsCompleted);
			Assert.IsFalse(firstTask.Result);

			Assert.IsTrue(secondTask.IsCompleted);
			Assert.IsFalse(secondTask.Result);
		}

		[TestMethod]
		public void StartAndStopListening_SameObjectId_MonitorStartedAndStopped()
		{
			// Arrange
			var lockManagerMock = new LockManagerMock();
			var unlockListenerMock = new UnlockListenerMock(lockManagerMock);

			string objectId = "objectId";

			// Act
			unlockListenerMock.StartListeningForUnlock(objectId);
			unlockListenerMock.StopListeningForUnlock(objectId);

			// Assert
			Assert.AreEqual(1, unlockListenerMock.AmountOfTimesMonitorStarted);
			Assert.AreEqual(1, unlockListenerMock.AmountOfTimesMonitorStopped);
		}

		[TestMethod]
		public void StartAndStopListening_DifferentObjectId_MonitorStarted()
		{
			// Arrange
			var lockManagerMock = new LockManagerMock();
			var unlockListenerMock = new UnlockListenerMock(lockManagerMock);

			string objectId = "objectId";
			string differentObjectId = "different objectId";

			// Act
			unlockListenerMock.StartListeningForUnlock(objectId);
			unlockListenerMock.StopListeningForUnlock(differentObjectId);

			// Assert
			Assert.AreEqual(1, unlockListenerMock.AmountOfTimesMonitorStarted);
			Assert.AreEqual(0, unlockListenerMock.AmountOfTimesMonitorStopped);
		}


		[TestMethod()]
		public void ListenForUnlocks_UnlockDetected_MonitorStartedAndStopped()
		{
			// Arrange
			var lockManagerMock = new LockManagerMock();

			var unlockListenerMock = new UnlockListenerMock(lockManagerMock);

			string objectId = "objectId";

			// Act
			lockManagerMock.RequestLock(new LockObjectRequest
			{
				ObjectId = objectId,
				Priority = 1
			});

			var task = unlockListenerMock.StartListeningForUnlock(objectId);

			lockManagerMock.UnlockObject(objectId, unlockLinkedObjects: true);

			bool taskCompletedSuccessfully = task.IsCompleted || task.Wait(timeout: TimeSpan.FromSeconds(1));
			bool unlockDetected = taskCompletedSuccessfully && task.Result;

			// Assert
			Assert.IsTrue(taskCompletedSuccessfully);
			Assert.IsTrue(unlockDetected);
			Assert.AreEqual(1, unlockListenerMock.AmountOfTimesMonitorStarted);
			Assert.AreEqual(0, unlockListenerMock.AmountOfTimesMonitorStopped);
		}

		[TestMethod()]
		public void ListenForUnlocks_OneUnlockDetected_MonitorStaysAlive()
		{
			// Arrange
			var lockManagerMock = new LockManagerMock();

			var unlockListenerMock = new UnlockListenerMock(lockManagerMock);

			string objectId = "objectId";
			string differentObjectId = "different objectId";

			// Act
			lockManagerMock.RequestLock(new LockObjectRequest
			{
				ObjectId = objectId,
				Priority = 1
			});

			lockManagerMock.RequestLock(new LockObjectRequest
			{
				ObjectId = differentObjectId,
				Priority = 1
			});

			var task = unlockListenerMock.StartListeningForUnlock(objectId);
			unlockListenerMock.StartListeningForUnlock(differentObjectId);

			lockManagerMock.UnlockObject(objectId, unlockLinkedObjects: true);

			bool taskCompletedSuccessfully = task.IsCompleted || task.Wait(timeout: TimeSpan.FromSeconds(1));
			bool unlockDetected = taskCompletedSuccessfully && task.Result;

			// Assert
			Assert.IsTrue(taskCompletedSuccessfully);
			Assert.IsTrue(unlockDetected);
			Assert.AreEqual(1, unlockListenerMock.AmountOfTimesMonitorStarted);
			Assert.AreEqual(0, unlockListenerMock.AmountOfTimesMonitorStopped);
		}
	}
}

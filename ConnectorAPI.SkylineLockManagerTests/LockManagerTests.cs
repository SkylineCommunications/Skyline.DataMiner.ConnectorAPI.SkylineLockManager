namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Locking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests.Setup;

	[TestClass]
	public class LockManagerTests
	{

		[TestMethod]
		public void UnlockExpiredObjects_UnlockedExpiredObject()
		{
			// Arrange
			var grantedLocks = new Dictionary<string, LockedObject>
			{
				{ Guid.NewGuid().ToString(), new LockedObject{ AutoUnlockTimestamp = DateTime.Now.AddMinutes(-1) } },
			};

			var lockManager = new LockManager(grantedLocks);

			// Act
			lockManager.UnlockExpiredObjects();

			// Assert
			Assert.AreEqual(0, grantedLocks.Count);
		}

		[TestMethod]
		public void UnlockExpiredObjects_KeepNonExpiredObjectLocked()
		{
			// Arrange
			var grantedLocks = new Dictionary<string, LockedObject>
			{
				{ Guid.NewGuid().ToString(), new LockedObject{ AutoUnlockTimestamp = DateTime.Now.AddMinutes(2) } },
			};

			var lockManager = new LockManager(grantedLocks);

			// Act
			lockManager.UnlockExpiredObjects();

			// Assert
			Assert.AreEqual(1, grantedLocks.Count);
		}

		[TestMethod]
		public void RequestSingleLock_Granted()
		{
			// Arrange
			var lockManager = new LockManager();

			string objectId = Guid.NewGuid().ToString();

			// Act
			var results = lockManager.RequestLock(
				new LockObjectRequest
				{
					ObjectId = objectId,
					ObjectDescription = "object description",
					LockRequesterInfo = "unit test",
					LinkedObjectRequests = new List<LockObjectRequest>(),
					AutoUnlockTimeSpan = TimeSpan.FromMinutes(5),
				});

			// Assert
			Assert.AreEqual(1, results.Count());
			Assert.IsTrue(results.Single().LockIsGranted);
			Assert.AreEqual(objectId, results.Single().ObjectId);
			Assert.AreEqual("unit test", results.Single().LockHolderInfo);
		}

		[TestMethod]
		public void RequestMultipleLocks_Granted()
		{
			// Arrange
			var lockManager = new LockManager();

			string objectId = Guid.NewGuid().ToString();
			string firstLinkedobjectId = Guid.NewGuid().ToString();
			string secondLinkedobjectId = Guid.NewGuid().ToString();

			// Act
			var results = lockManager.RequestLock(
				new LockObjectRequest
				{
					ObjectId = objectId,
					ObjectDescription = "object description",
					LockRequesterInfo = "unit test",
					LinkedObjectRequests = new List<LockObjectRequest>
					{
						new LockObjectRequest
						{
							ObjectId = firstLinkedobjectId,
							ObjectDescription = "first linked object",
							LockRequesterInfo = "unit test",
							AutoUnlockTimeSpan = TimeSpan.FromMinutes(5),
							LinkedObjectRequests = new List<LockObjectRequest>(),
						},
						new LockObjectRequest
						{
							ObjectId = secondLinkedobjectId,
							ObjectDescription = "second linked object",
							LockRequesterInfo = "unit test",
							AutoUnlockTimeSpan = TimeSpan.FromMinutes(5),
							LinkedObjectRequests = new List<LockObjectRequest>(),
						},
					},
					AutoUnlockTimeSpan = TimeSpan.FromMinutes(5),
				});

			// Assert
			Assert.AreEqual(3, results.Count());
			Assert.IsTrue(results.All(r => r.LockIsGranted));
		}

		[TestMethod]
		public void RequestLock_NotGranted()
		{
			// Arrange
			string objectId = Guid.NewGuid().ToString();

			var lockedObjects = new Dictionary<string, LockedObject>
			{
				{
					objectId, new LockedObject
					{
						ObjectId = objectId,
						ObjectDescription = "object description",
						LockHolderInfo = "first requester",
						LinkedObjectIds = new List<string>(),
						AutoUnlockTimestamp = DateTime.Now.AddMinutes(5),
					}
				},
			};

			var lockManager = new LockManager(lockedObjects);

			// Act
			var results = lockManager.RequestLock(new LockObjectRequest
			{
				ObjectId = objectId,
				ObjectDescription = "object description",
				LockRequesterInfo = "second requester",
				LinkedObjectRequests = new List<LockObjectRequest>(),
				AutoUnlockTimeSpan = TimeSpan.FromMinutes(5),
			});

			// Assert
			Assert.AreEqual(1, results.Count());
			Assert.IsFalse(results.Single().LockIsGranted);
			Assert.AreEqual(objectId, results.Single().ObjectId);
			Assert.AreEqual("first requester", results.Single().LockHolderInfo);
		}

		[TestMethod]
		public void RequestMultipleLocks_NotGranted()
		{
			// Arrange
			string objectId = Guid.NewGuid().ToString();
			string firstLinkedobjectId = Guid.NewGuid().ToString();
			string secondLinkedobjectId = Guid.NewGuid().ToString();

			var lockedObjects = new Dictionary<string, LockedObject>
			{
				{
					firstLinkedobjectId, new LockedObject
					{
						ObjectId = firstLinkedobjectId,
						ObjectDescription = "first linked object",
						LockHolderInfo = "first requester",
						LinkedObjectIds = new List<string>(),
						AutoUnlockTimestamp = DateTime.Now.AddMinutes(5),
					}
				},
			};

			var lockManager = new LockManager(lockedObjects);

			// Act
			var results = lockManager.RequestLock(
				new LockObjectRequest
				{
					ObjectId = objectId,
					ObjectDescription = "main object",
					LockRequesterInfo = "second requester",
					LinkedObjectRequests = new List<LockObjectRequest>
					{
						new LockObjectRequest
						{
							ObjectId = firstLinkedobjectId,
							ObjectDescription = "first linked object",
							LockRequesterInfo = "second requester",
							AutoUnlockTimeSpan = TimeSpan.FromMinutes(5),
							LinkedObjectRequests = new List<LockObjectRequest>(),
						},
						new LockObjectRequest
						{
							ObjectId = secondLinkedobjectId,
							ObjectDescription = "second linked object",
							LockRequesterInfo = "second requester",
							AutoUnlockTimeSpan = TimeSpan.FromMinutes(5),
							LinkedObjectRequests = new List<LockObjectRequest>(),
						},
					},
					AutoUnlockTimeSpan = TimeSpan.FromMinutes(5),
				});

			// Assert
			Assert.AreEqual(3, results.Count());
			Assert.IsTrue(results.All(r => !r.LockIsGranted));
		}
	}
}

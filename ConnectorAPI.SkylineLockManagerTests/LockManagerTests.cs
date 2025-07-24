namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Locking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager;

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
			var result = lockManager.RequestLock(
				new LockObjectRequest
				{
					ObjectId = objectId,
					ObjectDescription = "object description",
					ContextInfo = "unit test",
					LinkedObjectRequests = new List<LockObjectRequest>(),
					AutoUnlockTimeSpan = TimeSpan.FromMinutes(5),
				});

			// Assert
			Assert.AreEqual(1, result.Flatten().Count());
			Assert.IsTrue(result.Flatten().Single().LockIsGranted);
			Assert.IsTrue(result.Flatten().Single().LockIsAvailable);
			Assert.AreEqual(objectId, result.Flatten().Single().ObjectId);
			Assert.AreEqual("unit test", result.Flatten().Single().LockHolderInfo);
		}

		[TestMethod]
		public void RequestLayeredLock_Granted()
		{
			// Arrange
			var lockManager = new LockManager();

			string objectId = Guid.NewGuid().ToString();
			string firstLinkedobjectId = Guid.NewGuid().ToString();
			string secondLinkedobjectId = Guid.NewGuid().ToString();

			// Act
			var result = lockManager.RequestLock(
				new LockObjectRequest
				{
					ObjectId = objectId,
					ObjectDescription = "object description",
					ContextInfo = "unit test",
					LinkedObjectRequests = new List<LockObjectRequest>
					{
						new LockObjectRequest
						{
							ObjectId = firstLinkedobjectId,
							ObjectDescription = "first linked object",
							ContextInfo = "unit test",
							AutoUnlockTimeSpan = TimeSpan.FromMinutes(5),
							LinkedObjectRequests = new List<LockObjectRequest>(),
						},
						new LockObjectRequest
						{
							ObjectId = secondLinkedobjectId,
							ObjectDescription = "second linked object",
							ContextInfo = "unit test",
							AutoUnlockTimeSpan = TimeSpan.FromMinutes(5),
							LinkedObjectRequests = new List<LockObjectRequest>(),
						},
					},
					AutoUnlockTimeSpan = TimeSpan.FromMinutes(5),
				});

			// Assert
			Assert.AreEqual(3, result.Flatten().Count());
			Assert.IsTrue(result.Flatten().All(r => r.LockIsGranted));
			Assert.IsTrue(result.Flatten().All(r => r.LockIsAvailable));
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
						ContextInfo = "first requester",
						LinkedObjectIds = new List<string>(),
						AutoUnlockTimestamp = DateTime.Now.AddMinutes(5),
					}
				},
			};

			var lockManager = new LockManager(lockedObjects);

			// Act
			var result = lockManager.RequestLock(new LockObjectRequest
			{
				ObjectId = objectId,
				ObjectDescription = "object description",
				ContextInfo = "second requester",
				LinkedObjectRequests = new List<LockObjectRequest>(),
				AutoUnlockTimeSpan = TimeSpan.FromMinutes(5),
			});

			// Assert
			Assert.AreEqual(1, result.Flatten().Count());
			Assert.IsFalse(result.Flatten().Single().LockIsGranted);
			Assert.IsFalse(result.Flatten().Single().LockIsAvailable);
			Assert.AreEqual(objectId, result.Flatten().Single().ObjectId);
			Assert.AreEqual("first requester", result.Flatten().Single().LockHolderInfo);
		}

		[TestMethod]
		public void RequestLayeredLock_NotGranted()
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
						ContextInfo = "first requester",
						LinkedObjectIds = new List<string>(),
						AutoUnlockTimestamp = DateTime.Now.AddMinutes(5),
					}
				},
			};

			var lockManager = new LockManager(lockedObjects);

			// Act
			var result = lockManager.RequestLock(
				new LockObjectRequest
				{
					ObjectId = objectId,
					ObjectDescription = "main object",
					ContextInfo = "second requester",
					LinkedObjectRequests = new List<LockObjectRequest>
					{
						new LockObjectRequest
						{
							ObjectId = firstLinkedobjectId,
							ObjectDescription = "first linked object",
							ContextInfo = "second requester",
							AutoUnlockTimeSpan = TimeSpan.FromMinutes(5),
							LinkedObjectRequests = new List<LockObjectRequest>(),
						},
						new LockObjectRequest
						{
							ObjectId = secondLinkedobjectId,
							ObjectDescription = "second linked object",
							ContextInfo = "second requester",
							AutoUnlockTimeSpan = TimeSpan.FromMinutes(5),
							LinkedObjectRequests = new List<LockObjectRequest>(),
						},
					},
					AutoUnlockTimeSpan = TimeSpan.FromMinutes(5),
				});

			// Assert
			Assert.AreEqual(3, result.Flatten().Count());
			Assert.IsTrue(result.Flatten().All(r => !r.LockIsGranted));

			var responseForMainObject = result.Flatten().First(r => r.ObjectId == objectId);
			Assert.IsTrue(responseForMainObject.LockIsAvailable);

			var responseForFirstLinkedObject = result.Flatten().First(r => r.ObjectId == firstLinkedobjectId);
			Assert.IsFalse(responseForFirstLinkedObject.LockIsAvailable);

			var responseForSecondLinkedObject = result.Flatten().First(r => r.ObjectId == secondLinkedobjectId);
			Assert.IsTrue(responseForSecondLinkedObject.LockIsAvailable);
		}
	}
}

namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests.Setup
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners.HigherPriorityLockRequests;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Locking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager;

	public class LockManagerMock : LockManager
	{
		private List<LockObjectRequest> receivedLockObjectRequests = new List<LockObjectRequest>();

		public LockManagerMock(IDictionary<string, LockedObject>? lockedObjects = null) : base(lockedObjects)
		{

		}

		public IReadOnlyCollection<LockObjectRequest> ReceivedLockObjectRequests => receivedLockObjectRequests;

		public event EventHandler<string[]>? ObjectsUnlocked;

		public event EventHandler<LockObjectRequestEventArgs>? HigherPriorityLockRequestReceived;

		public void InvokeObjectsUnlocked(string[] objectIds)
		{
			ObjectsUnlocked?.Invoke(this, objectIds);
		}

		public override LockObjectResponse RequestLock(LockObjectRequest lockObjectRequest)
		{
			var result = base.RequestLock(lockObjectRequest);

			receivedLockObjectRequests.Add(lockObjectRequest);

			InvokeHigherPriorityLockRequestReceived(lockObjectRequest);

			return result;
		}

		public override ICollection<string> UnlockAllObjects()
		{
			var unlockedObjectIds = base.UnlockAllObjects();

			InvokeObjectsUnlocked(unlockedObjectIds.ToArray());

			return unlockedObjectIds;
		}

		public override ICollection<string> UnlockObject(string objectId, bool unlockLinkedObjects)
		{
			var unlockedObjectIds = base.UnlockObject(objectId, unlockLinkedObjects);

			InvokeObjectsUnlocked(unlockedObjectIds.ToArray());

			return unlockedObjectIds;
		}

		private void InvokeHigherPriorityLockRequestReceived(LockObjectRequest lockObjectRequest)
		{
			HigherPriorityLockRequestReceived?.Invoke(this, new LockObjectRequestEventArgs(lockObjectRequest));
		}
	}
}

namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests.LockManager.Setup
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners.HigherPriorityLockRequests;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Locking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager;

	public class LockManagerMock : LockManager
	{
		public LockManagerMock(IDictionary<string, LockedObject>? lockedObjects = null) : base(lockedObjects)
		{

		}

		public event EventHandler<string[]>? ObjectsUnlocked;

		public event EventHandler<LockObjectRequestEventArgs>? HigherPriorityLockRequestReceived;

		public void InvokeObjectsUnlocked(string[] objectIds)
		{
			ObjectsUnlocked?.Invoke(this, objectIds);
		}

		public void InvokeHigherPriorityLockRequestReceived(LockObjectRequest lockObjectRequest)
		{
			HigherPriorityLockRequestReceived?.Invoke(this, new LockObjectRequestEventArgs(lockObjectRequest));
		}
	}
}

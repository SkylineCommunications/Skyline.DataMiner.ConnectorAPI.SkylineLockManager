namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Locking;

	public interface ILockManager
	{
		TimeSpan DefaultAutoLockReleaseTimeSpan { get; }

		IEnumerable<LockObjectResponse> RequestLock(LockObjectRequest lockObjectRequest);

		ICollection<string> UnlockAllObjects();

		void UnlockExpiredObjects();

		ICollection<string> UnlockObject(string objectId, bool unlockLinkedObjects);
	}
}
namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager
{
	using System;

	public interface ILockInfo
	{
		string ObjectId { get; }

		string LockHolderInfo { get; }

		bool IsGranted { get; }

		DateTime AutoUnlockTimestamp { get; }
	}
}

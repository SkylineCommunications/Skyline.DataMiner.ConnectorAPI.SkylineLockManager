namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi
{
	using System;

	/// <inheritdoc cref="ILockInfo"/>
	public class LockInfo : ILockInfo
	{
		/// <inheritdoc cref="ILockInfo.ObjectId"/>
		public string ObjectId { get; set; }

		/// <inheritdoc cref="ILockInfo.IsGranted"/>
		public bool IsGranted { get; set; }

		/// <inheritdoc cref="ILockInfo.LockHolderInfo"/>
		public string LockHolderInfo { get; set; }

		/// <inheritdoc cref="ILockInfo.AutoUnlockTimestamp"/>
		public DateTime AutoUnlockTimestamp { get; set; }
	}
}

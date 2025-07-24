namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi
{
	using System;

	/// <inheritdoc cref="ILockInfo"/>
	public class LockInfo : LockBaseInfo, ILockInfo
	{
		/// <inheritdoc/>
		public bool IsGranted { get; set; }

		/// <inheritdoc/>
		public DateTime AutoUnlockTimestamp { get; set; }
	}
}

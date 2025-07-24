namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi
{
	using System;

	/// <summary>
	/// An interface representing info about a requested lock.
	/// </summary>
	public interface ILockInfo : ILockBaseInfo
	{
		/// <summary>
		/// Gets a boolean indicating if the lock was granted or not.
		/// </summary>
		bool IsGranted { get; }

		/// <summary>
		/// Gets a timestamp when the lock will be automatically released by the element.
		/// </summary>
		DateTime AutoUnlockTimestamp { get; }
	}
}

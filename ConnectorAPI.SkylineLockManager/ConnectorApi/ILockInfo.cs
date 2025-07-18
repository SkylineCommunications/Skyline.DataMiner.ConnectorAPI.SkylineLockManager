namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi
{
	using System;

	/// <summary>
	/// An interface representing info about a requested lock.
	/// </summary>
	public interface ILockInfo
	{
		/// <summary>
		/// Gets the ID of the object for which lock info is given.
		/// </summary>
		string ObjectId { get; }

		/// <summary>
		/// Gets the information about the holder of the lock for the given object.
		/// </summary>
		string LockHolderInfo { get; }

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

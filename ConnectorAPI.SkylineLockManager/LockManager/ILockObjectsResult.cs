namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager
{
	using System;
	using System.Collections.Generic;

	///<summary>
	/// Represents the result of attempting to lock multiple objects, including lock information and total waiting time.
	/// </summary>
	public interface ILockObjectsResult
	{
		/// <summary>
		/// Gets the lock information for each object ID.
		/// </summary>
		IReadOnlyDictionary<string, ILockInfo> LockInfosPerObjectId { get; }

		/// <summary>
		/// Gets the total time spent waiting to acquire the locks.
		/// </summary>
		TimeSpan TotalWaitingTime { get; }
	}
}
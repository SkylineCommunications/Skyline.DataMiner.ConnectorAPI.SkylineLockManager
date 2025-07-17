namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager
{
	using System;

	///<summary>
	/// Represents the result of a lock operation, including lock information and the total waiting time.
	/// </summary>
	public interface ILockObjectResult
	{

		/// <summary>
		/// Gets the lock information associated with the lock operation.
		/// </summary>
		ILockInfo LockInfo { get; }

		/// <summary>
		/// Gets the total time spent waiting to acquire the lock.
		/// </summary>
		TimeSpan TotalWaitingTime { get; }
	}
}
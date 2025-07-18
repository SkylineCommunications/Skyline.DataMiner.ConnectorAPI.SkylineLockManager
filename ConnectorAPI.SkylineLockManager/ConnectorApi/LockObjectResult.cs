namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi
{
	using System;

	/// <inheritdoc cref="ILockObjectResult"/>
	public class LockObjectResult : ILockObjectResult
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LockObjectResult"/> class.
		/// </summary>
		/// <param name="lockInfo">The lock information associated with the lock operation.</param>
		/// <param name="totalWaitingTime">The total time spent waiting to acquire the lock.</param>
		/// <exception cref="ArgumentNullException"><paramref name="lockInfo"/> is <c>null</c>.</exception>
		internal LockObjectResult(ILockInfo lockInfo, TimeSpan totalWaitingTime)
		{
			LockInfo = lockInfo ?? throw new ArgumentNullException(nameof(lockInfo));
			TotalWaitingTime = totalWaitingTime;
		}

		/// <inheritdoc cref="ILockObjectResult.LockInfo"/>
		public ILockInfo LockInfo { get; }

		/// <inheritdoc cref="ILockObjectResult.TotalWaitingTime"/>
		public TimeSpan TotalWaitingTime { get; }
	}
}

namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager
{
	using System;
	using System.Collections.Generic;

	/// <inheritdoc cref="ILockObjectsResult"/>>
	public class LockObjectsResult : ILockObjectsResult
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LockObjectsResult"/> class.
		/// </summary>
		/// <param name="lockInfosPerObjectId">A dictionary containing lock information for each object ID.</param>
		/// <param name="totalWaitingTime">The total time spent waiting to acquire the locks.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="lockInfosPerObjectId"/> is <c>null</c>.</exception>
		internal LockObjectsResult(Dictionary<string, ILockInfo> lockInfosPerObjectId, TimeSpan totalWaitingTime)
		{
			LockInfosPerObjectId = lockInfosPerObjectId ?? throw new ArgumentNullException(nameof(lockInfosPerObjectId));
			TotalWaitingTime = totalWaitingTime;
		}

		/// <inheritdoc cref="ILockObjectsResult.LockInfosPerObjectId"/>>
		public IReadOnlyDictionary<string, ILockInfo> LockInfosPerObjectId { get; }

		/// <inheritdoc cref="ILockObjectsResult.TotalWaitingTime"/>>
		public TimeSpan TotalWaitingTime { get; }

		/// <summary>
		/// Gets the lock result for a single object by its ID.
		/// </summary>
		/// <param name="objectId">The ID of the object to retrieve lock information for.</param>
		/// <returns>A <see cref="LockObjectResult"/> containing the lock information and waiting time for the specified object.</returns>
		/// <exception cref="KeyNotFoundException">Thrown when no lock information is found for the specified object ID.</exception>
		internal LockObjectResult GetSingleResult(string objectId)
		{
			if (LockInfosPerObjectId.TryGetValue(objectId, out var lockInfo))
			{
				return new LockObjectResult(lockInfo, TotalWaitingTime);
			}
			else
			{
				throw new KeyNotFoundException($"No lock information found for object ID '{objectId}'.");
			}
		}

		/// <summary>
		/// Creates an empty <see cref="LockObjectsResult"/> instance.
		/// </summary>
		/// <returns>An empty <see cref="LockObjectsResult"/>.</returns>
		public static LockObjectsResult Empty()
		{
			return new LockObjectsResult(new Dictionary<string, ILockInfo>(), TimeSpan.Zero);
		}
	}
}

namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Locking;

	/// <summary>
	/// Defines methods for managing lock objects within the Skyline Lock Manager.
	/// </summary>
	public interface ILockManager
	{
		/// <summary>
		/// Gets the default time span after which an automatically acquired lock is released.
		/// </summary>
		TimeSpan DefaultAutoLockReleaseTimeSpan { get; }

		/// <summary>
		/// Requests a lock on the specified object.
		/// </summary>
		/// <param name="lockObjectRequest">The request containing information about the object to lock.</param>
		/// <returns>
		/// An enumerable collection of <see cref="LockObjectResponse"/> objects representing the result of the lock request.
		/// </returns>
		IEnumerable<LockObjectResponse> RequestLock(LockObjectRequest lockObjectRequest);

		/// <summary>
		/// Unlocks all currently locked objects.
		/// </summary>
		/// <returns>
		/// A collection of object IDs that were unlocked.
		/// </returns>
		ICollection<string> UnlockAllObjects();

		/// <summary>
		/// Unlocks all objects whose lock has expired.
		/// </summary>
		void UnlockExpiredObjects();

		/// <summary>
		/// Unlocks the specified object, with an option to unlock linked objects.
		/// </summary>
		/// <param name="objectId">The ID of the object to unlock.</param>
		/// <param name="unlockLinkedObjects">If set to <c>true</c>, also unlocks objects linked to the specified object.</param>
		/// <returns>
		/// A collection of object IDs that were unlocked.
		/// </returns>
		ICollection<string> UnlockObject(string objectId, bool unlockLinkedObjects);
	}
}
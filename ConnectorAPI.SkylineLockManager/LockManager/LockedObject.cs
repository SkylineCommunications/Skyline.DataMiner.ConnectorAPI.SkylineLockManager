namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Represents an object that is currently locked by the Lock Manager.
	/// </summary>
	public class LockedObject : LockBaseInfo
	{
		/// <summary>
		/// Gets or sets the timestamp when the object was locked.
		/// </summary>
		public DateTime Timestamp { get; set; }

		/// <summary>
		/// Gets or sets the collection of linked object identifiers associated with this lock.
		/// </summary>
		public IReadOnlyCollection<string> LinkedObjectIds { get; set; }

		/// <summary>
		/// Gets or sets the timestamp when the lock will be automatically released.
		/// </summary>
		public DateTime AutoUnlockTimestamp { get; set; }
	}
}

namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Represents an object that is currently locked by the Lock Manager.
	/// </summary>
	public class LockedObject
	{
		/// <summary>
		/// Gets or sets the unique identifier of the locked object.
		/// </summary>
		public string ObjectId { get; set; }

		/// <summary>
		/// Gets or sets the description of the locked object.
		/// </summary>
		public string ObjectDescription { get; set; }

		/// <summary>
		/// Gets or sets information about the entity holding the lock.
		/// </summary>
		public string LockHolderInfo { get; set; }

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

		/// <summary>
		/// Gets or sets the priority of the lock. Lower values indicate higher priority.
		/// </summary>
		public int Priority { get; set; }
	}
}

namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Locking
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Represents a single object to be locked.
	/// </summary>
	public class LockObjectRequest
	{
		/// <summary>
		/// Gets or sets the id of the objects you want to lock.
		/// </summary>
		public string ObjectId { get; set; }

		/// <summary>
		/// Gets or sets the id of the objects you want to lock.
		/// </summary>
		public string ObjectDescription { get; set; }

		public string LockRequesterInfo { get; set; }

		/// <summary>
		/// Gets or sets collection of related objects of specified type.
		/// </summary>
		public List<LockObjectRequest> LinkedObjectRequests { get; set; } = new List<LockObjectRequest>();

		public DateTime? AutoUnlockTimestamp { get; set; }
	}
}

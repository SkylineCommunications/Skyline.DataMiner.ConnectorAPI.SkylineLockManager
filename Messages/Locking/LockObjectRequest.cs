namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Locking
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Represents a request to lock an object.
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

		/// <summary>
		/// Gets or sets information about the requester of the lock.
		/// </summary>
		public string LockRequesterInfo { get; set; }

		/// <summary>
		/// Gets or sets collection of requests to lock objects that are linked to this <see cref="LockObjectRequest"/>.
		/// </summary>
		public List<LockObjectRequest> LinkedObjectRequests { get; set; } = new List<LockObjectRequest>();

		/// <summary>
		/// Gets or sets the timestamp at which the element can automatically release the lock. If set to null, the default value defined in the element will be used.
		/// </summary>
		public DateTime? AutoUnlockTimestamp { get; set; }
	}
}

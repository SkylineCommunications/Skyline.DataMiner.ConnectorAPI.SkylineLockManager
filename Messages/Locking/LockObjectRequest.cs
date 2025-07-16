namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Locking
{
	using System;
	using System.Collections.Generic;
	using Newtonsoft.Json;

	/// <summary>
	/// Represents a request to lock an object.
	/// </summary>
	public class LockObjectRequest
	{
		/// <summary>
		/// Gets or sets the id of the objects you want to lock.
		/// </summary>
		[JsonProperty("ObjectId")]
		public string ObjectId { get; set; }

		/// <summary>
		/// Gets or sets the id of the objects you want to lock.
		/// </summary>
		[JsonProperty("ObjectDescription")]
		public string ObjectDescription { get; set; }

		/// <summary>
		/// Gets or sets information about the requester of the lock.
		/// </summary>
		[JsonProperty("LockRequesterInfo")]
		public string LockRequesterInfo { get; set; }

		/// <summary>
		/// Gets or sets collection of requests to lock objects that are linked to this <see cref="LockObjectRequest"/>.
		/// </summary>
		[JsonProperty("LinkedObjectRequests")]
		public List<LockObjectRequest> LinkedObjectRequests { get; set; } = new List<LockObjectRequest>();

		/// <summary>
		/// Gets or sets the timespan after which the element will automatically release the lock. If set to null, the default value defined in the element will be used.
		/// </summary>
		[JsonProperty("AutoUnlockTimeSpan")]
		public TimeSpan? AutoUnlockTimeSpan { get; set; }

		/// <summary>
		/// Gets or sets the priority level of the request.
		/// </summary>
		[JsonProperty("Priority")]
		public Priority Priority { get; set; } = Priority.Medium;
	}
}

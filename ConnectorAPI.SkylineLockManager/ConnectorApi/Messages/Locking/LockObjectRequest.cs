﻿namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Locking
{
	using System;
	using System.Collections.Generic;
	using Newtonsoft.Json;

	/// <summary>
	/// Represents a request to lock an object.
	/// </summary>
	public class LockObjectRequest : LockBaseInfo
	{
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
		/// Flattens the current request and its linked object requests into a single enumerable sequence.
		/// </summary>
		/// <remarks>This method returns the current request followed by all objects in the hierarchy of linked
		/// requests, traversing recursively through the <see cref="LinkedObjectRequests"/> collection. If there are no
		/// linked requests, only the current object is returned.</remarks>
		/// <returns>An <see cref="IEnumerable{T}"/> of <see cref="LockObjectRequest"/> objects, including the current object and all
		/// linked requests in a flattened structure.</returns>
		public IEnumerable<LockObjectRequest> Flatten()
		{
			yield return this;

			if (LinkedObjectRequests == null || LinkedObjectRequests.Count == 0)
			{
				yield break;
			}

			foreach (var linkedResponse in LinkedObjectRequests)
			{
				foreach (var response in linkedResponse.Flatten())
				{
					yield return response;
				}
			}
		}
	}
}

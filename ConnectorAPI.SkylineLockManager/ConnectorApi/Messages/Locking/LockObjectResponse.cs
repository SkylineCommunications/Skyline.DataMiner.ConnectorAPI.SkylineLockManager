namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Locking
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Represent the response to a <see cref="LockObjectRequest"/>.
	/// </summary>
	public class LockObjectResponse
	{
		/// <summary>
		/// Gets or sets the id of object for which the lock was requested.
		/// </summary>
		public string ObjectId { get; set; }

		/// <summary>
		/// Gets or sets information about the lock holder.
		/// </summary>
		public string LockHolderInfo { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the requested lock was granted or not.
		/// </summary>
		public bool LockIsGranted { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the lock is currently available for acquisition.
		/// </summary>
		public bool LockIsAvailable { get; set; }

		/// <summary>
		/// Gets or sets the timestamp at which the element will automatically release the lock.
		/// </summary>
		public DateTime AutoUnlockTimestamp { get; set; }

		/// <summary>
		/// Gets or sets the collection of responses to the lock requests of the linked objects.
		/// </summary>
		public ICollection<LockObjectResponse> LinkedObjectResponses { get; set; } = new List<LockObjectResponse>();

		/// <summary>
		/// Flattens the current response and its linked object responses into a single enumerable sequence.
		/// </summary>
		/// <remarks>This method returns the current response followed by all objects in the hierarchy of linked
		/// responses, traversing recursively through the <see cref="LinkedObjectResponses"/> collection. If there are no
		/// linked responses, only the current object is returned.</remarks>
		/// <returns>An <see cref="IEnumerable{T}"/> of <see cref="LockObjectResponse"/> objects, including the current object and all
		/// linked responses in a flattened structure.</returns>
		public IEnumerable<LockObjectResponse> Flatten()
		{
			yield return this;

			if (LinkedObjectResponses == null || LinkedObjectResponses.Count == 0)
			{
				yield break;
			}

			foreach (var linkedResponse in LinkedObjectResponses)
			{
				foreach (var response in linkedResponse.Flatten())
				{
					yield return response;
				}
			}
		}
	}
}

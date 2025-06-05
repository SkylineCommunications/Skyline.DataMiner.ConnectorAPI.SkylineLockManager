namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Locking
{
	using System;
	
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
		/// Gets or sets the timestamp at which the element will automatically release the lock.
		/// </summary>
		public DateTime AutoUnlockTimestamp { get; set; }
	}
}

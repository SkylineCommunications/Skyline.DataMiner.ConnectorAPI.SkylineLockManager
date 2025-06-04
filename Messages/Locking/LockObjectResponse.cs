using System;

namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Locking
{
	/// <summary>
	/// 
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

		public DateTime AutoUnlockTimestamp { get; set; }
	}
}

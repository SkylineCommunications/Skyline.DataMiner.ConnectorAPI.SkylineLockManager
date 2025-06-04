namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager
{
	using System;

	/// <summary>
	/// Output of lock request.
	/// </summary>
	public class LockInfo : ILockInfo
	{
		/// <summary>
		/// Gets or sets the Id of the requested object.
		/// </summary>
		public string ObjectId { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the lock is granted or not.
		/// </summary>
		public bool IsGranted { get; set; }

		/// <summary>
		/// Gets or sets the context from which the user was granted the lock to the requested object.
		/// </summary>
		public string LockHolderInfo { get; set; }

		/// <summary>
		/// Gets or sets the time indication on when the requested lock will be released.
		/// </summary>
		public DateTime AutoUnlockTimestamp { get; set; }
	}
}

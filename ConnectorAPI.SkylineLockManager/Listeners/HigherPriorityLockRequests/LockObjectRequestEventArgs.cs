namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners.HigherPriorityLockRequests
{
	using System;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Locking;

	public class LockObjectRequestEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LockObjectRequestEventArgs"/> class.
		/// </summary>
		public LockObjectRequestEventArgs(LockObjectRequest lockObjectRequest)
		{
			if (lockObjectRequest == null)
			{
				throw new ArgumentNullException(nameof(lockObjectRequest));
			}

			LockObjectRequest = lockObjectRequest;
		}

		public LockObjectRequest LockObjectRequest { get; }
	}
}

namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners.HigherPriorityLockRequests
{
	using System;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Locking;

	/// <summary>
	/// Provides data for the event that is triggered when a lock object request is made.
	/// </summary>
	/// <remarks>This class encapsulates the details of a lock object request, allowing event handlers  to access
	/// the associated <see cref="LockObjectRequest"/> instance.</remarks>
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

		/// <summary>
		/// Gets the lock object request.
		/// </summary>
		public LockObjectRequest LockObjectRequest { get; }
	}
}

namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager
{
	using System.Collections.Generic;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Locking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Unlocking;

	/// <summary>
	/// Represents an API to communicate with an element running the Skyline Lock Manager connector.
	/// </summary>
	public interface ILockManagerElement
	{
		/// <summary>
		/// Attempts to lock the given object.
		/// </summary>
		/// <param name="request">Represents the requests to lock object.</param>
		/// <returns>A <see cref="ILockInfo"/> object, containing info about the obtained lock.</returns>
		ILockInfo LockObject(LockObjectRequest request);

		/// <summary>
		/// Attempts to lock the given objects.
		/// </summary>
		/// <param name="requests">Represents the requests to lock objects.</param>
		/// <returns>A collection of <see cref="ILockInfo"/>, containing info about the obtained locks.</returns>
		IEnumerable<ILockInfo> LockObjects(IEnumerable<LockObjectRequest> requests);

		/// <summary>
		/// Attempts to unlock the given object.
		/// </summary>
		/// <param name="request">Represents the requests to unlock the object.</param>
		void UnlockObject(UnlockObjectRequest request);

		/// <summary>
		/// Attempts to unlock the given objects.
		/// </summary>
		/// <param name="requests">Represents the requests to unlock objects.</param>
		void UnlockObjects(IEnumerable<UnlockObjectRequest> requests);
	}
}

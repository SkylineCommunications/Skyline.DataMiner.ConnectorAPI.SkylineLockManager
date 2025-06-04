namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager
{
	using System.Collections.Generic;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Locking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Unlocking;

	/// <summary>
	/// 
	/// </summary>
	public interface ILockManagerElement
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		ILockInfo LockObject(LockObjectRequest request);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="requests"></param>
		/// <returns></returns>
		IEnumerable<ILockInfo> LockObjects(IEnumerable<LockObjectRequest> requests);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		void UnlockObject(UnlockObjectRequest request);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="requests"></param>
		void UnlockObjects(IEnumerable<UnlockObjectRequest> requests);
	}
}

namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager
{
	/// <summary>
	/// An interface representing info about unlocking an object. 
	/// </summary>
	public interface IUnlockInfo
	{
		/// <summary>
		/// Gets the ID of the object for which the unlock info is given.
		/// </summary>
		string ObjectId { get; }

		/// <summary>
		/// Gets a value indicating whether the lock was released or not.
		/// </summary>
		bool SuccessfullyUnlocked { get; }
	}
}

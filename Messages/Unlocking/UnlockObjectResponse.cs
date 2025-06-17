namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Unlocking
{
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager;

	/// <summary>
	/// Represent the response to <see cref="UnlockObjectRequest"/>.
	/// </summary>
	public class UnlockObjectResponse : IUnlockInfo
	{
		/// <summary>
		/// Gets or sets the id of the object for which the lock was released.
		/// </summary>
		public string ObjectId { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the lock was released or not.
		/// </summary>
		public bool SuccessfullyUnlocked { get; set; }
	}
}

namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Unlocking
{
	public class UnlockObjectResponse
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

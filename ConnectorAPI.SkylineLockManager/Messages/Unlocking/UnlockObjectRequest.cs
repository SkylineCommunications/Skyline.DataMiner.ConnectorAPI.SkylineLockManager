namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Unlocking
{
	/// <summary>
	/// Represents a request to unlock an individual object.
	/// </summary>
	public class UnlockObjectRequest
	{
		/// <summary>
		/// Gets or sets the id of the object you want to release the lock from.
		/// </summary>
		public string ObjectId { get; set; }

		/// <summary>
		/// Gets or sets whether locks for the related objects should be released.
		/// </summary>
		public bool ReleaseLinkedObjects { get; set; }
	}
}

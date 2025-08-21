namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager
{
	/// <summary>
	/// Represents the base information for a lock, including context, description, identifier, and priority.
	/// </summary>
	public interface ILockBaseInfo
	{
		/// <summary>
		/// Gets or sets the context information associated with the lock.
		/// </summary>
		string ContextInfo { get; set; }

		/// <summary>
		/// Gets or sets a description of the object.
		/// </summary>
		string ObjectDescription { get; set; }

		/// <summary>
		/// Gets or sets the unique identifier for the object.
		/// </summary>
		string ObjectId { get; set; }

		/// <summary>
		/// Gets or sets the priority level of the lock. Lower values indicate higher priority.
		/// </summary>
		int Priority { get; set; }
	}
}
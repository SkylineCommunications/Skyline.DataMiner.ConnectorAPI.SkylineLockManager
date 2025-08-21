namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager
{
	using Newtonsoft.Json;

	/// <summary>
	/// Represents information about a lockable object, including its identifier, description, context, and priority level.
	/// </summary>
	/// <remarks>This class is used to encapsulate metadata about a lockable object, such as its unique identifier, 
	/// a textual description, contextual information, and its priority level. The priority level determines  the
	/// importance of the lock, where lower values indicate higher priority.</remarks>
	public class LockBaseInfo : ILockBaseInfo
	{
		/// <inheritdoc/>
		[JsonProperty("ObjectId")]
		public string ObjectId { get; set; }

		/// <inheritdoc/>
		[JsonProperty("ObjectDescription")]
		public string ObjectDescription { get; set; }

		/// <inheritdoc/>
		[JsonProperty("ContextInfo")]
		public string ContextInfo { get; set; }

		/// <inheritdoc/>
		[JsonProperty("Priority")]
		public int Priority { get; set; }
	}
}

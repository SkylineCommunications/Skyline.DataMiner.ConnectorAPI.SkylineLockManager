namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager
{
	using Newtonsoft.Json;

	/// <summary>
	/// Represents information about a lockable object, including its identifier, description, context, and priority level.
	/// </summary>
	/// <remarks>This class is used to encapsulate metadata about a lockable object, such as its unique identifier, 
	/// a textual description, contextual information, and its priority level. The priority level determines  the
	/// importance of the lock, where lower values indicate higher priority.</remarks>
	public class LockBaseInfo
	{
		/// <summary>
		/// Gets or sets the id of the object.
		/// </summary>
		[JsonProperty("ObjectId")]
		public string ObjectId { get; set; }

		/// <summary>
		/// Gets or sets the description of the object.
		/// </summary>
		[JsonProperty("ObjectDescription")]
		public string ObjectDescription { get; set; }

		/// <summary>
		/// Gets or sets information about the context.
		/// </summary>
		[JsonProperty("ContextInfo")]
		public string ContextInfo { get; set; }

		/// <summary>
		/// Gets or sets the priority level of the lock. Lower values indicate higher priority.
		/// </summary>
		[JsonProperty("Priority")]
		public int Priority { get; set; }
	}
}

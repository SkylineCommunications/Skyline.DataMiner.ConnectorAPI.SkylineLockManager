namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners.HigherPriorityLockRequests
{
	using System;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager;

	/// <summary>
	/// Provides data for an event that includes an object ID and a priority value.
	/// </summary>
	/// <remarks>This class is typically used to pass information about an object and its associated priority in
	/// event-driven scenarios. The <see cref="ObjectId"/> property represents the unique identifier of the object, and the
	/// <see cref="Priority"/> property indicates its priority level.</remarks>
	public class ObjectIdAndPriorityEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectIdAndPriorityEventArgs"/> class.
		/// </summary>
		/// <param name="objectId">The object ID.</param>
		/// <param name="priority">The priority.</param>
		public ObjectIdAndPriorityEventArgs(string objectId, Priority priority)
		{
			if (string.IsNullOrWhiteSpace(objectId))
			{
				throw new ArgumentNullException(nameof(objectId));
			}

			ObjectId = objectId;
			Priority = priority;
		}

		/// <summary>
		/// Gets the object ID.
		/// </summary>
		public string ObjectId { get; }
		/// <summary>
		/// Gets the priority.
		/// </summary>
		public Priority Priority { get; }
	}
}

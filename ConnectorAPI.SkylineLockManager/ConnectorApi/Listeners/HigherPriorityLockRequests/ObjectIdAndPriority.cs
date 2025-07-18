namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners.HigherPriorityLockRequests
{
	using System;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager;

	/// <summary>
	/// Represents an object ID and a priority.
	/// </summary>
	public readonly struct ObjectIdAndPriority
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectIdAndPriority"/> struct.
		/// </summary>
		/// <param name="objectId"></param>
		/// <param name="priorityToCompareWith"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public ObjectIdAndPriority(string objectId, Priority priorityToCompareWith)
		{
			if (string.IsNullOrWhiteSpace(objectId))
			{
				throw new ArgumentNullException(nameof(objectId));
			}

			ObjectId = objectId;
			Priority = priorityToCompareWith;
		}

		/// <summary>
		/// Gets the object ID.
		/// </summary>
		public string ObjectId { get; }

		/// <summary>
		/// Gets the priority.
		/// </summary>
		public Priority Priority { get; }

		/// <summary>
		/// Returns a value indicating whether the current instance is equal to another instance of the same type.
		/// </summary>
		/// <param name="other">The other instance of the same type.</param>
		/// <returns>A boolean indicating if both instances are equal.</returns>
		public bool Equals(ObjectIdAndPriority other)
		{
			return ObjectId == other.ObjectId && Priority == other.Priority;
		}

		/// <summary>
		/// Returns a value indicating whether the current instance is equal to a specified object.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			return obj is ObjectIdAndPriority other && Equals(other);
		}

		/// <summary>
		/// Returns a hash code for the current instance.
		/// </summary>
		/// <returns>An integer representing the hash code for this instance.</returns>
		public override int GetHashCode()
		{
			return ObjectId.GetHashCode() * 397 ^ Priority.GetHashCode();
		}
	}
}

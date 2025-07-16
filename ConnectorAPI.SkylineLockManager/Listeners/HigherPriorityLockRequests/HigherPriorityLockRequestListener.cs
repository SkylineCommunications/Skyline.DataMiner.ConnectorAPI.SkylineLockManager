namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners.HigherPriorityLockRequests;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Locking;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Utils.SecureCoding.SecureSerialization.Json.Newtonsoft;

	/// <summary>
	/// Represents a listener that listens for higher priority lock requests.
	/// </summary>
	public class HigherPriorityLockRequestListener : HigherPriorityLockRequestListenerBase, IHigherPriorityLockRequestListener
	{
		private static readonly int HigherPrioLockRequests_ParameterId = 201;

		private readonly IDmsStandaloneParameter<string> parameter;

		/// <summary>
		/// Initializes a new instance of the <see cref="HigherPriorityLockRequestListener"/> class.
		/// </summary>
		/// <param name="element">The <see cref="IDmsElement"/> instance used to retrieve the parameter associated with higher priority lock
		/// requests. Cannot be <see langword="null"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="element"/> is <see langword="null"/>.</exception>
		public HigherPriorityLockRequestListener(IDmsElement element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			this.parameter = element.GetStandaloneParameter<string>(HigherPrioLockRequests_ParameterId);
		}

		/// <inheritdoc cref="Listener.StartMonitor"/>
		protected override void StartMonitor()
		{
			parameter.StartValueMonitor(sourceId, (change) =>
			{
				string serializedLockObjectRequest = change.Value;

				var lockObjectRequest = SecureNewtonsoftDeserialization.DeserializeObject<LockObjectRequest>(serializedLockObjectRequest) ?? throw new InvalidOperationException($"{serializedLockObjectRequest} could not be deserialized to a {nameof(LockObjectRequest)}.");

				string objectIdFromLockRequest = lockObjectRequest.ObjectId;
				var priorityFromLockRequest = lockObjectRequest.Priority;

				var lowerPriorities = Enum.GetValues(typeof(Priority)).Cast<Priority>().Where(prio => prio < priorityFromLockRequest).ToList();

				foreach (var lowerPriority in lowerPriorities)
				{
					var objectIdAndPriority = new ObjectIdAndPriority(objectIdFromLockRequest, lowerPriority);

					if (objectIdsAndPriorities.Contains(objectIdAndPriority))
					{
						InvokeHigherPriorityLockRequestReceived(lockObjectRequest);
						break; // No need to check further if we found a match
					}
				}
			});
		}

		/// <inheritdoc cref="Listener.StopMonitor"/>
		protected override void StopMonitor()
		{
			parameter.StopValueMonitor(sourceId);
		}
	}
}

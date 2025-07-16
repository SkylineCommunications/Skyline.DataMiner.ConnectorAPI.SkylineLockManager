namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners.HigherPriorityLockRequests;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Locking;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Core.DataMinerSystem.Common.Subscription.Monitors;
	using Skyline.DataMiner.Net.ToolsSpace.Collections;
	using Skyline.DataMiner.Utils.SecureCoding.SecureSerialization.Json.Newtonsoft;

	/// <summary>
	/// Represents a listener that listens for higher priority lock requests.
	/// </summary>
	public partial class HigherPriorityLockRequestListener : Listener, IHigherPriorityLockRequestListener
	{
		private static readonly int HigherPrioLockRequests_ParameterId = 201;

		private readonly ConcurrentHashSet<ObjectIdAndPriority> objectIdsAndPriorities = new ConcurrentHashSet<ObjectIdAndPriority>();

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

		/// <inheritdoc cref="IHigherPriorityLockRequestListener.HigherPriorityLockRequestReceived"/>
		public event EventHandler<LockObjectRequestEventArgs> HigherPriorityLockRequestReceived;

		/// <summary>
		/// Starts listening for higher priority lock requests for a specific object ID and priority.
		/// </summary>
		/// <param name="objectId"></param>
		/// <param name="priorityToCompareWith"></param>
		/// <returns></returns>
		public void ListenForLockRequestsWithHigherPriorityThan(string objectId, Priority priorityToCompareWith)
		{
			if (String.IsNullOrWhiteSpace(objectId))
			{
				throw new ArgumentNullException(nameof(objectId));
			}

			objectIdsAndPriorities.TryAdd(new ObjectIdAndPriority(objectId, priorityToCompareWith));

			if (!isListening)
			{
				StartListening();
			}
		}

		/// <summary>
		/// Stops listening for higher priority lock requests for a specific object ID and priority.
		/// </summary>
		/// <param name="objectId"></param>
		/// <param name="priorityToCompareWith"></param>
		public void StopListeningForLockRequestsWithHigherPriorityThan(string objectId, Priority priorityToCompareWith)
		{
			if (String.IsNullOrWhiteSpace(objectId))
			{
				throw new ArgumentNullException(nameof(objectId));
			}

			objectIdsAndPriorities.TryRemove(new ObjectIdAndPriority(objectId, priorityToCompareWith));

			if (objectIdsAndPriorities.IsEmpty && isListening)
			{
				StopListening();
			}
		}

		/// <inheritdoc cref="Listener.StartMonitor"/>
		protected override void StartMonitor()
		{
			Action<ParamValueChange<string>> reportHigherPrioLockRequest = (change) =>
			{
				string serializedLockObjectRequest = change.Value;

				var lockObjectRequest = SecureNewtonsoftDeserialization.DeserializeObject<LockObjectRequest>(serializedLockObjectRequest) ?? throw new InvalidOperationException($"{serializedLockObjectRequest} could not be deserialized to a {nameof(LockObjectRequest)}.");

				string objectIdFromLockRequest = lockObjectRequest.ObjectId;
				var priorityFromLockRequest = lockObjectRequest.Priority;

				var lowerPriorities = Enum.GetValues(typeof(Priority)).Cast<Priority>().Where(prio => (int)prio < (int)priorityFromLockRequest).ToList();

				foreach (var lowerPriority in lowerPriorities)
				{
					var objectIdAndPriority = new ObjectIdAndPriority(objectIdFromLockRequest, lowerPriority);

					if (objectIdsAndPriorities.Contains(objectIdAndPriority))
					{
						InvokeHigherPrioLockRequestReceived(lockObjectRequest);
						break; // No need to check further if we found a match
					}
				}
			};

			parameter.StartValueMonitor(sourceId, reportHigherPrioLockRequest);
		}

		/// <summary>
		/// Invokes the <see cref="HigherPriorityLockRequestReceived"/> event with the specified object ID and priority.
		/// </summary>
		private void InvokeHigherPrioLockRequestReceived(LockObjectRequest lockObjectRequest)
		{
			HigherPriorityLockRequestReceived?.Invoke(this, new LockObjectRequestEventArgs(lockObjectRequest));
		}

		/// <inheritdoc cref="Listener.StopMonitor"/>
		protected override void StopMonitor()
		{
			parameter.StopValueMonitor(sourceId);
		}

		/// <inheritdoc cref="Listener.Dispose(bool)"/>
		protected override void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					StopListening();
				}

				disposedValue = true;
			}
		}
	}
}

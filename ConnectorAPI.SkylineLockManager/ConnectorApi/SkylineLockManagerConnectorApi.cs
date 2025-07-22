namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Threading.Tasks;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners.HigherPriorityLockRequests;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners.Unlocks;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Locking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Unlocking;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Core.InterAppCalls.Common.CallBulk;
	using Skyline.DataMiner.Net;

	/// <inheritdoc cref="ISkylineLockManagerConnectorApi"/>
	public class SkylineLockManagerConnectorApi : ISkylineLockManagerConnectorApi
	{
		private readonly IInterAppHandler interAppHandler;
		private readonly IUnlockListener unlockListener;
		private readonly IHigherPriorityLockRequestListener higherPrioLockRequestListener;
		private readonly ILogger logger;

		private bool disposedValue;

		/// <summary>
		/// Name of the Connector with which this element is able to communicate.
		/// </summary>
		public static readonly string SkylineLockManager_ConnectorName = "Skyline Lock Manager";

		/// <summary>
		/// A collection containing all classes used in InterApp communication.
		/// </summary>
		public static IReadOnlyCollection<Type> InterAppKnownTypes { get; } = new List<Type>
		{
			typeof(IInterAppCall),
			typeof(LockObjectsRequestsMessage),
			typeof(LockObjectsResponsesMessage),
			typeof(UnlockObjectsRequestsMessage),
			typeof(FailureMessage),
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="SkylineLockManagerConnectorApi"/> class, providing the necessary
		/// handlers and listeners for inter-application communication and lock management.
		/// </summary>
		/// <param name="interAppHandler">The handler responsible for managing inter-application communication. This parameter cannot be <see
		/// langword="null"/>.</param>
		/// <param name="unlockListener">The listener that responds to unlock events. This parameter cannot be <see langword="null"/>.</param>
		/// <param name="higherPrioLockRequestListener">The listener that handles requests for higher-priority locks. This parameter cannot be <see langword="null"/>.</param>
		/// <param name="logger">The logger used for recording diagnostic and operational information. This parameter cannot be <see
		/// langword="null"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if any of the parameters are <see langword="null"/>.</exception>
		public SkylineLockManagerConnectorApi(IInterAppHandler interAppHandler, IUnlockListener unlockListener, IHigherPriorityLockRequestListener higherPrioLockRequestListener, ILogger logger = null)
		{
			this.interAppHandler = interAppHandler ?? throw new ArgumentNullException(nameof(interAppHandler));
			this.unlockListener = unlockListener ?? throw new ArgumentNullException(nameof(unlockListener));
			this.higherPrioLockRequestListener = higherPrioLockRequestListener ?? throw new ArgumentNullException(nameof(higherPrioLockRequestListener));
			this.logger = logger;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SkylineLockManagerConnectorApi"/> class.
		/// </summary>
		/// <param name="connection">Connection used to communicate with the Lock Manager element.</param>
		/// <param name="agentId">ID of the agent on which the Lock Manager element is hosted.</param>
		/// <param name="elementId">ID of the Lock Manager element.</param>
		/// <param name="logger">Object that is used to log info about locks.</param>
		/// <exception cref="ArgumentNullException">Thrown when the provided connection or the element is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when provided element id or agent id is negative.</exception>
		/// <exception cref="InvalidOperationException">Thrown when described element is inactive.</exception>
		public SkylineLockManagerConnectorApi(IConnection connection, int agentId, int elementId, ILogger logger = null)
		{
			var element = connection.GetDms().GetElement(new DmsElementId(agentId, elementId)) ?? throw new ArgumentException($"Unable to find an element with ID {agentId}\\{elementId}", nameof(elementId));

			if (element.State != ElementState.Active)
			{
				throw new InvalidOperationException($"The element with ID {agentId}\\{elementId} is not active.");
			}

			interAppHandler = new InterAppHandler(connection, element, logger);
			unlockListener = new UnlockListener(element, logger);
			higherPrioLockRequestListener = new HigherPriorityLockRequestListener(element);
			this.logger = logger;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SkylineLockManagerConnectorApi"/> class.
		/// </summary>
		/// <param name="connection">Connection used to communicate with the Lock Manager element.</param>
		/// <param name="elementName">Name of the Lock Manager element.</param>
		/// <param name="logger">Object that is used to log info about locks.</param>
		/// <exception cref="ArgumentNullException">Thrown when the provided connection or the element is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when provided element id or agent id is negative.</exception>
		/// <exception cref="InvalidOperationException">Thrown when described element is inactive.</exception>
		public SkylineLockManagerConnectorApi(IConnection connection, string elementName, ILogger logger = null)
		{
			var element = connection.GetDms().GetElement(elementName) ?? throw new ArgumentException($"Unable to find an element with Name {elementName}", nameof(elementName));

			if (element.State != ElementState.Active)
			{
				throw new InvalidOperationException($"The element with name {elementName} is not active.");
			}

			interAppHandler = new InterAppHandler(connection, element, logger);
			unlockListener = new UnlockListener(element, logger);
			higherPrioLockRequestListener = new HigherPriorityLockRequestListener(element);
			this.logger = logger;
		}

		/// <inheritdoc cref="IHigherPriorityLockRequestListener.HigherPriorityLockRequestReceived"/>
		public event EventHandler<LockObjectRequestEventArgs> HigherPriorityLockRequestReceived
		{
			add
			{
				lock (higherPrioLockRequestListener)
				{
					higherPrioLockRequestListener.HigherPriorityLockRequestReceived += value;
				}
			}
			remove
			{
				lock (higherPrioLockRequestListener)
				{
					higherPrioLockRequestListener.HigherPriorityLockRequestReceived -= value;
				}
			}
		}

		/// <inheritdoc cref="IHigherPriorityLockRequestListener.ListenForLockRequestsWithHigherPriorityThan(ObjectIdAndPriority[])"/>
		public void ListenForLockRequestsWithHigherPriorityThan(params ObjectIdAndPriority[] objectIdAndPriorities)
		{
			higherPrioLockRequestListener.ListenForLockRequestsWithHigherPriorityThan(objectIdAndPriorities);
		}

		/// <inheritdoc cref="IHigherPriorityLockRequestListener.StopListeningForLockRequestsWithHigherPriorityThan(ObjectIdAndPriority[])"/>
		public void StopListeningForLockRequestsWithHigherPriorityThan(params ObjectIdAndPriority[] objectIdAndPriorities)
		{
			higherPrioLockRequestListener.StopListeningForLockRequestsWithHigherPriorityThan(objectIdAndPriorities);
		}

		/// <inheritdoc cref="ISkylineLockManagerConnectorApi.LockObject(LockObjectRequest, TimeSpan?)"/>
		/// <exception cref="ArgumentNullException"/>
		public ILockObjectsResult LockObject(LockObjectRequest request, TimeSpan? maxWaitingTime = null)
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			return LockObjectsInternal(new[] { request }, maxWaitingTime);
		}

		/// <inheritdoc cref="ISkylineLockManagerConnectorApi.LockObjects(IEnumerable{LockObjectRequest}, TimeSpan?)"/>
		/// <exception cref="ArgumentNullException"/>
		public ILockObjectsResult LockObjects(IEnumerable<LockObjectRequest> requests, TimeSpan? maxWaitingTime = null)
		{
			return LockObjectsInternal(requests, maxWaitingTime);
		}

		/// <inheritdoc cref="ISkylineLockManagerConnectorApi.UnlockObject(UnlockObjectRequest)"/>
		/// <exception cref="ArgumentNullException"/>
		public void UnlockObject(UnlockObjectRequest request)
		{
			UnlockObjects(new[] { request });
		}

		/// <inheritdoc cref="ISkylineLockManagerConnectorApi.UnlockObjects(IEnumerable{UnlockObjectRequest})"/>
		/// <exception cref="ArgumentNullException"/>
		public void UnlockObjects(IEnumerable<UnlockObjectRequest> requests)
		{
			if (requests is null) throw new ArgumentNullException(nameof(requests));

			var requestsList = requests.ToList();

			if (!requestsList.Any()) return;

			var message = new UnlockObjectsRequestsMessage
			{
				Requests = requestsList,
			};

			interAppHandler.SendUnlockObjectsRequestsMessage(message);

			Log($"Unlocked objects {string.Join(", ", requestsList.Select(r => r.ObjectId))}");
		}

		/// <inheritdoc cref="IDisposable.Dispose"/>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases the resources used by the current instance of the class.
		/// </summary>
		/// <remarks>This method should be called when the instance is no longer needed to free up resources.  It
		/// ensures that any managed resources are properly disposed of.  For derived classes, override <see
		/// cref="Dispose(bool)"/> to release additional resources.</remarks>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					unlockListener.Dispose();
					higherPrioLockRequestListener.Dispose();
				}

				disposedValue = true;
			}
		}

		private LockObjectsResult LockObjectsInternal(IEnumerable<LockObjectRequest> requests, TimeSpan? maxWaitingTime)
		{
			if (requests == null)
			{
				throw new ArgumentNullException(nameof(requests));
			}

			var requestsList = requests.ToList();
			if (!requestsList.Any())
			{
				return LockObjectsResult.Empty();
			}

			List<LockObjectResponse> lockObjectResponses;
			TimeSpan totalWaitingTime = TimeSpan.Zero;

			if (maxWaitingTime.HasValue)
			{
				lockObjectResponses = LockObjectsWithWait(requestsList, maxWaitingTime.Value, out totalWaitingTime);
			}
			else
			{
				lockObjectResponses = SendLockObjectRequests(requestsList);
			}

			var lockInfos = lockObjectResponses.SelectMany(response => response.Flatten()).Select(response => (ILockInfo)new LockInfo
			{
				ObjectId = response.ObjectId,
				LockHolderInfo = response.LockHolderInfo,
				IsGranted = response.LockIsGranted,
				AutoUnlockTimestamp = response.AutoUnlockTimestamp,
			}).ToList();

			return new LockObjectsResult(lockInfos.ToDictionary(li => li.ObjectId), totalWaitingTime);
		}

		private List<LockObjectResponse> LockObjectsWithWait(ICollection<LockObjectRequest> requests, TimeSpan maxWaitingTime, out TimeSpan totalWaitingTime)
		{
			if (requests == null)
			{
				throw new ArgumentNullException(nameof(requests));
			}

			if (maxWaitingTime <= TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException(nameof(maxWaitingTime), "Max waiting time must be greater than zero.");
			}

			var lockObjectResponses = SendLockObjectRequests(requests);

			var objectIdsForNonAvailableLocks = lockObjectResponses.SelectMany(response => response.Flatten()).Where(response => !response.LockIsAvailable).Select(response => response.ObjectId).ToList();

			totalWaitingTime = TimeSpan.Zero;

			while (objectIdsForNonAvailableLocks.Count > 0 && totalWaitingTime < maxWaitingTime)
			{
				var tasksToWaitForUnlock = unlockListener.StartListeningForUnlocks(objectIdsForNonAvailableLocks).ToArray();

				Log($"Start waiting until objects '{String.Join(", ", objectIdsForNonAvailableLocks)}' are unlocked");

				var remainingWaitingTime = maxWaitingTime - totalWaitingTime;
				var stopwatch = Stopwatch.StartNew();

				bool objectsGotUnlockedAfterWaiting = Task.WaitAll(tasksToWaitForUnlock, remainingWaitingTime);

				stopwatch.Stop();
				totalWaitingTime += stopwatch.Elapsed;

				if (objectsGotUnlockedAfterWaiting)
				{
					Log($"Finished waiting until objects '{String.Join(", ", objectIdsForNonAvailableLocks)}' are unlocked, took {stopwatch.Elapsed}");

					lockObjectResponses = SendLockObjectRequests(requests);
					objectIdsForNonAvailableLocks = lockObjectResponses.SelectMany(response => response.Flatten()).Where(response => !response.LockIsAvailable).Select(response => response.ObjectId).ToList();
				}
				else
				{
					Log($"Timed-out while waiting until objects '{String.Join(", ", objectIdsForNonAvailableLocks)}' are unlocked.");
					break;
				}
			}

			unlockListener.StopListeningForUnlocks(objectIdsForNonAvailableLocks);

			return lockObjectResponses;
		}

		private List<LockObjectResponse> SendLockObjectRequests(IEnumerable<LockObjectRequest> requests)
		{
			if (requests is null) throw new ArgumentNullException(nameof(requests));

			var requestsList = requests.ToList();

			if (!requestsList.Any()) return new List<LockObjectResponse>();

			var lockObjectsRequestsMessage = new LockObjectsRequestsMessage
			{
				Requests = requestsList,
			};

			Log($"Requesting locks for {string.Join(", ", requestsList.Select(r => r.ObjectId))}");

			var responseMessage = interAppHandler.SendLockObjectsRequestsMessage(lockObjectsRequestsMessage);

			return responseMessage.Responses.ToList();
		}

		private void Log(string message)
		{
			string nameOfMethod = new StackTrace().GetFrame(1).GetMethod().Name;

			logger?.Log(nameof(SkylineLockManagerConnectorApi), nameOfMethod, message);
		}
	}
}
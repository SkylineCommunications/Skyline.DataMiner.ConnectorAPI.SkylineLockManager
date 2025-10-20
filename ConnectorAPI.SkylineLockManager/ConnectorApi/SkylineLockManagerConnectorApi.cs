namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using Microsoft.Extensions.Logging;
	using Microsoft.Extensions.Logging.Abstractions;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Messages.Locking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Messages.Unlocking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Sending;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners.HigherPriorityLockRequests;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners.Unlocks;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Net;

	/// <inheritdoc cref="ISkylineLockManagerConnectorApi"/>
	public partial class SkylineLockManagerConnectorApi : ISkylineLockManagerConnectorApi
	{
		private static readonly ActivitySource activitySource = new ActivitySource("Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.SkylineLockManagerConnectorApi");

		private readonly ILogger<SkylineLockManagerConnectorApi> logger;

		private readonly IInterAppSender interAppHandler;
		private readonly IUnlockListener unlockListener;
		private readonly IHigherPriorityLockRequestListener higherPrioLockRequestListener;

		private bool disposedValue;

		/// <summary>
		/// Name of the Connector with which this element is able to communicate.
		/// </summary>
		public static readonly string SkylineLockManager_ConnectorName = "Skyline Lock Manager";

		/// <summary>
		/// Initializes a new instance of the <see cref="SkylineLockManagerConnectorApi"/> class, providing the necessary
		/// handlers and listeners for inter-application communication and lock management.
		/// </summary>
		/// <param name="interAppHandler">The handler responsible for managing inter-application communication. This parameter cannot be <see
		/// langword="null"/>.</param>
		/// <param name="unlockListener">The listener that responds to unlock events. This parameter cannot be <see langword="null"/>.</param>
		/// <param name="higherPrioLockRequestListener">The listener that handles requests for higher-priority locks. This parameter cannot be <see langword="null"/>.</param>
		/// <param name="loggerFactory">An optional logger factory.</param>
		/// <exception cref="ArgumentNullException">Thrown if any of the parameters are <see langword="null"/>.</exception>
		internal SkylineLockManagerConnectorApi(IInterAppSender interAppHandler, IUnlockListener unlockListener, IHigherPriorityLockRequestListener higherPrioLockRequestListener, ILoggerFactory loggerFactory = null)
		{
			loggerFactory = loggerFactory ?? new NullLoggerFactory();
			this.logger = loggerFactory.CreateLogger<SkylineLockManagerConnectorApi>();

			this.interAppHandler = interAppHandler ?? throw new ArgumentNullException(nameof(interAppHandler));
			this.unlockListener = unlockListener ?? throw new ArgumentNullException(nameof(unlockListener));
			this.higherPrioLockRequestListener = higherPrioLockRequestListener ?? throw new ArgumentNullException(nameof(higherPrioLockRequestListener));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SkylineLockManagerConnectorApi"/> class.
		/// </summary>
		/// <param name="connection">Connection used to communicate with the Lock Manager element.</param>
		/// <param name="agentId">ID of the agent on which the Lock Manager element is hosted.</param>
		/// <param name="elementId">ID of the Lock Manager element.</param>
		/// <param name="loggerFactory">An optional logger factory.</param>
		/// <exception cref="ArgumentNullException">Thrown when the provided connection or the element is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when provided element id or agent id is negative.</exception>
		/// <exception cref="InvalidOperationException">Thrown when described element is inactive.</exception>
		public SkylineLockManagerConnectorApi(IConnection connection, int agentId, int elementId, ILoggerFactory loggerFactory = null)
		{
			var element = connection.GetDms().GetElement(new DmsElementId(agentId, elementId)) ?? throw new ArgumentException($"Unable to find an element with ID {agentId}\\{elementId}", nameof(elementId));

			if (element.State != ElementState.Active)
			{
				throw new InvalidOperationException($"The element with ID {agentId}\\{elementId} is not active.");
			}

			loggerFactory = loggerFactory ?? new NullLoggerFactory();
			this.logger = loggerFactory.CreateLogger<SkylineLockManagerConnectorApi>();

			interAppHandler = new InterAppSender(connection, element, loggerFactory.CreateLogger<InterAppSender>());
			unlockListener = new UnlockListener(element, loggerFactory.CreateLogger<UnlockListener>());
			higherPrioLockRequestListener = new HigherPriorityLockRequestListener(element, loggerFactory.CreateLogger<HigherPriorityLockRequestListener>());
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SkylineLockManagerConnectorApi"/> class.
		/// </summary>
		/// <param name="connection">Connection used to communicate with the Lock Manager element.</param>
		/// <param name="elementName">Name of the Lock Manager element.</param>
		/// <param name="loggerFactory">An optional logger factory.</param>
		/// <exception cref="ArgumentNullException">Thrown when the provided connection or the element is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when provided element id or agent id is negative.</exception>
		/// <exception cref="InvalidOperationException">Thrown when described element is inactive.</exception>
		public SkylineLockManagerConnectorApi(IConnection connection, string elementName, ILoggerFactory loggerFactory = null)
		{
			var element = connection.GetDms().GetElement(elementName) ?? throw new ArgumentException($"Unable to find an element with Name {elementName}", nameof(elementName));

			if (element.State != ElementState.Active)
			{
				throw new InvalidOperationException($"The element with name {elementName} is not active.");
			}

			loggerFactory = loggerFactory ?? new NullLoggerFactory();
			this.logger = loggerFactory.CreateLogger<SkylineLockManagerConnectorApi>();

			interAppHandler = new InterAppSender(connection, element, loggerFactory.CreateLogger<InterAppSender>());
			unlockListener = new UnlockListener(element, loggerFactory.CreateLogger<UnlockListener>());
			higherPrioLockRequestListener = new HigherPriorityLockRequestListener(element, loggerFactory.CreateLogger<HigherPriorityLockRequestListener>());
		}

		/// <inheritdoc/>
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

		/// <inheritdoc/>
		public void ListenForLockRequestsWithHigherPriorityThan(string objectId, int priority)
		{
			ListenForLockRequestsWithHigherPriorityThan(new Dictionary<string, ICollection<int>>
			{
				{ objectId, new[] { priority } }
			});
		}

		/// <inheritdoc/>
		public void ListenForLockRequestsWithHigherPriorityThan(ICollection<KeyValuePair<string, ICollection<int>>> objectIdsAndPrioritiesToStartListeningFor)
		{
			using (activitySource.StartActivity())
			{
				higherPrioLockRequestListener.ListenForLockRequestsWithHigherPriorityThan(objectIdsAndPrioritiesToStartListeningFor);
			}
		}

		/// <inheritdoc/>
		public void StopListeningForLockRequestsWithHigherPriorityThan(string objectId, int priority)
		{
			StopListeningForLockRequestsWithHigherPriorityThan(new Dictionary<string, ICollection<int>>
			{
				{ objectId, new[] { priority } }
			});
		}

		/// <inheritdoc/>
		public void StopListeningForLockRequestsWithHigherPriorityThan(ICollection<KeyValuePair<string, ICollection<int>>> objectIdsAndPrioritiesToStopListeningFor)
		{
			using (activitySource.StartActivity())
			{
				higherPrioLockRequestListener.StopListeningForLockRequestsWithHigherPriorityThan(objectIdsAndPrioritiesToStopListeningFor);
			}
		}

		/// <inheritdoc/>
		/// <exception cref="ArgumentNullException"/>
		public ILockObjectsResult LockObject(LockObjectRequest request, TimeSpan? maxWaitingTime = null)
		{	
			ValidateRequests(request);

			return LockObjectsInternal(new[] { request }, maxWaitingTime);		
		}

		/// <inheritdoc/>
		/// <exception cref="ArgumentNullException"/>
		public ILockObjectsResult LockObjects(IEnumerable<LockObjectRequest> requests, TimeSpan? maxWaitingTime = null)
		{
			ValidateRequests(requests.ToArray());

			return LockObjectsInternal(requests, maxWaitingTime);
		}

		/// <inheritdoc/>
		/// <exception cref="ArgumentNullException"/>
		public void UnlockObject(UnlockObjectRequest request)
		{
			UnlockObjects(new[] { request });
		}

		/// <inheritdoc/>
		/// <exception cref="ArgumentNullException"/>
		public void UnlockObjects(IEnumerable<UnlockObjectRequest> requests)
		{
			if (requests is null) throw new ArgumentNullException(nameof(requests));

			using (logger.BeginScope(requests.Select(req => new KeyValuePair<string, object>("ObjectId", req?.ObjectId))))
			{
				var requestsList = requests.ToList();

				if (!requestsList.Any()) return;

				var message = new UnlockObjectsRequestsMessage
				{
					Requests = requestsList,
				};

				interAppHandler.SendUnlockObjectsRequestsMessage(message);

				logger.LogInformation(LogEvents.UnlockRequest, "Unlocked objects {ObjectIds}", String.Join(", ", requestsList.Select(r => r.ObjectId)));
			}
		}

		/// <inheritdoc/>
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
			using (activitySource.StartActivity())
			{
				if (requests == null)
				{
					throw new ArgumentNullException(nameof(requests));
				}

				using (logger.BeginScope(requests.Select(req => new KeyValuePair<string, object>("ObjectId", req?.ObjectId))))
				{
					var requestsList = requests.ToList();
					if (!requestsList.Any())
					{
						return LockObjectsResult.Empty();
					}

					List<LockObjectResponse> lockObjectResponses;
					TimeSpan totalWaitingTime = TimeSpan.Zero;

					if (maxWaitingTime.HasValue && maxWaitingTime.Value > TimeSpan.Zero)
					{
						lockObjectResponses = LockObjectsWithWait(requestsList, maxWaitingTime.Value, out totalWaitingTime);
					}
					else
					{
						lockObjectResponses = SendLockObjectRequests(requestsList);
					}

					var lockInfosPerObjectId = new Dictionary<string, ILockInfo>();

					var requestsPerObjectId = requestsList.SelectMany(req => req.Flatten()).ToDictionary(req => req.ObjectId);

					foreach (var response in lockObjectResponses.SelectMany(lor => lor.Flatten()))
					{
						var matchingRequest = requestsPerObjectId[response.ObjectId];

						lockInfosPerObjectId.Add(response.ObjectId, new LockInfo
						{
							ObjectId = response.ObjectId,
							ObjectDescription = matchingRequest.ObjectDescription,
							ContextInfo = response.LockHolderInfo,
							IsGranted = response.LockIsGranted,
							AutoUnlockTimestamp = response.AutoUnlockTimestamp,
							Priority = matchingRequest.Priority
						});
					}

					return new LockObjectsResult(lockInfosPerObjectId, totalWaitingTime);
				}
			}
		}

		private void ValidateRequests(params LockObjectRequest[] requests)
		{
			if (requests == null)
			{
				throw new ArgumentNullException(nameof(requests));
			}

			var objectIds = requests.SelectMany(req => req.Flatten()).Select(req => req.ObjectId).ToList();

			var distinctObjectIds = objectIds.Distinct().ToList();

			if (objectIds.Count != distinctObjectIds.Count)
			{
				throw new ArgumentException("The provided requests contain duplicate object IDs.", nameof(requests));
			}
		}

		private List<LockObjectResponse> LockObjectsWithWait(ICollection<LockObjectRequest> requests, TimeSpan maxWaitingTime, out TimeSpan totalWaitingTime)
		{
			return Helper.LockObjectsWithWait(this, requests, maxWaitingTime, out totalWaitingTime);
		}

		private LockObjectResponse SendLockObjectRequest(LockObjectRequest request)
		{
			if (request == null) throw new ArgumentNullException(nameof(request));

			return SendLockObjectRequests(new[] { request }).Single();
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

			logger.LogInformation(LogEvents.LockRequest, "Requesting locks for {ObjectIds}", String.Join(", ", requestsList.Select(r => r.ObjectId)).ToList());

			var responseMessage = interAppHandler.SendLockObjectsRequestsMessage(lockObjectsRequestsMessage);

			return responseMessage.Responses.ToList();
		}
	}
}
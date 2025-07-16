namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners.HigherPriorityLockRequests;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners.Unlocks;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Locking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Unlocking;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Core.InterAppCalls.Common.CallBulk;
	using Skyline.DataMiner.Net;

	/// <inheritdoc cref="ILockManagerElement"/>
	public class LockManagerElement : ILockManagerElement
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
		/// Initializes a new instance of the <see cref="LockManagerElement"/> class. To be used by unit tests only.
		/// </summary>
		public LockManagerElement(IConnection connection, IDmsElement element, IUnlockListener unlockListener = null, IInterAppHandler interAppHandler = null, IHigherPriorityLockRequestListener higherPrioLockRequestListener = null, ILogger logger = null)
		{
			if (element.State != ElementState.Active) throw new InvalidOperationException($"Element {element.Name} is not active");

			this.higherPrioLockRequestListener = higherPrioLockRequestListener ?? new HigherPriorityLockRequestListener(element);
			this.interAppHandler = interAppHandler ?? new InterAppHandler(connection, element, logger);
			this.unlockListener = unlockListener ?? new UnlockListener(element);
			this.logger = logger;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LockManagerElement"/> class.
		/// </summary>
		/// <param name="connection">Connection used to communicate with the Lock Manager element.</param>
		/// <param name="agentId">ID of the agent on which the Lock Manager element is hosted.</param>
		/// <param name="elementId">ID of the Lock Manager element.</param>
		/// <param name="logger">Object that is used to log info about locks.</param>
		/// <exception cref="ArgumentNullException">Thrown when the provided connection or the element is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when provided element id or agent id is negative.</exception>
		/// <exception cref="InvalidOperationException">Thrown when described element is inactive.</exception>
		public LockManagerElement(IConnection connection, int agentId, int elementId, ILogger logger = null) : this(connection, connection.GetDms().GetElement(new DmsElementId(agentId, elementId)) ?? throw new ArgumentException($"Unable to find an element with ID {agentId}\\{elementId}", nameof(elementId)), logger: logger)
		{
			
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LockManagerElement"/> class.
		/// </summary>
		/// <param name="connection">Connection used to communicate with the Lock Manager element.</param>
		/// <param name="elementName">Name of the Lock Manager element.</param>
		/// <param name="logger">Object that is used to log info about locks.</param>
		/// <exception cref="ArgumentNullException">Thrown when the provided connection or the element is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when provided element id or agent id is negative.</exception>
		/// <exception cref="InvalidOperationException">Thrown when described element is inactive.</exception>
		public LockManagerElement(IConnection connection, string elementName, ILogger logger = null) : this(connection, connection.GetDms().GetElement(elementName) ?? throw new ArgumentException($"Unable to find an element with Name {elementName}", nameof(elementName)), logger: logger)
		{

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

		/// <inheritdoc cref="IHigherPriorityLockRequestListener.ListenForLockRequestsWithHigherPriorityThan(string, Priority)"/>
		public void ListenForLockRequestsWithHigherPriorityThan(string objectId, Priority priorityToCompareWith)
		{
			higherPrioLockRequestListener.ListenForLockRequestsWithHigherPriorityThan(objectId, priorityToCompareWith);
		}

		/// <inheritdoc cref="IHigherPriorityLockRequestListener.StopListeningForLockRequestsWithHigherPriorityThan(string, Priority)"/>
		public void StopListeningForLockRequestsWithHigherPriorityThan(string objectId, Priority priorityToCompareWith)
		{
			higherPrioLockRequestListener.StopListeningForLockRequestsWithHigherPriorityThan(objectId, priorityToCompareWith);
		}

		/// <inheritdoc cref="ILockManagerElement.LockObject(LockObjectRequest, TimeSpan?)"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="InvalidOperationException"/>
		public ILockInfo LockObject(LockObjectRequest request, TimeSpan? maxWaitingTime = null)
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			if (maxWaitingTime.HasValue)
			{
				return LockObjectWithWait(request, maxWaitingTime.Value);
			}
			else
			{
				return LockObjectWithoutWait(request);
			}		
		}

		/// <inheritdoc cref="ILockManagerElement.LockObjects(IEnumerable{LockObjectRequest})"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="InvalidOperationException"/>
		public IEnumerable<ILockInfo> LockObjects(IEnumerable<LockObjectRequest> requests)
		{
			if (requests is null) throw new ArgumentNullException(nameof(requests));

			var requestsList = requests.ToList();

			if (!requestsList.Any()) return Enumerable.Empty<ILockInfo>();

			var lockObjectsRequestsMessage = new LockObjectsRequestsMessage
			{
				Requests = requestsList,
			};

			Log($"Requesting locks for {String.Join(", ", requestsList.Select(r => r.ObjectId))}");

			var responseMessage = interAppHandler.SendMessageWithResponse<LockObjectsResponsesMessage>(lockObjectsRequestsMessage);

			return responseMessage.Responses.Select(x => new LockInfo
			{
				LockHolderInfo = x.LockHolderInfo,
				IsGranted = x.LockIsGranted,
				ObjectId = x.ObjectId,
				AutoUnlockTimestamp = x.AutoUnlockTimestamp,
			}).ToList();
		}

		/// <inheritdoc cref="ILockManagerElement.UnlockObject(UnlockObjectRequest)"/>
		/// <exception cref="ArgumentNullException"/>
		public void UnlockObject(UnlockObjectRequest request)
		{
			UnlockObjects(new[] { request });
		}

		/// <inheritdoc cref="ILockManagerElement.UnlockObjects(IEnumerable{UnlockObjectRequest})"/>
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

			interAppHandler.SendMessageWithoutResponse(message);

			Log($"Unlocked objects {String.Join(", ", requestsList.Select(r => r.ObjectId))}");
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

		private ILockInfo LockObjectWithWait(LockObjectRequest request, TimeSpan maxWaitingTime)
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			if (maxWaitingTime <= TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException(nameof(maxWaitingTime), "Max waiting time must be greater than zero.");
			}

			var taskToWaitForUnlock = unlockListener.StartListeningForUnlock(request.ObjectId);

			var lockInfo = LockObject(request);

			if (lockInfo.IsGranted)
			{
				unlockListener.StopListeningForUnlock(request.ObjectId);

				return lockInfo;
			}

			bool objectGotUnlockedAfterWaiting = taskToWaitForUnlock.Wait(maxWaitingTime);

			unlockListener.StopListeningForUnlock(request.ObjectId);

			if (objectGotUnlockedAfterWaiting)
			{
				lockInfo = LockObject(request);
			}

			return lockInfo;
		}

		private ILockInfo LockObjectWithoutWait(LockObjectRequest request)
		{
			return LockObjects(new[] { request }).Single();
		}

		private void Log(string message)
		{
			string nameOfMethod = new StackTrace().GetFrame(1).GetMethod().Name;

			logger?.Log(nameof(LockManagerElement), nameOfMethod, message);
		}
	}
}
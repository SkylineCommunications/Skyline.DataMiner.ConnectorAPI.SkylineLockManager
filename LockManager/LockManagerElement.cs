namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Locking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Unlocking;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Core.InterAppCalls.Common.CallBulk;
	using Skyline.DataMiner.Core.InterAppCalls.Common.CallSingle;
	using Skyline.DataMiner.Net;

	/// <summary>
	/// Represents a Skyline Lock Manager element in DataMiner and exposes methods to lock and unlock objects.
	/// </summary>
	public class LockManagerElement : ILockManagerElement
	{
		private static readonly int InterAppReceive_ParameterId = 9000000;
		private static readonly int InterAppTimeout_ParameterId = 100;

		private readonly IConnection connection;
		private readonly IDmsElement element;
		private readonly ILogger logger;

		private readonly Lazy<TimeSpan> timeout;

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
		public LockManagerElement(IConnection connection, int agentId, int elementId, ILogger logger = null)
		{
			this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
			this.logger = logger;
			this.timeout = new Lazy<TimeSpan>(GetTimeout);

			element = connection.GetDms().GetElement(new DmsElementId(agentId, elementId)) ?? throw new ArgumentException($"Unable to find an element with ID {agentId}\\{elementId}", nameof(elementId));

			if (element.State != ElementState.Active) throw new InvalidOperationException($"Element {element.Name} is not active");
		}

		private TimeSpan Timeout => timeout.Value;

		/// <summary>
		/// A collection containing all classes used in InterApp communication.
		/// </summary>
		public static IReadOnlyCollection<Type> InterAppKnownTypes { get; } = new List<Type>
		{
			typeof(IInterAppCall),
			typeof(LockObjectsRequestsMessage),
			typeof(LockObjectsResponsesMessage),
			typeof(UnlockObjectsRequestsMessage),
			typeof(UnlockObjectsResponsesMessage),
			typeof(FailureMessage),
		};

		/// <summary>
		/// Locks object of a specific type.
		/// </summary>
		/// <param name="request">ID of the object to be locked.</param>
		/// <returns>LockInfo object containing information on the requested lock.</returns>
		/// <exception cref="InvalidOperationException">If something went wrong during InterApp communication.</exception>
		public ILockInfo LockObject(LockObjectRequest request)
		{
			return LockObjects(new[] { request }).Single();
		}

		/// <summary>
		/// Locks objects of a specific type.
		/// </summary>
		/// <param name="requests">IDs of the objects to be locked.</param>
		/// <returns>LockInfo object containing information on the requested lock.</returns>
		/// <exception cref="InvalidOperationException">If something went wrong during InterApp communication.</exception>
		public IEnumerable<ILockInfo> LockObjects(IEnumerable<LockObjectRequest> requests)
		{
			if (requests is null) throw new ArgumentNullException(nameof(requests));
			if (!requests.Any()) return Enumerable.Empty<ILockInfo>();

			var lockObjectsRequestsMessage = new LockObjectsRequestsMessage
			{
				Requests = requests.ToList(),
			};

			Log($"Requesting locks for {string.Join(", ", requests)}");

			SendMessage(lockObjectsRequestsMessage, requiresResponse: true, out LockObjectsResponsesMessage responseMessage);

			return responseMessage.Responses.Select(x => new LockInfo
			{
				LockHolderInfo = x.LockHolderInfo,
				IsGranted = x.LockIsGranted,
				ObjectId = x.ObjectId,
				AutoUnlockTimestamp = x.AutoUnlockTimestamp,
			}).ToList();
		}

		/// <summary>
		/// Unlocks an object.
		/// </summary>
		/// <param name="request">Info about the object to be unlocked.</param>
		/// <returns>LockInfo object containing information on the requested lock.</returns>
		public void UnlockObject(UnlockObjectRequest request)
		{
			UnlockObjects(new[] { request });
		}

		/// <summary>
		/// Unlocks objects of a specific type.
		/// </summary>
		/// <param name="requests">Object IDs to unlock per object type.</param>
		/// <returns>True if lock was released correctly, else false.</returns>
		/// <exception cref="InvalidOperationException">If something went wrong during InterApp communication.</exception>
		public void UnlockObjects(IEnumerable<UnlockObjectRequest> requests)
		{
			if (requests is null) throw new ArgumentNullException(nameof(requests));
			if (!requests.Any()) return;

			var message = new UnlockObjectsRequestsMessage
			{
				Requests = requests,
			};

			SendMessage(message, requiresResponse: false, out UnlockObjectsResponsesMessage responseMessage);

			Log($"Releasing locks for {string.Join(", ", requests)}");
		}

		private void SendMessage<T>(Message message, bool requiresResponse, out T responseMessage) where T : Message
		{
			responseMessage = default;

			var commands = InterAppCallFactory.CreateNew();
			commands.Messages.Add(message);

			Log($"Message: {JsonConvert.SerializeObject(message)}");

			if (requiresResponse)
			{
				var response = commands.Send(connection, element.AgentId, element.Id, InterAppReceive_ParameterId, Timeout, InterAppKnownTypes).First();

				Log($"Response: {JsonConvert.SerializeObject(response)}");

				if (response is FailureMessage failureResponse)
				{
					throw new InvalidOperationException(failureResponse.Message);
				}
				else if (response is T expectedResponse)
				{
					responseMessage = expectedResponse;
				}
				else
				{
					throw new InvalidOperationException($"Received response is not of type {typeof(T)}");
				}
			}
			else
			{
				commands.Send(connection, element.AgentId, element.Id, InterAppReceive_ParameterId, InterAppKnownTypes);
			}
		}

		private TimeSpan GetTimeout()
		{
			try
			{
				var timeoutInSeconds = element.GetStandaloneParameter<double?>(InterAppTimeout_ParameterId) ?? throw new NullReferenceException("InterApp Timeout value is null.");
				var retrievedTimeout = TimeSpan.FromSeconds(timeoutInSeconds.GetValue().Value);

				Log($"Timeout: {timeout}");

				return retrievedTimeout;
			}
			catch (Exception e)
			{
				var retrievedTimeout = TimeSpan.FromSeconds(30);

				Log($"Unable to retrieve timeout due to: {e}");

				return retrievedTimeout;
			}
		}

		private void Log(string message)
		{
			string nameOfMethod = new StackTrace().GetFrame(1).GetMethod().Name;

			logger?.Log(nameof(LockManagerElement), nameOfMethod, message);
		}
	}
}
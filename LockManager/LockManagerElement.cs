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

	/// <inheritdoc cref="ILockManagerElement"/>
	public class LockManagerElement : ILockManagerElement
	{
		private static readonly int InterAppReceive_ParameterId = 9000000;
		private static readonly int InterAppTimeout_ParameterId = 100;

		private readonly IConnection connection;
		private readonly IDmsElement element;
		private readonly ILogger logger;

		private readonly Lazy<TimeSpan> timeout;

		/// <summary>
		/// Name of the Connector with which this element is able to communicate.
		/// </summary>
		public static readonly string SkylineLockManager_ConnectorName = "Skyline Lock Manager";

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

		/// <summary>
		/// Initializes a new instance of the <see cref="LockManagerElement"/> class.
		/// </summary>
		/// <param name="connection">Connection used to communicate with the Lock Manager element.</param>
		/// <param name="elementName">Name of the Lock Manager element.</param>
		/// <param name="logger">Object that is used to log info about locks.</param>
		/// <exception cref="ArgumentNullException">Thrown when the provided connection or the element is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when provided element id or agent id is negative.</exception>
		/// <exception cref="InvalidOperationException">Thrown when described element is inactive.</exception>
		public LockManagerElement(IConnection connection, string elementName, ILogger logger = null)
		{
			this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
			this.logger = logger;
			this.timeout = new Lazy<TimeSpan>(GetTimeout);

			element = connection.GetDms().GetElement(elementName) ?? throw new ArgumentException($"Unable to find an element with Name {elementName}", nameof(elementName));

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
			typeof(FailureMessage),
		};

		/// <inheritdoc cref="ILockManagerElement.LockObject(LockObjectRequest)"/>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="InvalidOperationException"/>
		public ILockInfo LockObject(LockObjectRequest request)
		{
			return LockObjects(new[] { request }).Single();
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

			Log($"Requesting locks for {string.Join(", ", requestsList)}");

			SendMessageWithResponse(lockObjectsRequestsMessage, out LockObjectsResponsesMessage responseMessage);

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

			SendMessageWithoutResponse(message);

			Log($"Releasing locks for {string.Join(", ", requestsList)}");
		}

		private void SendMessageWithoutResponse(Message message)
		{
			var commands = InterAppCallFactory.CreateNew();
			commands.Messages.Add(message);

			commands.Send(connection, element.AgentId, element.Id, InterAppReceive_ParameterId, InterAppKnownTypes);
		}

		private void SendMessageWithResponse<T>(Message message, out T responseMessage) where T : Message
		{
			responseMessage = default;

			var commands = InterAppCallFactory.CreateNew();
			commands.Messages.Add(message);

			Log($"Message: {JsonConvert.SerializeObject(message)}");

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

		private TimeSpan GetTimeout()
		{
			try
			{
				var timeoutInSeconds = element.GetStandaloneParameter<double?>(InterAppTimeout_ParameterId) ?? throw new ParameterNotFoundException($"Unable to find parameter {InterAppTimeout_ParameterId} in element {element.Name} ({element.DmsElementId})");

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
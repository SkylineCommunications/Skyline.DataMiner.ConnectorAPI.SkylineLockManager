namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Sending
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using Microsoft.Extensions.Logging;
	using Newtonsoft.Json;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Messages;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Messages.Locking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Messages.Unlocking;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Core.InterAppCalls.Common.CallBulk;
	using Skyline.DataMiner.Core.InterAppCalls.Common.CallSingle;
	using Skyline.DataMiner.Core.InterAppCalls.Common.Shared;
	using Skyline.DataMiner.Net;

	internal class InterAppSender : IInterAppSender
	{
		private static readonly int InterAppTimeout_ParameterId = 100;
		private static readonly int InterAppReceive_ParameterId = 9000000;
		private static readonly int InterAppResponse_ParameterId = 9000001;

		private readonly IConnection connection;
		private readonly IDmsElement element;
		private readonly ILogger logger;
		private readonly Lazy<TimeSpan> timeout;

		public InterAppSender(IConnection connection, IDmsElement element, ILogger<InterAppSender> logger)
		{
			timeout = new Lazy<TimeSpan>(GetTimeout);
			this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
			this.element = element ?? throw new ArgumentNullException(nameof(element));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		private TimeSpan Timeout => timeout.Value;

		/// <inheritdoc cref="IInterAppSender.SendLockObjectsRequestsMessage(LockObjectsRequestsMessage)"/>
		public LockObjectsResponsesMessage SendLockObjectsRequestsMessage(LockObjectsRequestsMessage message)
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			var response = SendMessageWithResponse<LockObjectsResponsesMessage>(message);

			return response;
		}

		/// <inheritdoc cref="IInterAppSender.SendUnlockObjectsRequestsMessage(UnlockObjectsRequestsMessage)"/>
		public void SendUnlockObjectsRequestsMessage(UnlockObjectsRequestsMessage message)
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			SendMessageWithoutResponse(message);
		}

		private void SendMessageWithoutResponse(Message message)
		{
			var interAppCall = InterAppCallFactory.CreateNew();
			interAppCall.Messages.Add(message);

			Log($"Sending InterApp call: {JsonConvert.SerializeObject(interAppCall)}");

			interAppCall.Send(connection, element.AgentId, element.Id, InterAppReceive_ParameterId, InterAppKnownTypes.KnownTypes);
		}

		private T SendMessageWithResponse<T>(Message message) where T : Message
		{
			var interAppCall = InterAppCallFactory.CreateNew();
			interAppCall.ReturnAddress = new ReturnAddress(element.AgentId, element.Id, InterAppResponse_ParameterId);
			interAppCall.Messages.Add(message);

			Log($"Sending InterApp call: {JsonConvert.SerializeObject(interAppCall)}");

			var response = interAppCall.Send(connection, element.AgentId, element.Id, InterAppReceive_ParameterId, Timeout, InterAppKnownTypes.KnownTypes).First();

			Log($"Received response: {JsonConvert.SerializeObject(response)}");

			if (response is FailureMessage failureResponse)
			{
				throw new InvalidOperationException(failureResponse.Message);
			}
			else if (response is T expectedResponse)
			{
				return expectedResponse;
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

				Log($"InterApp timeout: {retrievedTimeout}");

				return retrievedTimeout;
			}
			catch (Exception e)
			{
				var retrievedTimeout = TimeSpan.FromSeconds(30);

				Log($"Exception while retrieving InterApp timeout: {e}");

				return retrievedTimeout;
			}
		}

		private void Log(string message, LogLevel logLevel = LogLevel.Debug)
		{
			if (logger == null)
			{
				return;
			}

			string nameOfMethod = new StackTrace().GetFrame(1).GetMethod().Name;

			logger.Log(logLevel, "{className}|{methodName}|{message}", GetType().Name, nameOfMethod, message);
		}
	}
}

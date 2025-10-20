namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Receiving
{
	using System;
	using System.Collections.Generic;
	using Microsoft.Extensions.Logging;
	using Newtonsoft.Json;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Messages.Locking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Messages.Unlocking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager;
	using Skyline.DataMiner.Core.InterAppCalls.Common.CallBulk;
	using Skyline.DataMiner.Net;

	internal class InterAppReceiver : IInterAppReceiver
	{
		private static readonly Dictionary<Type, Type> messageToExecutor = new Dictionary<Type, Type>
		{
			{typeof(LockObjectsRequestsMessage), typeof(LockObjectsRequestsExecutor)},
			{typeof(UnlockObjectsRequestsMessage), typeof(UnlockObjectsRequestExecutor)},
		};

		public void HandleIncomingInterAppMessage(string rawInterAppCall, ILockManager lockManager, IReporter reporter, ILogger logger, IConnection connection)
		{
			try
			{
				var receivedCall = InterAppCallFactory.CreateFromRaw(rawInterAppCall, InterAppKnownTypes.KnownTypes);

				foreach (var message in receivedCall.Messages)
				{
					using (logger.BeginScope(new Dictionary<string, object> { { "InterAppMessage.ID", message.Guid } }))
					{
						logger.LogDebug("Received InterApp message: {message}.", JsonConvert.SerializeObject(message));

						if (!message.TryExecute(null, new InterAppMessageExecutionDependencies(logger, lockManager, reporter), messageToExecutor, out var replyMessage))
						{
							throw new InvalidOperationException($"Unable to execute incoming message");
						}

						if (!message.ExpectsReply)
						{
							logger.LogDebug("Message does not expect reply.");
							continue;
						}

						if (replyMessage is null)
						{
							throw new InvalidOperationException($"Unable to build reply message");
						}

						logger.LogDebug("Sending InterApp reply: {replymessage}.", JsonConvert.SerializeObject(replyMessage));

						message.Reply(connection, replyMessage, InterAppKnownTypes.KnownTypes);
					}
				}

			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error handling incoming inter-app message.");
			}
		}
	}
}
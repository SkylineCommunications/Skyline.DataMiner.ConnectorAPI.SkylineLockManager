namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Receiving
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Microsoft.Extensions.Logging;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Messages;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Messages.Locking;
	using Skyline.DataMiner.Core.InterAppCalls.Common.CallSingle;
	using Skyline.DataMiner.Core.InterAppCalls.Common.MessageExecution;

	internal class LockObjectsRequestsExecutor : SimpleMessageExecutor<LockObjectsRequestsMessage>
	{
		public LockObjectsRequestsExecutor(LockObjectsRequestsMessage message) : base(message)
		{
		}

		public override bool TryExecute(object dataSource, object dataDestination, out Message optionalReturnMessage)
		{
			var dependencies = dataDestination as InterAppMessageExecutionDependencies ?? throw new ArgumentException($"Argument is not of type {nameof(InterAppMessageExecutionDependencies)}", nameof(dataDestination));
			var lockManager = dependencies.LockManager ?? throw new ArgumentException("LockManager is null", nameof(dataDestination));
			var reporter = dependencies.Reporter ?? throw new ArgumentException("Reporter is null", nameof(dataDestination));
			var logger = dependencies.Logger ?? throw new ArgumentException("Logger is null", nameof(dataDestination));

			try
			{
				var allLockObjectResponses = new List<LockObjectResponse>();

				foreach (var lockObjectRequest in Message.Requests)
				{
					string joinedObjectIds = string.Join(", ", lockObjectRequest.Flatten().Select(lor => lor.ObjectId));
					using (logger.BeginScope(new Dictionary<string, object>{ { "LockObjectRequest.ObjectIDs", joinedObjectIds } }))
					{
						var lockObjectResponse = lockManager.RequestLock(lockObjectRequest);

						bool lockIsAlreadyTaken = !lockObjectResponse.LockIsGranted;
						logger.LogInformation("Locks are {granted}granted to {lockrequester}.", lockIsAlreadyTaken ? "not " : string.Empty, lockObjectRequest.ContextInfo);

						if (lockIsAlreadyTaken)
						{
							reporter.ReportLockObjectRequest(lockObjectRequest);
						}

						allLockObjectResponses.Add(lockObjectResponse);
					}
				}

				optionalReturnMessage = new LockObjectsResponsesMessage
				{
					Guid = Message.Guid,
					Responses = allLockObjectResponses,
				};
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Exception occurred: {exception}", ex.ToString());

				optionalReturnMessage = new FailureMessage
				{
					Guid = Message.Guid,
					Message = ex.ToString(),
				};
			}

			return true;
        }
	}
}

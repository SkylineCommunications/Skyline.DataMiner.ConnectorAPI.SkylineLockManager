namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Receiving
{
	using System;
	using System.Collections.Generic;
	using Microsoft.Extensions.Logging;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Messages.Unlocking;
	using Skyline.DataMiner.Core.InterAppCalls.Common.CallSingle;
	using Skyline.DataMiner.Core.InterAppCalls.Common.MessageExecution;

	internal class UnlockObjectsRequestExecutor : SimpleMessageExecutor<UnlockObjectsRequestsMessage>
	{
		public UnlockObjectsRequestExecutor(UnlockObjectsRequestsMessage unlockObjectsRequestsMessage) : base(unlockObjectsRequestsMessage)
		{
		}

		public override bool TryExecute(object dataSource, object dataDestination, out Message optionalReturnMessage)
		{
			var dependencies = dataDestination as InterAppMessageExecutionDependencies ?? throw new ArgumentException($"Argument is not of type {nameof(InterAppMessageExecutionDependencies)}", nameof(dataDestination));
			var lockManager = dependencies.LockManager ?? throw new ArgumentException("LockManager is null", nameof(dataDestination));
			var reporter = dependencies.Reporter ?? throw new ArgumentException("Reporter is null", nameof(dataDestination));
			var logger = dependencies.Logger ?? throw new ArgumentException("Logger is null", nameof(dataDestination));

			var allUnlockedObjects = new List<string>();

			foreach (var unlockObjectRequest in Message.Requests)
			{
				using (logger.BeginScope(new Dictionary<string, object> { { "UnlockObjectRequest.ObjectID", unlockObjectRequest.ObjectId } }))
				{
					var unlockedObjectIds = lockManager.UnlockObject(unlockObjectRequest.ObjectId, unlockObjectRequest.ReleaseLinkedObjects);

					logger.LogInformation("Unlocked objects {unlockedObjectIds}.", string.Join(", ", unlockedObjectIds));

					allUnlockedObjects.AddRange(unlockedObjectIds);
				}
			}

			reporter.ReportUnlockedObjects(allUnlockedObjects);

			optionalReturnMessage = null;
			return true;
		}
	}
}

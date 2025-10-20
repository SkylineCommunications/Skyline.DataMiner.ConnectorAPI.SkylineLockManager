namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Messages;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Messages.Locking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Messages.Unlocking;
	using Skyline.DataMiner.Core.InterAppCalls.Common.CallBulk;

	internal static class InterAppKnownTypes
	{
		public static IReadOnlyCollection<Type> KnownTypes { get; } = new List<Type>
		{
			typeof(IInterAppCall),
			typeof(LockObjectsRequestsMessage),
			typeof(LockObjectsResponsesMessage),
			typeof(UnlockObjectsRequestsMessage),
			typeof(FailureMessage),
		};
	}
}

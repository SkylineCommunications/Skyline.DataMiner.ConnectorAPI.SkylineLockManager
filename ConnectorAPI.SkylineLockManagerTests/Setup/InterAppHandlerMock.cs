namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests.Setup
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Messages.Locking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Messages.Unlocking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Sending;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager;

	internal class InterAppHandlerMock : IInterAppSender
	{
		private readonly ILockManager lockManager;

		internal InterAppHandlerMock(ILockManager lockManager)
		{
			this.lockManager = lockManager ?? throw new ArgumentNullException(nameof(lockManager));
		}

		public LockObjectsResponsesMessage SendLockObjectsRequestsMessage(LockObjectsRequestsMessage message)
		{
			if (message is null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			var responses = message.Requests.Select(req => lockManager.RequestLock(req));

			return new LockObjectsResponsesMessage { Responses = responses.ToList() };
		}

		public void SendUnlockObjectsRequestsMessage(UnlockObjectsRequestsMessage message)
		{
			if (message is null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			foreach (var unlockObjectRequest in message.Requests)
			{
				lockManager.UnlockObject(unlockObjectRequest.ObjectId, unlockObjectRequest.ReleaseLinkedObjects);
			}
		}
	}
}

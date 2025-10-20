namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Receiving
{
	using System.Collections.Generic;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Messages.Locking;

	public interface IReporter
	{
		void ReportLockObjectRequest(LockObjectRequest lockObjectRequest);

		void ReportUnlockedObjects(IEnumerable<string> unlockedObjectIds);
	}
}
namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Messages.Unlocking
{
	using System.Collections.Generic;
	using Skyline.DataMiner.Core.InterAppCalls.Common.CallSingle;

	/// <summary>
	/// InterApp message that is sent to the Lock Manager element when releasing a lock.
	/// </summary>
	internal class UnlockObjectsRequestsMessage : Message
	{
		/// <summary>
		/// Collection of items to be unlocked.
		/// </summary>
		public IEnumerable<UnlockObjectRequest> Requests { get; set; } = new List<UnlockObjectRequest>();
	}
}

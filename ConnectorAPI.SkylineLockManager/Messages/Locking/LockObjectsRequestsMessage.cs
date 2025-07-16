namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Locking
{
	using System.Collections.Generic;
	using Skyline.DataMiner.Core.InterAppCalls.Common.CallSingle;

	/// <summary>
	/// InterApp message that is sent to the Lock Manager element when requesting a lock.
	/// </summary>
	public class LockObjectsRequestsMessage : Message
	{
		/// <summary>
		/// Collection of items to be locked.
		/// </summary>
		public IEnumerable<LockObjectRequest> Requests { get; set; } = new List<LockObjectRequest>();
	}
}

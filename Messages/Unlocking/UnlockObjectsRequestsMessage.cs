using System.Collections.Generic;
using Skyline.DataMiner.Core.InterAppCalls.Common.CallSingle;

namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Unlocking
{
	/// <summary>
	/// InterApp message that is sent to the Lock Manager element when releasing a lock.
	/// </summary>
	public class UnlockObjectsRequestsMessage : Message
	{
		/// <summary>
		/// Collection of items to be unlocked.
		/// </summary>
		public IEnumerable<UnlockObjectRequest> Requests { get; set; } = new List<UnlockObjectRequest>();
	}
}

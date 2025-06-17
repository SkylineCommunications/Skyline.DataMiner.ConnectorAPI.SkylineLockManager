namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Unlocking
{
	using System.Collections.Generic;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager;
	using Skyline.DataMiner.Core.InterAppCalls.Common.CallSingle;

	/// <summary>
	/// Represents an InterApp message serving as reply to <see cref="UnlockObjectsRequestsMessage"/>.
	/// </summary>
	public class UnlockObjectsResponsesMessage : Message
	{
		/// <summary>
		/// Gets or sets a collection of responses to individual <see cref="UnlockObjectRequest"/>.
		/// </summary>
		public IEnumerable<UnlockObjectResponse> Responses { get; set; } = new List<UnlockObjectResponse>();
	}
}

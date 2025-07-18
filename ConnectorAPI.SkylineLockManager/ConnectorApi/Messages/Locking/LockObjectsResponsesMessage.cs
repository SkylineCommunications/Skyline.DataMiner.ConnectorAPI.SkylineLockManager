namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Locking
{
	using System.Collections.Generic;
	using Skyline.DataMiner.Core.InterAppCalls.Common.CallSingle;

	/// <summary>
	/// Represents an InterApp message serving as reply to <see cref="LockObjectsRequestsMessage"/>.
	/// </summary>
	public class LockObjectsResponsesMessage : Message
	{
		/// <summary>
		/// Gets or sets a collection of responses to individual <see cref="LockObjectRequest"/>.
		/// </summary>
		public IEnumerable<LockObjectResponse> Responses { get; set; } = new List<LockObjectResponse>();
	}
}

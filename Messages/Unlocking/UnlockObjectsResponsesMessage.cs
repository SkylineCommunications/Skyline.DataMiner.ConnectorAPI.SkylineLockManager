namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Unlocking
{
	using System.Collections.Generic;
	using Skyline.DataMiner.Core.InterAppCalls.Common.CallSingle;

	public class UnlockObjectsResponsesMessage : Message
	{
		public IEnumerable<UnlockObjectResponse> Responses { get; set; } = new List<UnlockObjectResponse>();
	}
}

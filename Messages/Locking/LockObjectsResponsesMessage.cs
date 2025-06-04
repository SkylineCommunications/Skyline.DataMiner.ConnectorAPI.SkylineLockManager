namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Locking
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Skyline.DataMiner.Core.InterAppCalls.Common.CallSingle;

	public class LockObjectsResponsesMessage : Message
	{
		public IEnumerable<LockObjectResponse> Responses { get; set; } = new List<LockObjectResponse>();
	}
}

namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Skyline.DataMiner.Core.InterAppCalls.Common.CallSingle;

	public class FailureMessage : Message
	{
		public string Message { get; set; }
	}
}

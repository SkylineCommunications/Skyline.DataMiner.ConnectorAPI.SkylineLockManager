namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages
{
	using Skyline.DataMiner.Core.InterAppCalls.Common.CallSingle;

	/// <summary>
	/// Represents an InterApp message, used as reply by the element to indicate something went wrong.
	/// </summary>
	public class FailureMessage : Message
	{
		/// <summary>
		/// Gets or sets a message indicating what went wrong.
		/// </summary>
		public string Message { get; set; }
	}
}

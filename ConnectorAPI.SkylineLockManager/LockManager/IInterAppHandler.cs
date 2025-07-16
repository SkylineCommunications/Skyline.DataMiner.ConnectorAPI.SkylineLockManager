namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager
{
	using Skyline.DataMiner.Core.InterAppCalls.Common.CallSingle;

	/// <summary>
	/// Represents an object responsible for handling inter-app communication.
	/// </summary>
	public interface IInterAppHandler
	{
		/// <summary>
		/// Sends a message to another app without expecting a response.
		/// </summary>
		/// <param name="message">The message to send.</param>
		void SendMessageWithoutResponse(Message message);

		/// <summary>
		/// Sends a message to another app and expects a response of type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The type of the expected response.</typeparam>
		/// <param name="message">The message to send.</param>
		/// <returns>A response of type <typeparamref name="T"/>.</returns>
		T SendMessageWithResponse<T>(Message message) where T : Message;
	}
}
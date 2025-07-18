namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi
{
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Locking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Unlocking;

	/// <summary>
	/// Represents an object responsible for handling inter-app communication.
	/// </summary>
	public interface IInterAppHandler
	{
		/// <summary>
		/// Sends a request to lock specified objects and returns the response containing the results of the lock operation.
		/// </summary>
		/// <remarks>Use this method to request locks on one or more objects. The response will indicate whether each
		/// lock was successfully acquired. Ensure that the <paramref name="message"/> parameter is properly populated with
		/// the required details before calling this method.</remarks>
		/// <param name="message">The request message containing the details of the objects to be locked. This parameter cannot be null.</param>
		/// <returns>A <see cref="LockObjectsResponsesMessage"/> containing the results of the lock operation, including the status of
		/// each requested lock.</returns>
		LockObjectsResponsesMessage SendLockObjectsRequestsMessage(LockObjectsRequestsMessage message);

		/// <summary>
		/// Sends a message to request the unlocking of specified objects.
		/// </summary>
		/// <remarks>Ensure that the <paramref name="message"/> parameter is properly populated with the required
		/// details before calling this method. The behavior of the method depends on the contents of the message.</remarks>
		/// <param name="message">The message containing the details of the unlock requests, including the objects to be unlocked.</param>
		void SendUnlockObjectsRequestsMessage(UnlockObjectsRequestsMessage message);
	}
}
namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners.Unlocks
{
	using System;
	using System.Threading.Tasks;

	/// <summary>
	/// Represents an object responsible for listening for unlock events.
	/// </summary>
	public interface IUnlockListener : IDisposable
	{
		/// <summary>
		/// Start listening for unlock events for a specific object ID.
		/// </summary>
		Task StartListeningForUnlock(string objectId);

		/// <summary>
		/// Stop listening for unlock events for a specific object ID.
		/// </summary>
		/// <param name="objectId"></param>
		void StopListeningForUnlock(string objectId);
	}
}
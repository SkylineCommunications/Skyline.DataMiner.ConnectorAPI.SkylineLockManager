﻿namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners.Unlocks
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	/// <summary>
	/// Represents an object responsible for listening for unlock events.
	/// </summary>
	public interface IUnlockListener : IDisposable
	{
		/// <summary>
		/// Start listening for unlock events for a specific object ID.
		/// </summary>
		Task<bool> StartListeningForUnlock(string objectId);

		/// <summary>
		/// Start listening for unlock events for specific object IDs.
		/// </summary>
		IReadOnlyDictionary<string, Task<bool>> StartListeningForUnlocks(ICollection<string> objectIds);

		/// <summary>
		/// Stop listening for unlock events for a specific object ID.
		/// </summary>
		/// <param name="objectId"></param>
		void StopListeningForUnlock(string objectId);

		/// <summary>
		/// Stop listening for unlock events for specific object IDs.
		/// </summary>
		void StopListeningForUnlocks(ICollection<string> objectIds);
	}
}
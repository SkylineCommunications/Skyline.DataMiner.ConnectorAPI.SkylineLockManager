namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners.Unlocks
{
	using System;

	internal interface IListener : IDisposable
	{
		bool IsListening { get; }

		void StartListening();

		void StopListening();
	}
}
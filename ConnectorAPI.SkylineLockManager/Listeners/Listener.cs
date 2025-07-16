namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners.Unlocks;

	public abstract class Listener : IDisposable
	{
		protected readonly string sourceId = Guid.NewGuid().ToString();

		protected bool isListening;
		protected bool disposedValue;

		private bool startingListen;
		private bool stoppingListen;

		/// <summary>
		/// Disposes the <see cref="UnlockListener"/> instance, releasing any resources it holds.
		/// </summary>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected abstract void StartMonitor();

		protected abstract void StopMonitor();

		/// <summary>
		/// Releases the resources used by the current instance of the class.
		/// </summary>
		/// <remarks>This method should be called when the instance is no longer needed to ensure proper cleanup of
		/// resources. If the instance is used after calling this method, it may result in undefined behavior.</remarks>
		protected abstract void Dispose(bool disposing);

		protected void StartListening()
		{
			if (isListening || startingListen)
			{
				// Avoid re-entrancy
				return;
			}

			startingListen = true;

			StartMonitor();

			isListening = true;
			startingListen = false;
		}

		protected void StopListening()
		{
			if (!isListening || stoppingListen)
			{
				// Avoid re-entrancy
				return;
			}

			stoppingListen = true;

			StopMonitor();

			isListening = false;
			stoppingListen = false;
		}
	}
}

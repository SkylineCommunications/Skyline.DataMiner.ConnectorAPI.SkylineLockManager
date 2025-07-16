namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners.Unlocks
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Core.DataMinerSystem.Common.Subscription.Monitors;

	/// <inheritdoc cref="IUnlockListener"/>
	public class UnlockListener : Listener, IUnlockListener
	{
		private static readonly int UnlockUpdates_ParameterId = 200;

		private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> taskCompletionSources = new ConcurrentDictionary<string, TaskCompletionSource<bool>>();

		private readonly IDmsStandaloneParameter<string> parameter;

		/// <summary>
		/// Initializes a new instance of the <see cref="UnlockListener"/> class with the specified DMS element.
		/// </summary>
		/// <param name="element">The DMS element used to retrieve the standalone parameter. This parameter cannot be <see langword="null"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="element"/> is <see langword="null"/>.</exception>
		public UnlockListener(IDmsElement element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			this.parameter = element.GetStandaloneParameter<string>(UnlockUpdates_ParameterId);
		}

		/// <inheritdoc cref="IUnlockListener.StartListeningForUnlock(string)"/>
		public Task StartListeningForUnlock(string objectId)
		{
			var unlockTaskCompletionSource = taskCompletionSources.GetOrAdd(objectId, _ => new TaskCompletionSource<bool>());

			if (!isListening)
			{
				StartListening();
			}	

			return unlockTaskCompletionSource.Task;
		}

		/// <inheritdoc cref="IUnlockListener.StopListeningForUnlock(string)"/>
		public void StopListeningForUnlock(string objectId)
		{
			if (taskCompletionSources.TryRemove(objectId, out var taskCompletionSource))
			{
				// Make sure the task listening to this TaskCompletionSource is canceled.
				taskCompletionSource.TrySetCanceled();
			}

			if (taskCompletionSources.IsEmpty && isListening)
			{
				StopListening();
			}
		}

		/// <summary>
		/// Starts the monitor that listens for unlock events.
		/// </summary>
		protected override void StartMonitor()
		{
			Action<ParamValueChange<string>> reportUnlock = (change) =>
			{
				string unlockedObjectId = change.Value;

				if (taskCompletionSources.TryGetValue(unlockedObjectId, out var taskCompletionSource))
				{
					taskCompletionSource.SetResult(true);
				}
			};

			parameter.StartValueMonitor(sourceId, reportUnlock);
		}

		/// <summary>
		/// Stops the monitor that listens for unlock events.
		/// </summary>
		protected override void StopMonitor()
		{
			parameter.StopValueMonitor(sourceId);
		}


		protected override void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					StopListening();
					foreach (var taskCompletionSource in taskCompletionSources.Values)
					{
						taskCompletionSource.TrySetCanceled();
					}
				}

				disposedValue = true;
			}
		}
	}
}
namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners.Unlocks
{
	using System;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;

	/// <inheritdoc cref="IUnlockListener"/>
	public class UnlockListener : UnlockListenerBase, IUnlockListener
	{
		private static readonly int UnlockUpdates_ParameterId = 200;

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

		/// <inheritdoc cref="Listener.StartMonitor()"/>
		protected override void StartMonitor()
		{
			parameter.StartValueMonitor(sourceId, (paramValueChange) =>
			{
				var unlockedObjectIds = paramValueChange.Value.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

				foreach (var unlockedObjectId in unlockedObjectIds)
				{
					if (taskCompletionSources.TryGetValue(unlockedObjectId, out var taskCompletionSource))
					{
						taskCompletionSource.SetResult(true);
					}
				}
			});
		}

		/// <inheritdoc cref="Listener.StopMonitor()"/>
		protected override void StopMonitor()
		{
			parameter.StopValueMonitor(sourceId);
		}
	}
}
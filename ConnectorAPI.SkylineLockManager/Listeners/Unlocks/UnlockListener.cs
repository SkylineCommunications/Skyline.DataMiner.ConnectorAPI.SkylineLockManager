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
		/// <param name="logger">An optional logger.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="element"/> is <see langword="null"/>.</exception>
		public UnlockListener(IDmsElement element, ILogger logger = null) : base(logger)
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
			Log($"Starting monitor for Skyline Lock Manager element '{parameter.Element.Name}' parameter {parameter.Id}");

			parameter.StartValueMonitor(sourceId, (paramValueChange) =>
			{
				paramValueChange.Dms.CreateScript(new AutomationScriptConfiguration("VSC_Test_Success", "VSC"));

				Log($"Noticed unlocked objects '{paramValueChange.Value}'");
				Log($"Task completion source dict keys '{String.Join(", ", taskCompletionSources.Keys)}'");

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
			Log($"Stopping monitor for Skyline Lock Manager element '{parameter.Element.Name}' parameter {parameter.Id}");

			parameter.StopValueMonitor(sourceId);
		}
	}
}
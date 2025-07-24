namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners.Unlocks
{
	using System;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager;
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

			parameter = element.GetStandaloneParameter<string>(UnlockUpdates_ParameterId);
		}

		/// <inheritdoc/>
		protected override void StartMonitor()
		{
			Log($"Starting monitor for Skyline Lock Manager element '{parameter.Element.Name}' parameter {parameter.Id}");

			parameter.StartValueMonitor(sourceId, (paramValueChange) =>
			{
				var unlockedObjectIds = paramValueChange.Value.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

				ReportUnlockedObjects(unlockedObjectIds);
			});
		}

		/// <inheritdoc/>
		protected override void StopMonitor()
		{
			Log($"Stopping monitor for Skyline Lock Manager element '{parameter.Element.Name}' parameter {parameter.Id}");

			parameter.StopValueMonitor(sourceId);
		}
	}
}
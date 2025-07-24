namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners.HigherPriorityLockRequests
{
	using System;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Locking;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Utils.SecureCoding.SecureSerialization.Json.Newtonsoft;

	/// <inheritdoc cref="IHigherPriorityLockRequestListener"/>
	public class HigherPriorityLockRequestListener : HigherPriorityLockRequestListenerBase, IHigherPriorityLockRequestListener
	{
		private static readonly int HigherPrioLockRequests_ParameterId = 201;

		private readonly IDmsStandaloneParameter<string> parameter;

		/// <summary>
		/// Initializes a new instance of the <see cref="HigherPriorityLockRequestListener"/> class.
		/// </summary>
		/// <param name="element">The <see cref="IDmsElement"/> instance used to retrieve the parameter associated with higher priority lock
		/// requests. Cannot be <see langword="null"/>.</param>
		/// <param name="logger">An optional logger.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="element"/> is <see langword="null"/>.</exception>
		public HigherPriorityLockRequestListener(IDmsElement element, ILogger logger = null) : base(logger)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			parameter = element.GetStandaloneParameter<string>(HigherPrioLockRequests_ParameterId);
		}

		/// <inheritdoc/>
		protected override void StartMonitor()
		{
			Log($"Starting monitor for Skyline Lock Manager element '{parameter.Element.Name}' parameter {parameter.Id}");

			parameter.StartValueMonitor(sourceId, (change) =>
			{
				string serializedLockObjectRequest = change.Value;

				var lockObjectRequest = SecureNewtonsoftDeserialization.DeserializeObject<LockObjectRequest>(serializedLockObjectRequest) ?? throw new InvalidOperationException($"{serializedLockObjectRequest} could not be deserialized to a {nameof(LockObjectRequest)}.");

				ReportHigherPrioLockObjectRequest(lockObjectRequest);
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

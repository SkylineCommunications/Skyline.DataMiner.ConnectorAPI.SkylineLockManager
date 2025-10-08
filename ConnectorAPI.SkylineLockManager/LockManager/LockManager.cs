namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using Microsoft.Extensions.Logging;
	using Microsoft.Extensions.Logging.Abstractions;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Locking;

	/// <inheritdoc cref="ILockManager"/>
	public partial class LockManager : ILockManager
	{
		private static readonly ActivitySource activitySource = new ActivitySource("Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager");

		private ILogger<LockManager> logger;

		/// <summary>
		/// Represents a collection of objects that are locked, keyed by their unique identifiers.
		/// </summary>
		/// <remarks>This dictionary is used to manage and track objects that are currently locked.  The keys are
		/// strings representing unique identifiers, and the values are instances of <see cref="LockedObject"/>.</remarks>
		protected readonly IDictionary<string, LockedObject> lockedObjects;

		/// <summary>
		/// Initializes a new instance of the <see cref="LockManager"/> class, optionally with a predefined set of locked
		/// objects.
		/// </summary>
		/// <param name="lockedObjects">An optional dictionary containing the initial set of locked objects, where the key is the object identifier and
		/// the value is the corresponding <see cref="LockedObject"/>. If null, an empty dictionary is used.</param>
		/// <param name="logger">An optional logger.</param>
		public LockManager(IDictionary<string, LockedObject> lockedObjects = null, ILogger<LockManager> logger = null)
		{
			this.lockedObjects = lockedObjects ?? new Dictionary<string, LockedObject>();
			this.logger = logger ?? new NullLogger<LockManager>();
		}

		/// <inheritdoc cref="ILockManager.DefaultAutoLockReleaseTimeSpan"/>
		public TimeSpan DefaultAutoLockReleaseTimeSpan { get; protected set; } = TimeSpan.FromHours(1);

		/// <inheritdoc cref="ILockManager.SetLogger(ILogger)"/>
		public void SetLogger(ILogger logger)
		{
			if (logger == null)
			{
				this.logger = new NullLogger<LockManager>();
			}
			else if(logger is ILogger<LockManager> typedLogger)
			{
				this.logger = typedLogger;
			}
			else
			{
				throw new ArgumentException("Logger must be of type ILogger<LockManager>.", nameof(logger));
			}
		}

		/// <inheritdoc cref="ILockManager.UnlockExpiredObjects"/>
		public void UnlockExpiredObjects()
		{
			var now = DateTime.Now;

			var objectIdsToUnlock = lockedObjects.Where(lo => lo.Value.AutoUnlockTimestamp < now).Select(lo => lo.Key).ToList();

			logger.Log(objectIdsToUnlock.Count > 0 ? LogLevel.Warning : LogLevel.Debug, "Unlocking {Count} expired locks: {ObjectIds}", objectIdsToUnlock.Count, String.Join(", ", objectIdsToUnlock));

			foreach (var objectIdToUnlock in objectIdsToUnlock)
			{
				UnlockObject(objectIdToUnlock, unlockLinkedObjects: true);
			}
		}

		/// <inheritdoc cref="ILockManager.RequestLock(LockObjectRequest)"/>
		public virtual LockObjectResponse RequestLock(LockObjectRequest lockObjectRequest)
		{
			using (activitySource.StartActivity())
			{
				lock (lockedObjects)
				{
					SetAutoUnlockTimeSpan(lockObjectRequest);

					var lockObjectResponse = CheckLockAvailability(lockObjectRequest);

					var individualResponses = lockObjectResponse.Flatten().ToList();

					var unavailableLocks = individualResponses.Where(lockRequestResult => !lockRequestResult.LockIsAvailable).ToList();

					bool allLocksAreAvailable = unavailableLocks.Count == 0;
					if (allLocksAreAvailable)
					{
						LockObjects(lockObjectRequest);
						individualResponses.ForEach(ir => ir.LockIsGranted = true);
					}
					else
					{
						logger.LogDebug("Lock request for object ID '{ObjectId}' could not be granted because one or more locks are not available: {UnavailableLocks}", lockObjectRequest.ObjectId, String.Join(", ", unavailableLocks.Select(ul => ul.ObjectId)));

						// If one or more locks are not available, then none of the locks should be granted.
						individualResponses.ForEach(ir => ir.LockIsGranted = false);
					}

					return lockObjectResponse;
				}
			}
		}

		/// <inheritdoc cref="ILockManager.UnlockAllObjects"/>
		public virtual ICollection<string> UnlockAllObjects()
		{
			using (activitySource.StartActivity())
			{
				lock (lockedObjects)
				{
					var allUnlockedObjects = lockedObjects.Keys.ToList();

					lockedObjects.Clear();

					return allUnlockedObjects;
				}
			}
		}

		/// <inheritdoc cref="ILockManager.UnlockObject(string, bool)"/>
		public virtual ICollection<string> UnlockObject(string objectId, bool unlockLinkedObjects)
		{
			using (activitySource.StartActivity())
			{
				lock (lockedObjects)
				{
					var allUnlockedObjectIds = new List<string>();

					if (unlockLinkedObjects && lockedObjects.TryGetValue(objectId, out var objectToUnlock))
					{
						foreach (var linkedObjectId in objectToUnlock.LinkedObjectIds ?? Enumerable.Empty<string>())
						{
							allUnlockedObjectIds.AddRange(UnlockObject(linkedObjectId, unlockLinkedObjects));
						}
					}

					if (lockedObjects.Remove(objectId))
					{
						logger.LogTrace(LogEvents.Unlocked, $"Unlocked object {objectId}");

						allUnlockedObjectIds.Add(objectId);
					}

					return allUnlockedObjectIds;
				}
			}
		}

		private void SetAutoUnlockTimeSpan(LockObjectRequest request)
		{
			request.AutoUnlockTimeSpan = request.AutoUnlockTimeSpan ?? DefaultAutoLockReleaseTimeSpan;

			foreach (var linkedObjectRequest in request.LinkedObjectRequests)
			{
				SetAutoUnlockTimeSpan(linkedObjectRequest);
			}
		}

		private LockObjectResponse CheckLockAvailability(LockObjectRequest request)
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var linkedLockObjectResponses = new List<LockObjectResponse>();

			foreach (var linkedObjectRequest in request.LinkedObjectRequests)
			{
				linkedLockObjectResponses.Add(CheckLockAvailability(linkedObjectRequest));
			}

			var lockObjectResponse = new LockObjectResponse
			{
				ObjectId = request.ObjectId,
				LinkedObjectResponses = linkedLockObjectResponses,
			};

			if (lockedObjects.TryGetValue(request.ObjectId, out var lockedObject))
			{
				lockObjectResponse.LockIsAvailable = false;
				lockObjectResponse.LockHolderInfo = lockedObject.ContextInfo;
			}
			else
			{
				lockObjectResponse.LockIsAvailable = true;
				lockObjectResponse.LockHolderInfo = request.ContextInfo;
			}

			return lockObjectResponse;
		}

		private void LockObjects(LockObjectRequest lockRequest)
		{
			foreach (var linkedLockRequest in lockRequest.LinkedObjectRequests)
			{
				LockObjects(linkedLockRequest);
			}

			var lockedObject = new LockedObject
			{
				ObjectId = lockRequest.ObjectId,
				ObjectDescription = lockRequest.ObjectDescription,
				ContextInfo = lockRequest.ContextInfo,
				Timestamp = DateTime.Now,
				AutoUnlockTimestamp = DateTime.Now + lockRequest.AutoUnlockTimeSpan.Value,
				LinkedObjectIds = lockRequest.LinkedObjectRequests.Select(lo => lo.ObjectId).ToList(),
				Priority = lockRequest.Priority,
			};

			lockedObjects.Add(lockedObject.ObjectId, lockedObject);

			logger.LogTrace(LogEvents.Locked, $"Locked object {lockedObject.ObjectId}");
		}
	}
}

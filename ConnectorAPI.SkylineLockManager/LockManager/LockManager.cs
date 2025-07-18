namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Locking;
	using Skyline.DataMiner.Net.Helper;

	/// <inheritdoc cref="ILockManager"/>
	public class LockManager : ILockManager
	{
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
		public LockManager(IDictionary<string, LockedObject> lockedObjects = null)
		{
			this.lockedObjects = lockedObjects ?? new Dictionary<string, LockedObject>();
		}

		/// <inheritdoc cref="ILockManager.DefaultAutoLockReleaseTimeSpan"/>
		public TimeSpan DefaultAutoLockReleaseTimeSpan { get; protected set; } = TimeSpan.FromHours(1);

		/// <inheritdoc cref="ILockManager.UnlockExpiredObjects"/>
		public void UnlockExpiredObjects()
		{
			var now = DateTime.Now;

			var objectIdsToUnlock = lockedObjects.Where(lo => lo.Value.AutoUnlockTimestamp < now).Select(lo => lo.Key).ToList();

			foreach (var objectIdToUnlock in objectIdsToUnlock)
			{
				lockedObjects.Remove(objectIdToUnlock);
			}
		}

		/// <inheritdoc cref="ILockManager.RequestLock(LockObjectRequest)"/>
		public IEnumerable<LockObjectResponse> RequestLock(LockObjectRequest lockObjectRequest)
		{
			SetAutoUnlockTimeSpan(lockObjectRequest);

			var lockObjectResponses = CheckLockAvailability(lockObjectRequest);

			bool allLocksAreGranted = !lockObjectResponses.Any(lockRequestResult => !lockRequestResult.LockIsGranted);
			if (allLocksAreGranted)
			{
				LockObjects(lockObjectRequest);
			}
			else
			{
				// If one or more locks are not granted, then none of the locks should be granted.
				lockObjectResponses.ForEach(lockRequestResult => lockRequestResult.LockIsGranted = false);
			}

			return lockObjectResponses;
		}

		/// <inheritdoc cref="ILockManager.UnlockAllObjects"/>
		public ICollection<string> UnlockAllObjects()
		{
			var allUnlockedObjects = lockedObjects.Keys.ToList();

			lockedObjects.Clear();

			return allUnlockedObjects;
		}

		/// <inheritdoc cref="ILockManager.UnlockObject(string, bool)"/>
		public ICollection<string> UnlockObject(string objectId, bool unlockLinkedObjects)
		{
			var allUnlockedObjectIds = new List<string>();

			if (unlockLinkedObjects && lockedObjects.TryGetValue(objectId, out var objectToUnlock))
			{
				foreach (var linkedObjectId in objectToUnlock.LinkedObjectIds)
				{
					allUnlockedObjectIds.AddRange(UnlockObject(linkedObjectId, unlockLinkedObjects));
				}
			}

			if (lockedObjects.Remove(objectId))
			{
				allUnlockedObjectIds.Add(objectId);
			}

			return allUnlockedObjectIds;
		}

		private void SetAutoUnlockTimeSpan(LockObjectRequest request)
		{
			request.AutoUnlockTimeSpan = request.AutoUnlockTimeSpan ?? DefaultAutoLockReleaseTimeSpan;

			foreach (var linkedObjectRequest in request.LinkedObjectRequests)
			{
				SetAutoUnlockTimeSpan(linkedObjectRequest);
			}
		}

		private IEnumerable<LockObjectResponse> CheckLockAvailability(LockObjectRequest request)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			var lockObjectResponses = new List<LockObjectResponse>();

			foreach (var linkedObjectRequest in request.LinkedObjectRequests)
			{
				lockObjectResponses.AddRange(CheckLockAvailability(linkedObjectRequest));
			}

			var lockObjectResponse = new LockObjectResponse
			{
				ObjectId = request.ObjectId,
			};

			if (lockedObjects.TryGetValue(request.ObjectId, out var lockedObject))
			{
				lockObjectResponse.LockIsGranted = false;
				lockObjectResponse.LockHolderInfo = lockedObject.LockHolderInfo;
			}
			else
			{
				lockObjectResponse.LockIsGranted = true;
				lockObjectResponse.LockHolderInfo = request.LockRequesterInfo;
			}

			lockObjectResponses.Add(lockObjectResponse);

			return lockObjectResponses;
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
				LockHolderInfo = lockRequest.LockRequesterInfo,
				Timestamp = DateTime.Now,
				AutoUnlockTimestamp = DateTime.Now + lockRequest.AutoUnlockTimeSpan.Value,
				LinkedObjectIds = lockRequest.LinkedObjectRequests.Select(lo => lo.ObjectId).ToList(),
				Priority = lockRequest.Priority,
			};

			lockedObjects.Add(lockedObject.ObjectId, lockedObject);
		}
	}
}

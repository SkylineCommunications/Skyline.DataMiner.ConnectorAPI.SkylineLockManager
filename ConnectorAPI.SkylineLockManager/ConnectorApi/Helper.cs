namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Threading.Tasks;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Messages.Locking;

	public partial class SkylineLockManagerConnectorApi
	{
		private sealed class Helper
		{
			private readonly ConcurrentDictionary<LockObjectRequest, LockObjectResponse> mostRecentResponsePerRequest = new ConcurrentDictionary<LockObjectRequest, LockObjectResponse>();
			private readonly SkylineLockManagerConnectorApi api;
			private readonly ICollection<LockObjectRequest> allLockObjectRequests;
			private TimeSpan maxWaitingTime;

			private Helper(SkylineLockManagerConnectorApi skylineLockManagerConnectorApi, ICollection<LockObjectRequest> lockObjectRequests, TimeSpan maxWaitingTime)
			{
				this.api = skylineLockManagerConnectorApi ?? throw new ArgumentNullException(nameof(skylineLockManagerConnectorApi));
				this.allLockObjectRequests = lockObjectRequests ?? throw new ArgumentNullException(nameof(lockObjectRequests));
				this.maxWaitingTime = maxWaitingTime;

				if (maxWaitingTime <= TimeSpan.Zero)
				{
					throw new ArgumentOutOfRangeException(nameof(maxWaitingTime), "Max waiting time must be greater than zero.");
				}
			}

			public static List<LockObjectResponse> LockObjectsWithWait(SkylineLockManagerConnectorApi api, ICollection<LockObjectRequest> lockObjectRequests, TimeSpan maxWaitingTime, out TimeSpan totalWaitingTime)
			{
				var helper = new Helper(api, lockObjectRequests, maxWaitingTime);

				if (lockObjectRequests.Count == 0)
				{
					totalWaitingTime = TimeSpan.Zero;
					return new List<LockObjectResponse>();
				}

				return helper.LockObjectsWithWait(out totalWaitingTime);
			}

			private List<LockObjectResponse> LockObjectsWithWait(out TimeSpan totalWaitingTime)
			{
				SendLockObjectRequests(allLockObjectRequests);

				var notGrantedRequestResponsePairs = mostRecentResponsePerRequest.Where(kvp => !kvp.Value.LockIsGranted).ToList();

				if (notGrantedRequestResponsePairs.Count == 0)
				{
					// All locks were granted immediately.
					totalWaitingTime = TimeSpan.Zero;
					return mostRecentResponsePerRequest.Select(kvp => kvp.Value).ToList();
				}

				var stopwatch = Stopwatch.StartNew();

				if (!api.unlockListener.IsListening)
				{
					// Starting the listener can take some time (> 200 ms)
					// In this timespan objects could have been unlocked, so we need to resend the requests after starting the listener.

					api.unlockListener.StartListening();

					var notGrantedRequests = notGrantedRequestResponsePairs.Select(kvp => kvp.Key).ToList();

					SendLockObjectRequests(notGrantedRequests);

					maxWaitingTime -= stopwatch.Elapsed;
				}

				var tasksToWaitForPerRequest = new List<Task>();

				foreach (var kvp in mostRecentResponsePerRequest)
				{
					var lockObjectResponse = kvp.Value;

					if (lockObjectResponse.LockIsGranted)
					{
						continue;
					}

					var taskForRequest = Task.Run<bool>(() =>
					{
						var remainingWaitingTime = maxWaitingTime;

						var lockObjectRequest = kvp.Key;

						var notAvailableObjectIds = lockObjectResponse.Flatten().Where(response => !response.LockIsAvailable).Select(response => response.ObjectId).ToList();

						if (notAvailableObjectIds.Count <= 0)
						{
							throw new InvalidOperationException("No unavailable object IDs found, but lock was not granted.");
						}

						var stopwatchForUnlocks = new Stopwatch();

						while (remainingWaitingTime > TimeSpan.Zero)
						{
							var tasksToWaitForObjectUnlocks = api.unlockListener.StartListeningForUnlocks(notAvailableObjectIds);

							stopwatchForUnlocks.Restart();

							bool objectsGotUnlocked = Task.WaitAll(tasksToWaitForObjectUnlocks.Values.ToArray(), remainingWaitingTime) && tasksToWaitForObjectUnlocks.Values.All(t => t.Result);

							if (objectsGotUnlocked)
							{
								mostRecentResponsePerRequest[lockObjectRequest] = api.SendLockObjectRequest(lockObjectRequest);

								if (mostRecentResponsePerRequest[lockObjectRequest].LockIsGranted)
								{
									return true;
								}
								else
								{
									notAvailableObjectIds = mostRecentResponsePerRequest[lockObjectRequest].Flatten().Where(response => !response.LockIsAvailable).Select(response => response.ObjectId).ToList();
								}
							}
							else
							{
								// Objects did not get unlocked within the remaining waiting time.
								return false;
							}

							remainingWaitingTime -= stopwatchForUnlocks.Elapsed;
						}

						// Waiting time elapsed.
						return false;
					});

					tasksToWaitForPerRequest.Add(taskForRequest);
				}

				if (tasksToWaitForPerRequest.Count > 0)
				{
					Task.WaitAll(tasksToWaitForPerRequest.ToArray(), maxWaitingTime);
				}

				stopwatch.Stop();
				totalWaitingTime = stopwatch.Elapsed;

				api.unlockListener.StopListeningForUnlocks(allLockObjectRequests.SelectMany(req => req.Flatten()).Select(req => req.ObjectId).ToList());

				return mostRecentResponsePerRequest.Select(kvp => kvp.Value).ToList();
			}

			private void SendLockObjectRequests(ICollection<LockObjectRequest> requests)
			{
				var lockObjectResponses = api.SendLockObjectRequests(requests);

				foreach (var response in lockObjectResponses)
				{
					var request = allLockObjectRequests.Single(req => req.ObjectId == response.ObjectId);

					mostRecentResponsePerRequest[request] = response;
				}
			}
		}
	}
}

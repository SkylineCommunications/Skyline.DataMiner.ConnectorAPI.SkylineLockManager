namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager.Tests
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Listeners;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.Messages.Locking;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests.LockManager;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Net;

	[TestClass()]
	public class LockManagerElementTests
	{
		[TestMethod()]
		public void ListenForHigherPriorityLockRequests_HigherPrioLockRequestCameIn()
		{
			// Arrange
			var connectionMock = new Mock<IConnection>();

			var higherPrioLockRequestListenerMock = new HigherPrioLockRequestListenerMock();

			var elementMock = new Mock<IDmsElement>();
			elementMock.SetupGet(e => e.State).Returns(ElementState.Active);

			var lockManagerElement = new LockManagerElement(connectionMock.Object, elementMock.Object, higherPrioLockRequestListener: higherPrioLockRequestListenerMock);

			// Act
			lockManagerElement.ListenForLockRequestsWithHigherPriorityThan(new ObjectIdAndPriority("objectId", Priority.Medium));

			bool higherPrioLockRequestCameIn = false;
			lockManagerElement.HigherPriorityLockRequestReceived += (sender, args) =>
			{
				if (args.LockObjectRequest.ObjectId == "objectId" && args.LockObjectRequest.Priority > Priority.Medium)
				{
					higherPrioLockRequestCameIn = true;
				}
			};

			higherPrioLockRequestListenerMock.SpoofLockRequest(new LockObjectRequest
			{
				ObjectId = "objectId",
				Priority = Priority.High,
			});

			// Assert
			Assert.IsTrue(higherPrioLockRequestCameIn);
		}

		[TestMethod()]
		public void ListenForHigherPriorityLockRequests_NoHigherPrioLockRequestCameIn()
		{
			// Arrange
			var connectionMock = new Mock<IConnection>();

			var higherPrioLockRequestListenerMock = new HigherPrioLockRequestListenerMock();

			var elementMock = new Mock<IDmsElement>();
			elementMock.SetupGet(e => e.State).Returns(ElementState.Active);

			var lockManagerElement = new LockManagerElement(connectionMock.Object, elementMock.Object, higherPrioLockRequestListener: higherPrioLockRequestListenerMock);

			// Act
			lockManagerElement.ListenForLockRequestsWithHigherPriorityThan(new ObjectIdAndPriority("objectId", Priority.Medium));

			bool higherPrioLockRequestCameIn = false;
			lockManagerElement.HigherPriorityLockRequestReceived += (sender, args) =>
			{
				if (args.LockObjectRequest.ObjectId == "objectId" && args.LockObjectRequest.Priority > Priority.Medium)
				{
					higherPrioLockRequestCameIn = true;
				}
			};

			higherPrioLockRequestListenerMock.SpoofLockRequest(new LockObjectRequest
			{
				ObjectId = "objectId",
				Priority = Priority.Medium,
			});

			// Assert
			Assert.IsFalse(higherPrioLockRequestCameIn);
		}

		[TestMethod()]
		public void LockObject_WithoutWait_NoSubscriptionMade()
		{
			// Arrange
			var connectionMock = new Mock<IConnection>();

			var unlockListenerMock = new UnlockListenerMock();

			var elementMock = new Mock<IDmsElement>();
			elementMock.SetupGet(e => e.State).Returns(ElementState.Active);

			var interappHandlerMock = new Mock<IInterAppHandler>();
			interappHandlerMock.Setup(m => m.SendMessageWithResponse<LockObjectsResponsesMessage>(It.IsAny<LockObjectsRequestsMessage>())).Returns(new LockObjectsResponsesMessage
			{
				Responses = new List<LockObjectResponse> 
				{
					new LockObjectResponse
					{
						ObjectId = "objectId",
						LockIsGranted = true,
					}
				}
			});

			var lockManagerElement = new LockManagerElement(connectionMock.Object, elementMock.Object, unlockListenerMock, interappHandlerMock.Object);

			// Act
			var lockObjectRequest = new LockObjectRequest
			{
				ObjectId = "objectId",
			};

			lockManagerElement.LockObject(lockObjectRequest, maxWaitingTime: null);

			// Assert
			Assert.AreEqual(0, unlockListenerMock.AmountOfTimesMonitorStarted);
			Assert.AreEqual(0, unlockListenerMock.AmountOfTimesMonitorStopped);
		}

		[TestMethod()]
		public void LockObject_WithWait_SubscriptionMade()
		{
			// Arrange
			var connectionMock = new Mock<IConnection>();

			var unlockListenerMock = new UnlockListenerMock();

			var elementMock = new Mock<IDmsElement>();
			elementMock.SetupGet(e => e.State).Returns(ElementState.Active);

			var interappHandlerMock = new Mock<IInterAppHandler>();
			interappHandlerMock.Setup(m => m.SendMessageWithResponse<LockObjectsResponsesMessage>(It.IsAny<LockObjectsRequestsMessage>())).Returns(new LockObjectsResponsesMessage
			{
				Responses = new List<LockObjectResponse>
				{
					new LockObjectResponse
					{
						ObjectId = "objectId",
						LockIsGranted = false,
					}
				}
			});

			var lockManagerElement = new LockManagerElement(connectionMock.Object, elementMock.Object, unlockListenerMock, interappHandlerMock.Object);

			// Act
			var lockObjectRequest = new LockObjectRequest
			{
				ObjectId = "objectId",
			};

			lockManagerElement.LockObject(lockObjectRequest, maxWaitingTime: TimeSpan.FromSeconds(1));

			// Assert
			Assert.AreEqual(1, unlockListenerMock.AmountOfTimesMonitorStarted);
			Assert.AreEqual(1, unlockListenerMock.AmountOfTimesMonitorStopped);
		}
	}
}
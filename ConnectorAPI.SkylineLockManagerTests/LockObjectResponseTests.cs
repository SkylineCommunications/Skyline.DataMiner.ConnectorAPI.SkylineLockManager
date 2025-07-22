namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManagerTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Messages.Locking;

	[TestClass]
	public class LockObjectResponseTests
	{
		[TestMethod]
		public void Flatten_SingleObject_ReturnsSelf()
		{
			// Arrange
			var response = new LockObjectResponse { ObjectId = "A" };

			// Act
			var result = response.Flatten().ToList();

			// Assert
			Assert.AreEqual(1, result.Count);
			Assert.AreSame(response, result[0]);
		}

		[TestMethod]
		public void Flatten_NestedObjects_ReturnsAllInDepthFirstOrder()
		{
			// Arrange
			var root = new LockObjectResponse { ObjectId = "Root" };
			var child1 = new LockObjectResponse { ObjectId = "Child1" };
			var child2 = new LockObjectResponse { ObjectId = "Child2" };
			var grandChild1 = new LockObjectResponse { ObjectId = "GrandChild1" };
			var grandChild2 = new LockObjectResponse { ObjectId = "GrandChild2" };

			child1.LinkedObjectResponses.Add(grandChild1);
			child2.LinkedObjectResponses.Add(grandChild2);
			root.LinkedObjectResponses.Add(child1);
			root.LinkedObjectResponses.Add(child2);

			// Act
			var result = root.Flatten().ToList();

			// Assert
			CollectionAssert.AreEqual(
				new[] { root, child1, grandChild1, child2, grandChild2 },
				result
			);
		}

		[TestMethod]
		public void Flatten_EmptyLinkedObjectResponses_ReturnsSelfOnly()
		{
			// Arrange
			var response = new LockObjectResponse { ObjectId = "A" };
			response.LinkedObjectResponses = new List<LockObjectResponse>();

			// Act
			var result = response.Flatten().ToList();

			// Assert
			Assert.AreEqual(1, result.Count);
			Assert.AreSame(response, result[0]);
		}

		[TestMethod]
		public void Flatten_NullLinkedObjectResponses_ReturnsSelfOnly()
		{
			// Arrange
			var response = new LockObjectResponse { ObjectId = "A", LinkedObjectResponses = null };

			// Act & Assert
			// Should not throw
			var result = response.Flatten().ToList();
			Assert.AreEqual(1, result.Count);
			Assert.AreSame(response, result[0]);
		}
	}
}

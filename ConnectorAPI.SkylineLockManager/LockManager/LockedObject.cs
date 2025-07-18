
namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.LockManager
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public class LockedObject
	{
		public string ObjectId { get; set; }

		public string ObjectDescription { get; set; }

		public string LockHolderInfo { get; set; }

		public DateTime Timestamp { get; set; }

		public IReadOnlyCollection<string> LinkedObjectIds { get; set; }

		public DateTime AutoUnlockTimestamp { get; set; }

		public Priority Priority { get; set; }
	}
}

namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager
{
	public interface ILockBaseInfo
	{
		string ContextInfo { get; set; }
		string ObjectDescription { get; set; }
		string ObjectId { get; set; }
		int Priority { get; set; }
	}
}
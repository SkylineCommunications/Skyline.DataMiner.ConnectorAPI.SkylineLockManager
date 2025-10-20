namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.InterApp.Receiving
{
	public class InterAppReceiverFactory
	{
		public static IInterAppReceiver Create()
		{
			return new InterAppReceiver();
		}
	}
}
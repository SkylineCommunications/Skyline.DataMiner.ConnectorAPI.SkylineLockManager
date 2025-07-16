namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	/// <summary>
	/// Interface that defines object used for logging info about locks.
	/// </summary>
	public interface ILogger
	{
		/// <summary>
		/// Method for logging information about locks.
		/// </summary>
		/// <param name="className">Class name from which info is logged.</param>
		/// <param name="methodName">Method name from which info is logged.</param>
		/// <param name="line">Information that will be logged.</param>
		void Log(string className, string methodName, string line);
	}
}

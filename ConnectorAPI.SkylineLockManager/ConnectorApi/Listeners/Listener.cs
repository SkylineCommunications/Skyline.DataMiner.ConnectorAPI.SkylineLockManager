namespace Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners
{
	using System;
	using System.Diagnostics;
	using Microsoft.Extensions.Logging;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi;
	using Skyline.DataMiner.ConnectorAPI.SkylineLockManager.ConnectorApi.Listeners.Unlocks;

	/// <summary>
	/// Provides an abstract base class for implementing listeners that monitor specific events or conditions.
	/// </summary>
	/// <remarks>The <see cref="Listener"/> class defines a framework for creating custom listeners that can start
	/// and stop monitoring activities. Derived classes must implement the <see cref="StartMonitor"/> and <see
	/// cref="StopMonitor"/> methods to define the specific monitoring behavior. The class also implements the <see
	/// cref="IDisposable"/>  interface to ensure proper resource cleanup.  This class is not thread-safe. Derived classes
	/// should ensure thread safety if required.</remarks>
	public abstract class Listener : IDisposable
	{
		/// <summary>
		/// Represents a unique identifier for the source, generated as a new GUID string.
		/// </summary>
		/// <remarks>This field is initialized with a unique value when the containing object is created.  It is
		/// intended to uniquely identify the source within the context of the application.</remarks>
		protected readonly string sourceId = Guid.NewGuid().ToString();

		/// <summary>
		/// Represents the logger instance used for logging messages within the class.
		/// </summary>
		protected readonly ILogger logger;

		/// <summary>
		/// Indicates whether the service is currently listening for incoming connections or events.
		/// </summary>
		/// <remarks>This property is intended for use within derived classes to check or manage the listening
		/// state.</remarks>
		protected bool isListening;

		/// <summary>
		/// Indicates whether the object has been disposed.
		/// </summary>
		/// <remarks>This field is used to track whether the object has already been disposed to prevent multiple 
		/// disposal operations. It is typically used in the implementation of the <see cref="IDisposable"/>
		/// pattern.</remarks>
		protected bool disposedValue;

		private bool startingListen;
		private bool stoppingListen;

		/// <summary>
		/// Initializes a new instance of the <see cref="Listener"/> class with an optional logger.
		/// </summary>
		/// <param name="logger">An optional <see cref="ILogger"/> instance used for logging. If null, no logging will be performed.</param>
		protected Listener(ILogger logger = null)
		{
			this.logger = logger;
		}

		/// <summary>
		/// Disposes the <see cref="UnlockListener"/> instance, releasing any resources it holds.
		/// </summary>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Starts the monitoring process for the implementing class.
		/// </summary>
		/// <remarks>This method must be implemented by derived classes to define the specific behavior for initiating
		/// monitoring. The implementation should ensure that any necessary resources are initialized and monitoring begins as
		/// expected.</remarks>
		protected abstract void StartMonitor();

		/// <summary>
		/// Stops the monitoring process for the implementing class.
		/// </summary>
		/// <remarks>This method is abstract and must be implemented by a derived class to define the specific
		/// behavior for stopping the monitoring process. Ensure that any resources or operations associated with monitoring
		/// are properly released or terminated in the implementation.</remarks>
		protected abstract void StopMonitor();

		/// <summary>
		/// Releases the resources used by the current instance of the class.
		/// </summary>
		/// <remarks>This method should be called when the instance is no longer needed to ensure proper cleanup of
		/// resources. If the instance is used after calling this method, it may result in undefined behavior.</remarks>
		protected abstract void Dispose(bool disposing);

		/// <summary>
		/// Starts the listening process if it is not already active.
		/// </summary>
		/// <remarks>This method ensures that the listening process is initiated only once,  preventing re-entrancy by
		/// checking the current state. If the process is  already active or in the process of starting, the method exits
		/// without  performing any action.</remarks>
		protected void StartListening()
		{
			if (isListening || startingListen)
			{
				// Avoid re-entrancy
				return;
			}

			startingListen = true;

			StartMonitor();

			isListening = true;
			startingListen = false;
		}

		/// <summary>
		/// Stops the listening process if it is currently active.
		/// </summary>
		/// <remarks>This method ensures that the listening process is stopped safely, avoiding re-entrancy issues. It
		/// has no effect if the listening process is not active.</remarks>
		protected void StopListening()
		{
			if (!isListening || stoppingListen)
			{
				// Avoid re-entrancy
				return;
			}

			stoppingListen = true;

			StopMonitor();

			isListening = false;
			stoppingListen = false;
		}

		/// <summary>
		/// Logs a message along with the name of the calling method and the current context.
		/// </summary>
		/// <remarks>This method captures the name of the calling method and logs it alongside the provided message.
		/// The logging behavior depends on the implementation of the <c>logger</c> instance.</remarks>
		/// <param name="message">The message to log. Cannot be null or empty.</param>
		/// <param name="logLevel">The severity level of the log entry.</param>
		protected void Log(string message, LogLevel logLevel = LogLevel.Debug)
		{
			if (logger == null)
			{
				return;
			}

			string nameOfMethod = new StackTrace().GetFrame(1).GetMethod().Name;

			lock (logger)
			{
				logger.Log(logLevel, "{className}|{methodName}|{message}", GetType().Name, nameOfMethod, message);
			}
		}
	}
}

using System;
namespace net.vieapps.Services
{
	/// <summary>
	/// Presents a service component
	/// </summary>
	public interface IServiceComponent : IDisposable
	{
		/// <summary>
		/// Gets the name
		/// </summary>
		string ServiceName { get; }

		/// <summary>
		/// Gets the URI
		/// </summary>
		string ServiceURI { get; }

		/// <summary>
		/// Gets or sets the logger
		/// </summary>
		Microsoft.Extensions.Logging.ILogger Logger { get; set; }

		/// <summary>
		/// Starts the service
		/// </summary>
		/// <param name="args">The arguments</param>
		/// <param name="initializeRepository">true to initialize the repository of the service</param>
		/// <param name="nextAsync">The next action to run on started</param>
		void Start(string[] args = null, bool initializeRepository = true, Func<IService, System.Threading.Tasks.Task> nextAsync = null);

		/// <summary>
		/// Stops the service
		/// </summary>
		/// <param name="args">The arguments</param>
		void Stop(string[] args = null);
	}
}
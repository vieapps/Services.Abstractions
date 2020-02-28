using System;
using Microsoft.Extensions.Logging;
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
		ILogger Logger { get; set; }

		/// <summary>
		/// Starts the service
		/// </summary>
		/// <param name="args">The arguments</param>
		/// <param name="initializeRepository">true to initialize the repository of the service</param>
		/// <param name="next">The next action to run when the service was started</param>
		void Start(string[] args = null, bool initializeRepository = true, Action<IService> next = null);

		/// <summary>
		/// Stops the service
		/// </summary>
		/// <param name="args">The arguments</param>
		void Stop(string[] args = null);
	}
}
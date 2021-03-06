﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
namespace net.vieapps.Services
{
	/// <summary>
	/// Presents a service component in the VIEApps NGX
	/// </summary>
	public interface IServiceComponent : IDisposable, IAsyncDisposable
	{
		/// <summary>
		/// Gets the identity of the node
		/// </summary>
		string NodeID { get; }

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
		/// Gets the cancellation token
		/// </summary>
		System.Threading.CancellationToken CancellationToken { get; }

		/// <summary>
		/// Registers the service with API Gateway
		/// </summary>
		/// <param name="args">The arguments for registering</param>
		/// <param name="onSuccess">The action to run when the service was registered successful</param>
		/// <param name="onError">The action to run when got any error</param>
		/// <returns></returns>
		Task RegisterServiceAsync(IEnumerable<string> args, Action<IService> onSuccess = null, Action<Exception> onError = null);

		/// <summary>
		/// Unregisters the service with API Gateway
		/// </summary>
		/// <param name="args">The arguments for unregistering</param>
		/// <param name="available">true to mark the service still available</param>
		/// <param name="onSuccess">The action to run when the service was unregistered successful</param>
		/// <param name="onError">The action to run when got any error</param>
		Task UnregisterServiceAsync(IEnumerable<string> args, bool available = true, Action<IService> onSuccess = null, Action<Exception> onError = null);

		/// <summary>
		/// Starts the service (the short way - connect to API Gateway and register the service)
		/// </summary>
		/// <param name="args">The arguments</param>
		/// <param name="initializeRepository">true to initialize the repository of the service</param>
		/// <param name="next">The next action to run when the service was started</param>
		Task StartAsync(string[] args = null, bool initializeRepository = true, Action<IService> next = null);

		/// <summary>
		/// Starts the service (the short way - connect to API Gateway and register the service)
		/// </summary>
		/// <param name="args">The arguments</param>
		/// <param name="initializeRepository">true to initialize the repository of the service</param>
		/// <param name="next">The next action to run when the service was started</param>
		void Start(string[] args = null, bool initializeRepository = true, Action<IService> next = null);

		/// <summary>
		/// Stops the service (unregister the service, disconnect from API Gateway and do the clean-up tasks)
		/// </summary>
		/// <param name="args">The arguments</param>
		/// <param name="next">The next action to run when the service was stopped</param>
		Task StopAsync(string[] args = null, Action<IService> next = null);

		/// <summary>
		/// Stops the service (unregister the service, disconnect from API Gateway and do the clean-up tasks)
		/// </summary>
		/// <param name="args">The arguments</param>
		/// <param name="next">The next action to run when the service was stopped</param>
		void Stop(string[] args = null, Action<IService> next = null);

		/// <summary>
		/// Does the synchronous work when the service starts with '/do-sync-work' argument
		/// </summary>
		/// <param name="args">The arguments</param>
		void DoWork(string[] args = null);

		/// <summary>
		/// Disposes the service (unregister the service, disconnect from API Gateway and do the clean-up tasks)
		/// </summary>
		/// <param name="args">The arguments</param>
		/// <param name="available">true to mark the service still available</param>
		/// <param name="disconnect">true to disconnect from API Gateway Router and close all WAMP channels</param>
		/// <param name="next">The next action to run when the service was disposed</param>
		ValueTask DisposeAsync(string[] args, bool available = true, bool disconnect = true, Action<IService> next = null);

		/// <summary>
		/// Disposes the service (unregister the service, disconnect from API Gateway and do the clean-up tasks)
		/// </summary>
		/// <param name="args">The arguments</param>
		/// <param name="available">true to mark the service still available</param>
		/// <param name="disconnect">true to disconnect from API Gateway Router and close all WAMP channels</param>
		/// <param name="next">The next action to run when the service was disposed</param>
		void Dispose(string[] args, bool available = true, bool disconnect = true, Action<IService> next = null);
	}
}
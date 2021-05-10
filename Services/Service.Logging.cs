using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using WampSharp.V2.Rpc;
namespace net.vieapps.Services
{
	/// <summary>
	/// Presents a logging service
	/// </summary>
	public interface ILoggingService
	{
		/// <summary>
		/// Writes the log into centralized log storage
		/// </summary>
		/// <param name="correlationID">The identity for tracking the correlation</param>
		/// <param name="developerID">The identity of the developer</param>
		/// <param name="appID">The identity of the app</param>
		/// <param name="serviceName">The name of service</param>
		/// <param name="objectName">The name of serivice's object</param>
		/// <param name="log">The log message</param>
		/// <param name="stack">The simple stack (usually is Exception.StackTrace)</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.logging.single")]
		Task WriteLogAsync(string correlationID, string developerID, string appID, string serviceName, string objectName, string log, string stack = null, CancellationToken cancellationToken = default);

		/// <summary>
		/// Writes the logs into centralized log storage
		/// </summary>
		/// <param name="correlationID">The identity of correlation</param>
		/// <param name="developerID">The identity of the developer</param>
		/// <param name="appID">The identity of the app</param>
		/// <param name="serviceName">The name of service</param>
		/// <param name="objectName">The name of serivice's object</param>
		/// <param name="logs">The collection of log messages</param>
		/// <param name="stack">The simple stack (usually is Exception.StackTrace)</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.logging.multiple")]
		Task WriteLogsAsync(string correlationID, string developerID, string appID, string serviceName, string objectName, List<string> logs, string stack = null, CancellationToken cancellationToken = default);
	}
}
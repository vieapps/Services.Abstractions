using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using WampSharp.V2.Rpc;
using net.vieapps.Components.Security;
namespace net.vieapps.Services
{
	/// <summary>
	/// Presents a business service in the VIEApps NGX
	/// </summary>
	public interface IService : IDisposable, IAsyncDisposable
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
		/// Gets the logger
		/// </summary>
		ILogger Logger { get; }

		/// <summary>
		/// Processes a request
		/// </summary>
		/// <param name="requestInfo">The requesting information</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns>The JSON object that contains the result</returns>
		[WampProcedure("services.{0}")]
		Task<JToken> ProcessRequestAsync(RequestInfo requestInfo, CancellationToken cancellationToken = default);

		/// <summary>
		/// Processes a web-hook message
		/// </summary>
		/// <param name="requestInfo">The requesting information</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.webhook")]
		Task ProcessWebHookMessageAsync(RequestInfo requestInfo, CancellationToken cancellationToken = default);

		/// <summary>
		/// Determines the user is able to manage or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="entityInfo">The identity of a specified business repository entity (means a business content-type at run-time) or type-name of an entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.manage")]
		Task<bool> CanManageAsync(User user, string objectName, string systemID, string entityInfo, string objectID, CancellationToken cancellationToken = default);

		/// <summary>
		/// Determines the user is able to moderate or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="entityInfo">The identity of a specified business repository entity (means a business content-type at run-time) or type-name of an entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.moderate")]
		Task<bool> CanModerateAsync(User user, string objectName, string systemID, string entityInfo, string objectID, CancellationToken cancellationToken = default);

		/// <summary>
		/// Determines the user is able to edit or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="entityInfo">The identity of a specified business repository entity (means a business content-type at run-time) or type-name of an entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.edit")]
		Task<bool> CanEditAsync(User user, string objectName, string systemID, string entityInfo, string objectID, CancellationToken cancellationToken = default);

		/// <summary>
		/// Determines the user is able to contribute or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="entityInfo">The identity of a specified business repository entity (means a business content-type at run-time) or type-name of an entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.contribute")]
		Task<bool> CanContributeAsync(User user, string objectName, string systemID, string entityInfo, string objectID, CancellationToken cancellationToken = default);

		/// <summary>
		/// Determines the user is able to view or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="entityInfo">The identity of a specified business repository entity (means a business content-type at run-time) or type-name of an entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.view")]
		Task<bool> CanViewAsync(User user, string objectName, string systemID, string entityInfo, string objectID, CancellationToken cancellationToken = default);

		/// <summary>
		/// Determines the user is able to download or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="entityInfo">The identity of a specified business repository entity (means a business content-type at run-time) or type-name of an entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.download")]
		Task<bool> CanDownloadAsync(User user, string objectName, string systemID, string entityInfo, string objectID, CancellationToken cancellationToken = default);
	}
}
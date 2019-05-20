using System.Threading;
using System.Threading.Tasks;
using WampSharp.V2.Rpc;
using Newtonsoft.Json.Linq;
using net.vieapps.Components.Security;
namespace net.vieapps.Services
{
	/// <summary>
	/// Presents a business service
	/// </summary>
	public interface IService : System.IDisposable
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
		Microsoft.Extensions.Logging.ILogger Logger { get; }

		/// <summary>
		/// Process the request
		/// </summary>
		/// <param name="requestInfo">The requesting Information</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}")]
		Task<JToken> ProcessRequestAsync(RequestInfo requestInfo, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Determines the user is able to manage or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the business entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.manage")]
		Task<bool> CanManageAsync(User user, string objectName, string systemID, string definitionID, string objectID, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Determines the user is able to moderate or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the business entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.moderate")]
		Task<bool> CanModerateAsync(User user, string objectName, string systemID, string definitionID, string objectID, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Determines the user is able to edit or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the business entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.edit")]
		Task<bool> CanEditAsync(User user, string objectName, string systemID, string definitionID, string objectID, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Determines the user is able to contribute or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the business entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.contribute")]
		Task<bool> CanContributeAsync(User user, string objectName, string systemID, string definitionID, string objectID, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Determines the user is able to view or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the business entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.view")]
		Task<bool> CanViewAsync(User user, string objectName, string systemID, string definitionID, string objectID, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Determines the user is able to download or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the business entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.download")]
		Task<bool> CanDownloadAsync(User user, string objectName, string systemID, string definitionID, string objectID, CancellationToken cancellationToken = default(CancellationToken));
	}
}
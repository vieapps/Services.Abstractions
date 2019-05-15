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
		/// Gets the name of this service (for working with WAMP)
		/// </summary>
		string ServiceName { get; }

		/// <summary>
		/// Gets the URI of this service (with full namespace - for working with WAMP)
		/// </summary>
		string ServiceURI { get; }

		/// <summary>
		/// Gets the logger
		/// </summary>
		Microsoft.Extensions.Logging.ILogger Logger { get; }

		/// <summary>
		/// Process the request of this service
		/// </summary>
		/// <param name="requestInfo">The requesting Information</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}")]
		Task<JToken> ProcessRequestAsync(RequestInfo requestInfo, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Gets the state that determines the user is able to manage or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the business entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.manage")]
		Task<bool> CanManageAsync(IUser user, string objectName, string systemID, string definitionID, string objectID, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Gets the state that determines the user is able to manage or not
		/// </summary>
		/// <param name="requestInfo">The requesting information</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the business entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.manages")]
		Task<bool> CanManageAsync(RequestInfo requestInfo, string objectName, string systemID, string definitionID, string objectID, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Gets the state that determines the user is able to moderate or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the business entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.moderate")]
		Task<bool> CanModerateAsync(IUser user, string objectName, string systemID, string definitionID, string objectID, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Gets the state that determines the user is able to moderate or not
		/// </summary>
		/// <param name="requestInfo">The requesting information</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the business entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.moderates")]
		Task<bool> CanModerateAsync(RequestInfo requestInfo, string objectName, string systemID, string definitionID, string objectID, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Gets the state that determines the user is able to edit or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the business entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.edit")]
		Task<bool> CanEditAsync(IUser user, string objectName, string systemID, string definitionID, string objectID, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Gets the state that determines the user is able to edit or not
		/// </summary>
		/// <param name="requestInfo">The requesting information</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the business entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.edits")]
		Task<bool> CanEditAsync(RequestInfo requestInfo, string objectName, string systemID, string definitionID, string objectID, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Gets the state that determines the user is able to contribute or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the business entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.contribute")]
		Task<bool> CanContributeAsync(IUser user, string objectName, string systemID, string definitionID, string objectID, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Gets the state that determines the user is able to contribute or not
		/// </summary>
		/// <param name="requestInfo">The requesting information</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the business entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.contributes")]
		Task<bool> CanContributeAsync(RequestInfo requestInfo, string objectName, string systemID, string definitionID, string objectID, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Gets the state that determines the user is able to view or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the business entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.view")]
		Task<bool> CanViewAsync(IUser user, string objectName, string systemID, string definitionID, string objectID, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Gets the state that determines the user is able to view or not
		/// </summary>
		/// <param name="requestInfo">The requesting information</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the business entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.views")]
		Task<bool> CanViewAsync(RequestInfo requestInfo, string objectName, string systemID, string definitionID, string objectID, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Gets the state that determines the user is able to download the attachment files or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the business entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.download")]
		Task<bool> CanDownloadAsync(IUser user, string objectName, string systemID, string definitionID, string objectID, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Gets the state that determines the user is able to download or not
		/// </summary>
		/// <param name="requestInfo">The requesting information</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the business entity definition</param>
		/// <param name="objectID">The identity of the object</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.privileges.downloads")]
		Task<bool> CanDownloadAsync(RequestInfo requestInfo, string objectName, string systemID, string definitionID, string objectID, CancellationToken cancellationToken = default(CancellationToken));
	}
}
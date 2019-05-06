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
		/// <param name="requestInfo">Requesting Information</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("services.{0}")]
		Task<JToken> ProcessRequestAsync(RequestInfo requestInfo, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Gets the state that determines the user is able to manage or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="objectIdentity">The identity of the service's object</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.permissions.manage.object")]
		Task<bool> CanManageAsync(IUser user, string objectName, string objectIdentity);

		/// <summary>
		/// Gets the state that determines the user is able to manage or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the entity definition</param>
		/// <param name="objectID">The identity of the business object</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.permissions.manage.definition")]
		Task<bool> CanManageAsync(IUser user, string systemID, string definitionID, string objectID);

		/// <summary>
		/// Gets the state that determines the user is able to moderate or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="objectIdentity">The identity of the service's object</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.permissions.moderate.object")]
		Task<bool> CanModerateAsync(IUser user, string objectName, string objectIdentity);

		/// <summary>
		/// Gets the state that determines the user is able to moderate or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the entity definition</param>
		/// <param name="objectID">The identity of the business object</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.permissions.moderate.definition")]
		Task<bool> CanModerateAsync(IUser user, string systemID, string definitionID, string objectID);

		/// <summary>
		/// Gets the state that determines the user is able to edit or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="objectIdentity">The identity of the service's object</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.permissions.edit.object")]
		Task<bool> CanEditAsync(IUser user, string objectName, string objectIdentity);

		/// <summary>
		/// Gets the state that determines the user is able to edit or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the entity definition</param>
		/// <param name="objectID">The identity of the business object</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.permissions.edit.definition")]
		Task<bool> CanEditAsync(IUser user, string systemID, string definitionID, string objectID);

		/// <summary>
		/// Gets the state that determines the user is able to contribute or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="objectIdentity">The identity of the service's object</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.permissions.contribute.object")]
		Task<bool> CanContributeAsync(IUser user, string objectName, string objectIdentity);

		/// <summary>
		/// Gets the state that determines the user is able to contribute or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the entity definition</param>
		/// <param name="objectID">The identity of the business object</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.permissions.contribute.definition")]
		Task<bool> CanContributeAsync(IUser user, string systemID, string definitionID, string objectID);

		/// <summary>
		/// Gets the state that determines the user is able to view or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="objectIdentity">The identity of the service's object</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.permissions.view.object")]
		Task<bool> CanViewAsync(IUser user, string objectName, string objectIdentity);

		/// <summary>
		/// Gets the state that determines the user is able to view or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the entity definition</param>
		/// <param name="objectID">The identity of the business object</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.permissions.view.definition")]
		Task<bool> CanViewAsync(IUser user, string systemID, string definitionID, string objectID);

		/// <summary>
		/// Gets the state that determines the user is able to download or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="objectName">The name of the service's object</param>
		/// <param name="objectIdentity">The identity of the service's object</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.permissions.download.object")]
		Task<bool> CanDownloadAsync(IUser user, string objectName, string objectIdentity);

		/// <summary>
		/// Gets the state that determines the user is able to download the attachment files or not
		/// </summary>
		/// <param name="user">The user who performs the action</param>
		/// <param name="systemID">The identity of the business system</param>
		/// <param name="definitionID">The identity of the entity definition</param>
		/// <param name="objectID">The identity of the business object</param>
		/// <returns></returns>
		[WampProcedure("services.{0}.permissions.download.definition")]
		Task<bool> CanDownloadAsync(IUser user, string systemID, string definitionID, string objectID);
	}
}
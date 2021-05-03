using System.Threading;
using System.Threading.Tasks;
using WampSharp.V2.Rpc;
using Newtonsoft.Json.Linq;
namespace net.vieapps.Services
{
	/// <summary>
	/// Presents a syncable business service in the VIEApps NGX
	/// </summary>
	public interface ISyncableService : IService
	{
		/// <summary>
		/// Processes the sync request
		/// </summary>
		/// <param name="requestInfo">The requesting information</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns>The JSON object that contains the result</returns>
		[WampProcedure("services.{0}.sync")]
		Task<JToken> SyncAsync(RequestInfo requestInfo, CancellationToken cancellationToken = default);
	}
}
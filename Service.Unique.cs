using System.Threading;
using System.Threading.Tasks;
using WampSharp.V2.Rpc;
using Newtonsoft.Json.Linq;
namespace net.vieapps.Services
{
	/// <summary>
	/// Presents a unique business service (means a single instance of a business service at a specific host/controller)
	/// </summary>
	public interface IUniqueService : System.IDisposable
	{
		/// <summary>
		/// Gets the unique name of this service (for working with WAMP router)
		/// </summary>
		string ServiceUniqueName { get; }

		/// <summary>
		/// Gets the unique URI of this service (with full namespace - for working with WAMP router)
		/// </summary>
		string ServiceUniqueURI { get; }

		/// <summary>
		/// Process the request of this service
		/// </summary>
		/// <param name="requestInfo">Requesting Information</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("net.vieapps.services.{0}")]
		Task<JToken> ProcessRequestAsync(RequestInfo requestInfo, CancellationToken cancellationToken = default(CancellationToken));
	}
}
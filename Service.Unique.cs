using System.Threading;
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
		[WampSharp.V2.Rpc.WampProcedure("services.{0}")]
		System.Threading.Tasks.Task<Newtonsoft.Json.Linq.JToken> ProcessRequestAsync(RequestInfo requestInfo, CancellationToken cancellationToken = default(CancellationToken));
	}
}
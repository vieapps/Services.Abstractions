using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WampSharp.V2.Rpc;
namespace net.vieapps.Services
{
	/// <summary>
	/// Presents a unique business service (means a single instance of a business service at a specific host/controller)
	/// </summary>
	public interface IUniqueService : IDisposable
	{
		/// <summary>
		/// Gets the unique name
		/// </summary>
		string ServiceUniqueName { get; }

		/// <summary>
		/// Gets the unique URI
		/// </summary>
		string ServiceUniqueURI { get; }

		/// <summary>
		/// Processes the request
		/// </summary>
		/// <param name="requestInfo">The requesting information</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns>The JSON object that contains the result</returns>
		[WampProcedure("services.{0}")]
		Task<JToken> ProcessRequestAsync(RequestInfo requestInfo, CancellationToken cancellationToken = default);
	}
}
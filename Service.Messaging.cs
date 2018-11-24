using System.Threading;
using System.Threading.Tasks;
using WampSharp.V2.Rpc;
using Newtonsoft.Json.Linq;
using net.vieapps.Components.Utility;
namespace net.vieapps.Services
{
	/// <summary>
	/// Presents a messaging service for sending email and web-hook messages
	/// </summary>
	public interface IMessagingService
	{
		/// <summary>
		/// Sends an email message
		/// </summary>
		/// <param name="message">The email message for sending</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("net.vieapps.services.messaging.email")]
		Task SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Sends a web hook message
		/// </summary>
		/// <param name="message">The web hook message for sending</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		[WampProcedure("net.vieapps.services.messaging.webhook")]
		Task SendWebHookAsync(WebHookMessage message, CancellationToken cancellationToken = default(CancellationToken));
	}
}
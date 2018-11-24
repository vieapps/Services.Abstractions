using System.Dynamic;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
namespace net.vieapps.Services
{
	/// <summary>
	/// Presents the requesting information of a service
	/// </summary>
	public interface IRequestInfo
	{
		/// <summary>
		/// Gets or sets the session
		/// </summary>
		ISession Session { get; set; }

		/// <summary>
		/// Gets or sets the name of service
		/// </summary>
		string ServiceName { get; set; }

		/// <summary>
		/// Gets or sets the name of service's object
		/// </summary>
		string ObjectName { get; set; }

		/// <summary>
		/// Gets or sets the verb (GET/POST/PUT/DELETE)
		/// </summary>
		string Verb { get; set; }

		/// <summary>
		/// Gets or sets the query
		/// </summary>
		Dictionary<string, string> Query { get; set; }

		/// <summary>
		/// Gets or sets the header
		/// </summary>
		Dictionary<string, string> Header { get; set; }

		/// <summary>
		/// Gets or sets the JSON body of the request (only available when verb is POST or PUT)
		/// </summary>
		string Body { get; set; }

		/// <summary>
		/// Gets or sets the extra information
		/// </summary>
		Dictionary<string, string> Extra { get; set; }

		/// <summary>
		/// Gets or sets the identity of the correlation
		/// </summary>
		string CorrelationID { get; set; }
	}
}
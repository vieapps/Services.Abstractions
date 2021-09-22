using System;
using System.Dynamic;
using System.Globalization;
using System.Collections.Generic;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using net.vieapps.Components.Utility;
namespace net.vieapps.Services
{
	/// <summary>
	/// Presents the requesting information of a service
	/// </summary>
	public class RequestInfo : ServiceObjectBase
	{
		/// <summary>
		/// Initializes a requesting information
		/// </summary>
		public RequestInfo()
			: this(null) { }

		/// <summary>
		/// Initializes a requesting information
		/// </summary>
		/// <param name="requestInfo"></param>
		public RequestInfo(RequestInfo requestInfo)
			: this(requestInfo?.Session, requestInfo?.ServiceName, requestInfo?.ObjectName, requestInfo?.Verb, requestInfo?.Query, requestInfo?.Header, requestInfo?.Body, requestInfo?.Extra, requestInfo?.CorrelationID) { }

		/// <summary>
		/// Initializes a requesting information
		/// </summary>
		/// <param name="session"></param>
		/// <param name="serviceName"></param>
		/// <param name="objectName"></param>
		/// <param name="verb"></param>
		/// <param name="query"></param>
		/// <param name="header"></param>
		/// <param name="body"></param>
		/// <param name="extra"></param>
		/// <param name="correlationID"></param>
		public RequestInfo(Session session, string serviceName, string objectName = null, string verb = null, Dictionary<string, string> query = null, Dictionary<string, string> header = null, string body = null, Dictionary<string, string> extra = null, string correlationID = null)
		{
			this.Session = new Session(session);
			this.ServiceName = (serviceName ?? "unknown").GetCapitalizedFirstLetter();
			this.ObjectName = (objectName ?? "").Trim().GetCapitalizedFirstLetter();
			this.Verb = (verb ?? "GET").Trim().ToUpper();
			this.Query = new Dictionary<string, string>(query ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
			this.Header = new Dictionary<string, string>(header ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
			this.Body = string.IsNullOrWhiteSpace(body) ? "" : body;
			this.Extra = new Dictionary<string, string>(extra ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
			this.CorrelationID = string.IsNullOrWhiteSpace(correlationID) ? UtilityService.NewUUID : correlationID;
		}

		#region Properties
		/// <summary>
		/// Gets or sets the session
		/// </summary>
		public Session Session { get; set; }

		/// <summary>
		/// Gets or sets the name of service
		/// </summary>
		public string ServiceName { get; set; }

		/// <summary>
		/// Gets or sets the name of service's object
		/// </summary>
		public string ObjectName { get; set; }

		/// <summary>
		/// Gets or sets the verb (HEAD/GET/POST/PUT/DELETE/PATCH or whatever)
		/// </summary>
		public string Verb { get; set; }

		/// <summary>
		/// Gets or sets the query
		/// </summary>
		public Dictionary<string, string> Query { get; set; }

		/// <summary>
		/// Gets or sets the header
		/// </summary>
		public Dictionary<string, string> Header { get; set; }

		/// <summary>
		/// Gets or sets the body (JSON string) of the request (only available when verb is POST, PUT or PATCH)
		/// </summary>
		public string Body { get; set; }

		/// <summary>
		/// Gets or sets the extra information
		/// </summary>
		public Dictionary<string, string> Extra { get; set; }

		/// <summary>
		/// Gets or sets the identity of the correlation
		/// </summary>
		public string CorrelationID { get; set; }

		/// <summary>
		/// Gets the culture code (means language code)
		/// </summary>
		[JsonIgnore, XmlIgnore]
		public string CultureCode => this.Query.TryGetValue("language", out var language) && !string.IsNullOrWhiteSpace(language) ? language : "vi-VN";

		/// <summary>
		/// Gets the culture info (for working with language, number, calendar, ...)
		/// </summary>
		[JsonIgnore, XmlIgnore]
		public CultureInfo CultureInfo => CultureInfo.GetCultureInfo(this.CultureCode);

		/// <summary>
		/// Gets the body as JSON object
		/// </summary>
		[JsonIgnore, XmlIgnore]
		public JToken BodyAsJson => this.Body?.ToJson() ?? new JObject();

		/// <summary>
		/// Gets the body as ExpandoObject object
		/// </summary>
		[JsonIgnore, XmlIgnore]
		public ExpandoObject BodyAsExpandoObject => this.Body?.ToExpandoObject() ?? new ExpandoObject();

		/// <summary>
		/// Gets the object as JSON object
		/// </summary>
		[JsonIgnore, XmlIgnore]
		public JToken AsJson => this.ToJson(json =>
		{
			json["Body"] = this.BodyAsJson;
			json.Get<JObject>("Header")?.Remove("x-app-token");
		});

		/// <summary>
		/// Gets the object as ExpandoObject object
		/// </summary>
		[JsonIgnore, XmlIgnore]
		public ExpandoObject AsExpandoObject => this.ToExpandoObject(expando =>
		{
			expando.Set("Body", this.BodyAsExpandoObject);
			expando.Get<ExpandoObject>("Header")?.Remove("x-app-token");
		});
		#endregion

	}
}
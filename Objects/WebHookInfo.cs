using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MsgPack.Serialization;
namespace net.vieapps.Services
{
	public class WebHookInfo : ServiceObjectBase
	{
		public WebHookInfo() { }

		public string SignAlgorithm { get; set; } = "SHA256";

		public string SignKey { get; set; }

		public bool SignKeyIsHex { get; set; } = false;

		public string SignatureName { get; set; }

		public bool SignatureAsHex { get; set; } = true;

		public string SignaturePrefix { get; set; }

		public string SignatureSuffix { get; set; }

		public bool SignWithTimestamp { get; set; } = false;

		public string SignWithTimestampName { get; set; }

		public string SignWithTimestampConnect { get; set; }

		public string Query { get; set; }

		public string Header { get; set; }

		public string EncryptionKey { get; set; }

		public string EncryptionIV { get; set; }

		public string PrepareBodyScript { get; set; }

		public int PrepareBodyScriptTimeout { get; set; } = 0;

		[JsonIgnore, MessagePackIgnore]
		public JObject QueryAsJson => string.IsNullOrWhiteSpace(this.Query) ? null : JObject.Parse(this.Query);

		[JsonIgnore, MessagePackIgnore]
		public JObject HeaderAsJson => string.IsNullOrWhiteSpace(this.Header) ? null : JObject.Parse(this.Header);
	}
}
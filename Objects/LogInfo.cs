using System;

namespace net.vieapps.Services
{
	/// <summary>
	/// Presents a service log information
	/// </summary>
	public sealed class LogInfo : ServiceObjectBase
	{
		public string ID { get; set; }
		public DateTime Time { get; set; }
		public string CorrelationID { get; set; }
		public string DeveloperID { get; set; }
		public string AppID { get; set; }
		public string NodeID { get; set; }
		public string ServiceName { get; set; }
		public string ObjectName { get; set; }
		public string Logs { get; set; }
		public string Stack { get; set; }
	}
}
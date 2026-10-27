using System;
using System.Collections.Generic;

namespace net.vieapps.Services
{
	/// <summary>
	/// Presents a trace log information
	/// </summary>
	public sealed class TraceInfo : ServiceObjectBase
	{
		public string ID { get; set; }
		public string ParentID { get; set; }
		public string CorrelationID { get; set; }
		public string NodeID { get; set; }

		public string ServiceName { get; set; }
		public string Operation { get; set; }
		public string Kind { get; set; }

		public DateTime StartTime { get; set; }
		public DateTime? EndTime { get; set; }
		public double? DurationMs { get; set; }

		public string Status { get; set; }
		public string StatusDescription { get; set; }

		public Dictionary<string, object> Attributes { get; set; }
	}
}
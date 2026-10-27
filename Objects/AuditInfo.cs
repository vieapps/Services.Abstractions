using System;
using System.Collections.Generic;

namespace net.vieapps.Services
{
	public sealed class AuditInfo : ServiceObjectBase
	{
		public string ID { get; set; }
		public DateTime? Time { get; set; }

		public string CorrelationID { get; set; }
		public string TraceID { get; set; }

		public string DeveloperID { get; set; }
		public string AppID { get; set; }

		public string ActorType { get; set; }
		public string ActorID { get; set; }

		public string NodeID { get; set; }

		public string ServiceName { get; set; }
		public string ObjectName { get; set; }

		public string ObjectID { get; set; }
		public string SystemID { get; set; }

		public string Operation { get; set; }
		public string Status { get; set; }

		public Dictionary<string, object> Changes { get; set; }
		public Dictionary<string, object> Details { get; set; }
	}
}
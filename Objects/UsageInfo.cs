using System;
using System.Collections.Generic;

namespace net.vieapps.Services
{
	/// <summary>
	/// Presents an usage log information
	/// </summary>
	public sealed class UsageInfo : ServiceObjectBase
	{
		public string ID { get; set; }
		public string CorrelationID { get; set; }
		public string TraceID { get; set; }

		public string DeveloperID { get; set; }
		public string AppID { get; set; }
		public string UserID { get; set; }
		public string NodeID { get; set; }

		public string ServiceName { get; set; }
		public string ObjectName { get; set; }
		public string SystemID { get; set; }

		public string Type { get; set; }
		public string Operation { get; set; }
		public string Provider { get; set; }
		public string Model { get; set; }

		public int? Attempt { get; set; }
		public int? ModelRequests { get; set; }
		public int? ToolCalls { get; set; }
		public int? MCPCalls { get; set; }

		public long? InputTokens { get; set; }
		public long? CachedInputTokens { get; set; }
		public long? CacheCreationInputTokens { get; set; }
		public long? OutputTokens { get; set; }
		public long? ReasoningTokens { get; set; }
		public long? TotalTokens { get; set; }

		public long? InputBytes { get; set; }
		public long? OutputBytes { get; set; }

		public DateTime StartTime { get; set; }
		public DateTime? EndTime { get; set; }
		public double? DurationMs { get; set; }

		public decimal? ProviderCost { get; set; }
		public string ProviderCurrency { get; set; }
		public string PricingVersion { get; set; }

		public string Status { get; set; }

		public Dictionary<string, object> Details { get; set; }
	}
}
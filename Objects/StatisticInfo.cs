using System;
using System.Linq;
using System.Collections.Generic;

namespace net.vieapps.Services
{
	/// <summary>
	/// Presents basic information of a statistic
	/// </summary>
	public class StatisticInfo : ServiceObjectBase
	{
		public StatisticInfo() { }

		/// <summary>
		/// Gets or sets the name of statistic
		/// </summary>
		public string Name { get; set; } = "";

		/// <summary>
		/// Gets or sets the counter of statistic
		/// </summary>
		public int Counters { get; set; } = 0;
	}

	/// <summary>
	/// Presents a statistic message
	/// </summary>
	public sealed class StatisticMessage
	{
		public DateTime Time;
		public string ServiceName;
		public string NodeID;
		public int Nodes;
		public bool UseL1Cache = false;
		public bool IsHttp = true;

		public int ThreadPoolWorkers;
		public int ThreadPoolAsyncIO;
		public int ThreadPoolMaxWorkers;
		public int ThreadPoolMaxAsyncIO;

		public long RequestsTotal;
		public long RequestsInFlight;
		public double RequestsRate;

		public string CacheProvider;
		public string CacheStatus;
		public long CacheTotalQueue;
		public long CacheInteractiveQueue;
		public long CachePingMilliseconds;

		public long L1Hit304;
		public long L1Hit200;
		public long L1Miss;
		public double L1HitRatio;
		public long L2Hit304;
		public long L2Hit200;
		public long L2Miss;
		public double L2HitRatio;

		public long RpcGateMax;
		public long RpcGateCurrent;
		public long RpcGateAvailable;

		public long RpcEntered;
		public double RpcEnteredRate;
		public long RpcCompleted;
		public double RpcCompletedRate;
		public long RpcInFlight;
		public long RpcRejected;
		public double RpcAvgLatency;
		public long RpcMaxLatency;
	}

	public static class StatisticMessageExtension
	{
		public static IEnumerable<StatisticMessage> Aggregate(this IEnumerable<StatisticMessage> messages)
		{
			var result = new List<StatisticMessage>();

			var groups = messages.GroupBy(message => new
			{
				message.ServiceName
			});

			foreach (var group in groups)
			{
				int threadpoolWorkers = 0, threadpoolAsyncIO = 0, threadpoolMaxWorkers = 0, threadpoolMaxAsyncIO = 0;

				double requestsRate = 0;
				long requestsTotal = 0, requestsInFlight = 0;

				string cacheProvider = "Redis", cacheStatus = "OK";
				long cacheTotalQueue = 0, cacheInteractiveQueue = 0, cachePingMilliseconds = 0;

				long l1Hit304 = 0, l1Hit200 = 0, l1Miss = 0;
				long l2Hit304 = 0, l2Hit200 = 0, l2Miss = 0;

				long rpcGateMax = 0, rpcGateCurrent = 0, rpcGateAvailable = 0;

				double rpcIn = 0, rpcOut = 0;
				long rpcEntered = 0, rpcCompleted = 0, rpcInFlight = 0, rpcRejected = 0;

				double rpcWeightedLatency = 0;
				long rpcTotalCompleted = 0, rpcMaxLatency = 0;

				var useL1Cache = false;
				foreach (var message in group)
				{
					useL1Cache = useL1Cache || message.UseL1Cache;

					threadpoolWorkers += message.ThreadPoolWorkers;
					threadpoolAsyncIO += message.ThreadPoolAsyncIO;
					threadpoolMaxWorkers += message.ThreadPoolMaxWorkers;
					threadpoolMaxAsyncIO += message.ThreadPoolMaxAsyncIO;

					requestsTotal += message.RequestsTotal;
					requestsInFlight += message.RequestsInFlight;
					requestsRate += message.RequestsRate;

					cacheProvider = message.CacheProvider;
					cacheStatus = cacheStatus == "OK" && message.CacheStatus != "OK" ? message.CacheStatus : cacheStatus;
					cacheTotalQueue = message.CacheTotalQueue > cacheTotalQueue ? message.CacheTotalQueue : cacheTotalQueue;
					cacheInteractiveQueue = message.CacheInteractiveQueue > cacheInteractiveQueue ? message.CacheInteractiveQueue : cacheInteractiveQueue;
					cachePingMilliseconds = message.CachePingMilliseconds > cachePingMilliseconds ? message.CachePingMilliseconds : cachePingMilliseconds;

					l1Hit304 += message.L1Hit304;
					l1Hit200 += message.L1Hit200;
					l1Miss += message.L1Miss;
					l2Hit304 += message.L2Hit304;
					l2Hit200 += message.L2Hit200;
					l2Miss += message.L2Miss;

					rpcGateMax += message.RpcGateMax;
					rpcGateCurrent += message.RpcGateCurrent;
					rpcGateAvailable += message.RpcGateAvailable;

					rpcEntered += message.RpcEntered;
					rpcCompleted += message.RpcCompleted;
					rpcInFlight += message.RpcInFlight;
					rpcRejected += message.RpcRejected;

					rpcIn += message.RpcEnteredRate;
					rpcOut += message.RpcCompletedRate;

					if (message.RpcCompleted > 0)
					{
						rpcWeightedLatency += message.RpcAvgLatency * message.RpcCompleted;
						rpcTotalCompleted += message.RpcCompleted;
					}

					if (message.RpcMaxLatency > rpcMaxLatency)
						rpcMaxLatency = message.RpcMaxLatency;
				}

				var rpcAvgLatency = rpcTotalCompleted > 0
					? rpcWeightedLatency / rpcTotalCompleted
					: 0;

				var serviceName = group.Key.ServiceName;
				var numberOfNodes = group.Select(message => message.NodeID).Distinct(StringComparer.OrdinalIgnoreCase).Count();

				result.Add(new StatisticMessage
				{
					ServiceName = serviceName,
					Nodes = numberOfNodes,

					ThreadPoolWorkers = threadpoolWorkers,
					ThreadPoolAsyncIO = threadpoolAsyncIO,
					ThreadPoolMaxWorkers = threadpoolMaxWorkers / numberOfNodes,
					ThreadPoolMaxAsyncIO = threadpoolMaxAsyncIO / numberOfNodes,

					RequestsTotal = requestsTotal,
					RequestsInFlight = requestsInFlight,
					RequestsRate = requestsRate,

					CacheProvider = cacheProvider,
					CacheStatus = cacheStatus,
					CacheTotalQueue = cacheTotalQueue,
					CacheInteractiveQueue = cacheInteractiveQueue,
					CachePingMilliseconds = cachePingMilliseconds,

					L1Hit304 = l1Hit304,
					L1Hit200 = l1Hit200,
					L1Miss = l1Miss,
					L1HitRatio = requestsTotal > 0 ? (l1Hit304 + l1Hit200) * 100.0 / requestsTotal : 0,
					L2Hit304 = l2Hit304,
					L2Hit200 = l2Hit200,
					L2Miss = l2Miss,
					L2HitRatio = useL1Cache
						? l1Miss > 0 ? (l2Hit304 + l2Hit200) * 100.0 / l1Miss : 0
						: requestsTotal > 0 ? (l2Hit304 + l2Hit200) * 100.0 / requestsTotal : 0,

					RpcGateMax = rpcGateMax,
					RpcGateCurrent = rpcGateCurrent,
					RpcGateAvailable = rpcGateAvailable,

					RpcEntered = rpcEntered,
					RpcEnteredRate = rpcIn,
					RpcCompleted = rpcCompleted,
					RpcCompletedRate = rpcOut,
					RpcInFlight = rpcInFlight,
					RpcRejected = rpcRejected,

					RpcAvgLatency = rpcAvgLatency,
					RpcMaxLatency = rpcMaxLatency
				});
			}

			return result;
		}
	}
}
using System;
using System.Linq;
using System.Threading.Channels;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using net.vieapps.Components.Utility;

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

		public long CacheL1Hit304;
		public long CacheL1Hit200;
		public long CacheL1Miss;
		public double CacheL1HitRatio;
		public long CacheL2Hit304;
		public long CacheL2Hit200;
		public long CacheL2Miss;
		public double CacheL2HitRatio;

		public long RpcGateCurrent;
		public long RpcGateAvailable;
		public long RpcGateMax;

		public long RpcEntered;
		public double RpcEnteredRate;
		public long RpcCompleted;
		public double RpcCompletedRate;
		public long RpcInFlight;
		public long RpcRejected;
		public double RpcAverageLatency;
		public long RpcMaxLatency;
	}

	public static class StatisticMessageExtension
	{
		public static (IEnumerable<StatisticMessage> ForAggregate, IEnumerable<StatisticMessage> ForReUpdate) GetMessages(this Channel<StatisticMessage> messages, DateTime? time = null)
		{
			var forReUpdate = new List<StatisticMessage>();
			var forAggregate = new List<StatisticMessage>();
			time = time != null ? time : DateTime.Now.AddMinutes(-1);
			time = new DateTime(time.Value.Year, time.Value.Month, time.Value.Day, time.Value.Hour, time.Value.Minute, 0);

			while (messages.Reader.TryRead(out var message))
			{
				if (message.Time.Hour == time.Value.Hour && message.Time.Minute == time.Value.Minute)
					forAggregate.Add(message);
				else if (message.Time > time.Value)
					forReUpdate.Add(message);
			}

			return (forAggregate, forReUpdate);
		}

		public static IEnumerable<StatisticMessage> Aggregate(this IEnumerable<IGrouping<string, StatisticMessage>> messagesGroups)
		{
			var statistics = new List<StatisticMessage>();

			foreach (var messagesGroup in messagesGroups)
			{
				int threadpoolWorkers = 0, threadpoolAsyncIO = 0, threadpoolMaxWorkers = 0, threadpoolMaxAsyncIO = 0;

				double requestsRate = 0;
				long requestsTotal = 0, requestsInFlight = 0;

				string cacheProvider = "Redis", cacheStatus = "OK";
				long cacheTotalQueue = 0, cacheInteractiveQueue = 0, cachePingMilliseconds = 0;

				long cacheL1Hit304 = 0, cacheL1Hit200 = 0, cacheL1Miss = 0;
				long cacheL2Hit304 = 0, cacheL2Hit200 = 0, cacheL2Miss = 0;

				long rpcGateMax = 0, rpcGateCurrent = 0, rpcGateAvailable = 0;

				double rpcIn = 0, rpcOut = 0;
				long rpcEntered = 0, rpcCompleted = 0, rpcInFlight = 0, rpcRejected = 0;

				double rpcWeightedLatency = 0;
				long rpcTotalCompleted = 0, rpcMaxLatency = 0;

				var useL1Cache = false;
				var numberOfMessages = 0;
				foreach (var message in messagesGroup)
				{
					numberOfMessages++;
					useL1Cache = useL1Cache || message.UseL1Cache;

					threadpoolWorkers += message.ThreadPoolWorkers;
					threadpoolAsyncIO += message.ThreadPoolAsyncIO;
					threadpoolMaxWorkers = Math.Max(threadpoolMaxWorkers, message.ThreadPoolMaxWorkers);
					threadpoolMaxAsyncIO = Math.Max(threadpoolMaxAsyncIO, message.ThreadPoolMaxAsyncIO);

					requestsTotal += message.RequestsTotal;
					requestsInFlight += message.RequestsInFlight;
					requestsRate += message.RequestsRate;

					cacheProvider = message.CacheProvider;
					cacheStatus = cacheStatus == "OK" && message.CacheStatus != "OK" ? message.CacheStatus : cacheStatus;
					cacheTotalQueue = message.CacheTotalQueue > cacheTotalQueue ? message.CacheTotalQueue : cacheTotalQueue;
					cacheInteractiveQueue = message.CacheInteractiveQueue > cacheInteractiveQueue ? message.CacheInteractiveQueue : cacheInteractiveQueue;
					cachePingMilliseconds = message.CachePingMilliseconds > cachePingMilliseconds ? message.CachePingMilliseconds : cachePingMilliseconds;

					cacheL1Hit304 += message.CacheL1Hit304;
					cacheL1Hit200 += message.CacheL1Hit200;
					cacheL1Miss += message.CacheL1Miss;
					cacheL2Hit304 += message.CacheL2Hit304;
					cacheL2Hit200 += message.CacheL2Hit200;
					cacheL2Miss += message.CacheL2Miss;

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
						rpcWeightedLatency += message.RpcAverageLatency * message.RpcCompleted;
						rpcTotalCompleted += message.RpcCompleted;
					}

					if (message.RpcMaxLatency > rpcMaxLatency)
						rpcMaxLatency = message.RpcMaxLatency;
				}

				var serviceName = messagesGroup.Key;
				var numberOfNodes = messagesGroup.Select(msg => msg.NodeID).Distinct(StringComparer.OrdinalIgnoreCase).Count();
				var rpcAverageLatency = rpcTotalCompleted > 0	? rpcWeightedLatency / rpcTotalCompleted : 0;

				statistics.Add(new StatisticMessage
				{
					ServiceName = serviceName,
					Nodes = numberOfNodes,

					ThreadPoolWorkers = threadpoolWorkers,
					ThreadPoolAsyncIO = threadpoolAsyncIO,
					ThreadPoolMaxWorkers = threadpoolMaxWorkers,
					ThreadPoolMaxAsyncIO = threadpoolMaxAsyncIO,

					RequestsTotal = requestsTotal,
					RequestsInFlight = requestsInFlight,
					RequestsRate = requestsRate,

					CacheProvider = cacheProvider,
					CacheStatus = cacheStatus,
					CacheTotalQueue = cacheTotalQueue,
					CacheInteractiveQueue = cacheInteractiveQueue,
					CachePingMilliseconds = cachePingMilliseconds,

					CacheL1Hit304 = cacheL1Hit304,
					CacheL1Hit200 = cacheL1Hit200,
					CacheL1Miss = cacheL1Miss,
					CacheL1HitRatio = requestsTotal > 0 ? (cacheL1Hit304 + cacheL1Hit200) * 100.0 / requestsTotal : 0,
					CacheL2Hit304 = cacheL2Hit304,
					CacheL2Hit200 = cacheL2Hit200,
					CacheL2Miss = cacheL2Miss,
					CacheL2HitRatio = useL1Cache
						? cacheL1Miss > 0 ? (cacheL2Hit304 + cacheL2Hit200) * 100.0 / cacheL1Miss : 0
						: requestsTotal > 0 ? (cacheL2Hit304 + cacheL2Hit200) * 100.0 / requestsTotal : 0,

					RpcGateCurrent = rpcGateCurrent,
					RpcGateAvailable = rpcGateAvailable / numberOfNodes,
					RpcGateMax = rpcGateMax / numberOfNodes,

					RpcEntered = rpcEntered,
					RpcEnteredRate = rpcIn,
					RpcCompleted = rpcCompleted,
					RpcCompletedRate = rpcOut,
					RpcInFlight = rpcInFlight,
					RpcRejected = rpcRejected,

					RpcAverageLatency = rpcAverageLatency,
					RpcMaxLatency = rpcMaxLatency
				});
			}

			return statistics;
		}

		public static JObject Aggregate(this IEnumerable<StatisticMessage> messages, Action<JObject> onCompleted = null)
		{
			var statistics = messages.GroupBy(message => message.ServiceName).Aggregate();
			var numberOfNodes = messages.Select(message => message.NodeID).Distinct().Count();
			var cacheStatuses = statistics.Where(message => message.CacheStatus != "OK");
			var nodeMax = messages.GroupBy(message => message.NodeID).Select(group => new
			{
				NodeID = group.Key,
				Workers = group.Max(message => message.ThreadPoolWorkers)
			}).OrderByDescending(message => message.Workers).First();

			var totalWorkers = statistics.Sum(message => message.ThreadPoolWorkers);
			var totalMaxWorkers = statistics.Max(message => message.ThreadPoolMaxWorkers);
			var totalAsyncIO = statistics.Sum(message => message.ThreadPoolAsyncIO);
			var totalMaxAsyncIO = statistics.Max(message => message.ThreadPoolMaxAsyncIO);
			var workersUsage = totalMaxWorkers > 0 ? totalWorkers * 1.0 / totalMaxWorkers : 0;

			var nodeMaxID = nodeMax.NodeID;
			var nodeMaxWorkers = nodeMax.Workers;
			var nodeMaxUsage = messages.GroupBy(message => message.NodeID).Select(group => group.Max(msg => msg.ThreadPoolMaxWorkers > 0 ? msg.ThreadPoolWorkers * 1.0 / msg.ThreadPoolMaxWorkers	: 0)).Max();

			var cacheProvider = statistics.First().CacheProvider;
			var cacheStatus = (cacheStatuses.FirstOrDefault(message => message.CacheStatus == "🔥CRITICAL") ?? cacheStatuses.FirstOrDefault(message => message.CacheStatus == "⚠️WARN"))?.CacheStatus ?? "OK";
			var cacheTotalQueue = statistics.Sum(message => message.CacheTotalQueue);
			var cacheMaxQueue = messages.Max(message => message.CacheTotalQueue);
			var cacheAverageQueue = numberOfNodes > 0 ? cacheTotalQueue * 1.0 / numberOfNodes : 0;
			var cacheTotalInteractiveQueue = statistics.Sum(message => message.CacheInteractiveQueue);
			var cacheMaxInteractiveQueue = messages.Max(message => message.CacheInteractiveQueue);
			var cacheAvgInteractiveQueue = numberOfNodes > 0 ? cacheTotalInteractiveQueue * 1.0 / numberOfNodes : 0;
			var cacheMaxPing = messages.Max(message => message.CachePingMilliseconds);
			var cacheAveragePing = messages.GroupBy(message => message.NodeID).Select(group => group.Max(msg => msg.CachePingMilliseconds)).Average();

			var totalCurrentRpcGate = statistics.Sum(message => message.RpcGateCurrent);
			var totalAvailableRpcGate = statistics.Sum(message => message.RpcGateAvailable);
			var totalMaxRpcGate = statistics.Sum(message => message.RpcGateMax);
			var rpcGateUsage = totalMaxRpcGate > 0 ? totalCurrentRpcGate * 1.0 / totalMaxRpcGate : 0;

			var totalRpcIn = statistics.Sum(message => message.RpcEnteredRate);
			var totalRpcOut = statistics.Sum(message => message.RpcCompletedRate);
			var totalRpcInFlight = statistics.Sum(message => message.RpcInFlight);
			var totalRpcRejected = statistics.Sum(message => message.RpcRejected);

			var rpcBackpressure = totalRpcIn - totalRpcOut;
			var rpcCompletionRatio = totalRpcIn > 0 ? totalRpcOut / totalRpcIn : 1;
			var rpcRejectRate = totalRpcIn > 0 ? totalRpcRejected * 1.0 / (totalRpcIn * 60) : 0;
			var rpcTotalCompleted = statistics.Sum(message => message.RpcCompleted);
			var rpcWeightedLatency = statistics.Sum(message => message.RpcAverageLatency * message.RpcCompleted);
			var rpcAverageLatency = rpcTotalCompleted > 0 ? rpcWeightedLatency / rpcTotalCompleted : 0;
			var rpcMaxLatency = statistics.Any() ? statistics.Max(message => message.RpcMaxLatency) : 0;

			var statisticsJson = new JObject
			{
				["Time"] = DateTime.Now.AddMinutes(-1),
				["Services"] = statistics.ToJArray(message => message.ToJson(json =>
				{
					json.Remove("Time");
					json.Remove("NodeID");
					json.Remove("UseL1Cache");
					if (message.ServiceName.IsEquals("APIGateway"))
					{
						json.Remove("CacheL1Hit304");
						json.Remove("CacheL1Hit200");
						json.Remove("CacheL1Miss");
						json.Remove("CacheL1HitRatio");
						json.Remove("CacheL2Hit304");
						json.Remove("CacheL2Hit200");
						json.Remove("CacheL2Miss");
						json.Remove("CacheL2HitRatio");
					}
				})),
				["ThreadPool"] = new JObject
				{
					["Workers"] = totalWorkers,
					["MaxWorkers"] = totalMaxWorkers,
					["Usage"] = workersUsage,
					["AsyncIO"] = totalAsyncIO,
					["MaxAsyncIO"] = totalMaxAsyncIO,
					["NodeMax"] = new JObject
					{
						["ID"] = nodeMaxID,
						["Workers"] = nodeMaxWorkers,
						["Usage"] = nodeMaxUsage,
					}
				},
				["Cache"] = new JObject
				{
					["Provider"] = cacheProvider,
					["Status"] = cacheStatus,
					["Ping"] = new JObject
					{
						["Max"] = cacheMaxPing,
						["Average"] = cacheAveragePing
					},
					["Queue"] = new JObject
					{
						["Max"] = cacheMaxQueue,
						["Average"] = cacheAverageQueue,
						["Total"] = cacheTotalQueue
					},
					["Interactive"] = new JObject
					{
						["Max"] = cacheMaxInteractiveQueue,
						["Average"] = cacheAvgInteractiveQueue,
						["Total"] = cacheTotalInteractiveQueue
					}
				},
				["Router"] = new JObject
				{
					["Gate"] = new JObject
					{
						["Usage"] = rpcGateUsage,
						["Current"] = totalCurrentRpcGate,
						["Available"] = totalAvailableRpcGate,
						["Max"] = totalMaxRpcGate
					},
					["Call"] = new JObject
					{
						["Backpressure"] = rpcBackpressure,
						["CompletionRatio"] = rpcCompletionRatio,
						["RejectRate"] = rpcRejectRate,
						["TotalCompleted"] = rpcTotalCompleted,
						["AverageLatency"] = rpcAverageLatency,
						["MaxLatency"] = rpcMaxLatency
					}
				}
			};

			onCompleted?.Invoke(statisticsJson);
			return statisticsJson;
		}
	}
}
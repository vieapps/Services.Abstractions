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
		public double CacheL1MissRatio;
		public long CacheL2Hit304;
		public long CacheL2Hit200;
		public long CacheL2Miss;
		public double CacheL2HitRatio;
		public double CacheL2MissRatio;

		public double RpcGateUsage;
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

		public static JObject Aggregate(this IEnumerable<StatisticMessage> messages, Action<JObject> onCompleted = null)
		{
			var groupbyServiceMessages = messages.GroupBy(message => message.ServiceName);
			var groupbyNodeMessages = messages.GroupBy(message => message.NodeID);
			var numberOfNodes = messages.Select(message => message.NodeID).Distinct().Count();
			var (servicesJson, threadpoolJson, cacheJson, rpcJson) = messages.Aggregate(groupbyServiceMessages, groupbyNodeMessages, numberOfNodes, true);
			var statisticsJson = new JObject
			{
				["Time"] = DateTime.Now.AddMinutes(-1),
				["Services"] = servicesJson,
				["ThreadPool"] = threadpoolJson,
				["Cache"] = cacheJson,
				["Router"] = rpcJson
			};
			onCompleted?.Invoke(statisticsJson);
			return statisticsJson;
		}

		static readonly string[] NoCacheInAPIGateway = new[] { "CacheL1Hit304", "CacheL1Hit200", "CacheL1Miss", "CacheL1HitRatio", "CacheL1MissRatio", "CacheL2Hit304", "CacheL2Hit200", "CacheL2Miss", "CacheL2HitRatio", "CacheL2MissRatio" };

		static (JToken ServicesJson, JToken ThreadPoolJson, JToken CacheJson, JToken RpcJson) Aggregate(this IEnumerable<StatisticMessage> originalMessages, IEnumerable<IGrouping<string, StatisticMessage>> groupbyServiceMessages, IEnumerable<IGrouping<string, StatisticMessage>> groupbyNodeMessages, int numberOfNodes, bool gotDetails)
		{
			var statisticsByNodes = groupbyNodeMessages.Select(group =>
			{
				var maxThreadPoolWorkers = group.Max(message => message.ThreadPoolMaxWorkers);
				var currentThreadPoolWorkers = group.Max(message => message.ThreadPoolWorkers);
				var workersUsage = maxThreadPoolWorkers > 0 ? (double)currentThreadPoolWorkers / maxThreadPoolWorkers : 0;
				var rpcGateMax = group.Max(message => message.RpcGateMax);
				return new
				{
					NodeID = group.Key,
					Workers = currentThreadPoolWorkers,
					MaxWorkers = maxThreadPoolWorkers,
					WorkersUsage = workersUsage,
					RpcGateMax = rpcGateMax
				};
			}).ToList();
			var statistics = groupbyServiceMessages.Aggregate(gotDetails);
			var cacheStatuses = statistics.Where(message => message.CacheStatus != "OK");

			var totalWorkers = statistics.Sum(message => message.ThreadPoolWorkers);
			var totalMaxWorkers = statistics.Max(message => message.ThreadPoolMaxWorkers);
			var totalAsyncIO = statistics.Sum(message => message.ThreadPoolAsyncIO);
			var totalMaxAsyncIO = statistics.Max(message => message.ThreadPoolMaxAsyncIO);
			var totalWorkersUsage = statisticsByNodes.Max(node => node.WorkersUsage);

			var nodeMax = statisticsByNodes.OrderByDescending(node => node.Workers).First();
			var nodeMaxID = nodeMax.NodeID;
			var nodeMaxWorkers = nodeMax.Workers;
			var nodeMaxUsage = nodeMax.WorkersUsage;

			var cacheProvider = statistics.First().CacheProvider;
			var cacheStatus = (cacheStatuses.FirstOrDefault(message => message.CacheStatus == "🔥CRITICAL") ?? cacheStatuses.FirstOrDefault(message => message.CacheStatus == "⚠️WARN"))?.CacheStatus ?? "OK";
			var cacheTotalQueue = statistics.Sum(message => message.CacheTotalQueue);
			var cacheMaxQueue = originalMessages.Max(message => message.CacheTotalQueue);
			var cacheAverageQueue = numberOfNodes > 0 ? cacheTotalQueue * 1.0 / numberOfNodes : 0;
			var cacheTotalInteractiveQueue = statistics.Sum(message => message.CacheInteractiveQueue);
			var cacheMaxInteractiveQueue = originalMessages.Max(message => message.CacheInteractiveQueue);
			var cacheAvgInteractiveQueue = numberOfNodes > 0 ? (double)cacheTotalInteractiveQueue / numberOfNodes : 0;
			var cacheMaxPing = originalMessages.Max(message => message.CachePingMilliseconds);
			var cacheAveragePing = originalMessages.GroupBy(message => message.NodeID).Select(group => group.Max(msg => msg.CachePingMilliseconds)).Average();

			var totalCurrentRpcGate = statistics.Sum(message => message.RpcGateCurrent);
			var totalMaxRpcGate = statisticsByNodes.Sum(statisticsByNode => statisticsByNode.RpcGateMax);
			var totalAvailableRpcGate = totalMaxRpcGate - totalCurrentRpcGate;
			var rpcGateUsage = totalMaxRpcGate > 0 ? (double)totalCurrentRpcGate / totalMaxRpcGate : 0;

			var totalRpcIn = statistics.Sum(message => message.RpcEnteredRate);
			var totalRpcOut = statistics.Sum(message => message.RpcCompletedRate);
			var totalRpcInFlight = statistics.Sum(message => message.RpcInFlight);
			var totalRpcRejected = statistics.Sum(message => message.RpcRejected);

			var rpcBackpressure = totalRpcIn - totalRpcOut;
			var rpcCompletionRatio = totalRpcIn > 0 ? totalRpcOut / totalRpcIn : 1;
			var rpcRejectRate = totalRpcIn > 0 ? totalRpcRejected / (totalRpcIn * 60) : 0;
			var rpcTotalCompleted = statistics.Sum(message => message.RpcCompleted);
			var rpcWeightedLatency = statistics.Sum(message => message.RpcAverageLatency * message.RpcCompleted);
			var rpcAverageLatency = rpcTotalCompleted > 0 ? rpcWeightedLatency / rpcTotalCompleted : 0;
			var rpcMaxLatency = statistics.Any() ? statistics.Max(message => message.RpcMaxLatency) : 0;

			var servicesJson = statistics.ToJArray(statistic => statistic.ToJson(json =>
			{
				json.Remove("Time");
				json.Remove("NodeID");
				json.Remove("UseL1Cache");

				if (gotDetails)
				{
					var serviceMessages = originalMessages.Where(message => message.ServiceName == statistic.ServiceName);
					var groupbyMessages = serviceMessages.GroupBy(message => message.NodeID);
					var numberOfServiceNodes = serviceMessages.Select(message => message.NodeID).Distinct().Count();
					var (nodesJson, _, _, _) = originalMessages.Aggregate(groupbyMessages, groupbyMessages, numberOfServiceNodes, false);
					(nodesJson as JArray).ForEach(nodeJson =>
					{
						nodeJson["NodeID"] = nodeJson["ServiceName"];
						nodeJson.Remove("ServiceName");
						nodeJson.Remove("Nodes");
					});
					json["Nodes"] = nodesJson;
				}

				if (statistic.ServiceName.IsEquals("APIGateway"))
				{
					NoCacheInAPIGateway.ForEach(name => json.Remove(name));
					if (gotDetails)
						json.Get<JArray>("Nodes").ForEach(nodeJson => NoCacheInAPIGateway.ForEach(name => nodeJson.Remove(name)));
				}
			}));

			var threadpoolJson = new JObject
			{
				["Usage"] = totalWorkersUsage,
				["Workers"] = totalWorkers,
				["MaxWorkers"] = totalMaxWorkers,
				["AsyncIO"] = totalAsyncIO,
				["MaxAsyncIO"] = totalMaxAsyncIO,
				["NodeMax"] = new JObject
				{
					["ID"] = nodeMaxID,
					["Workers"] = nodeMaxWorkers,
					["Usage"] = nodeMaxUsage,
				}
			};

			var cacheJson = new JObject
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
			};

			var rpcJson = new JObject
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
			};

			return (servicesJson, threadpoolJson, cacheJson, rpcJson);
		}

		static List<StatisticMessage> Aggregate(this IEnumerable<IGrouping<string, StatisticMessage>> groupbyMessages, bool gotDetails)
		{
			var statistics = new List<StatisticMessage>();

			foreach (var groupMessages in groupbyMessages)
			{
				int threadpoolWorkers = 0, threadpoolAsyncIO = 0, threadpoolMaxWorkers = 0, threadpoolMaxAsyncIO = 0;

				double requestsRate = 0;
				long requestsTotal = 0, requestsInFlight = 0;

				string cacheProvider = "Redis", cacheStatus = "OK";
				long cacheTotalQueue = 0, cacheInteractiveQueue = 0, cachePingMilliseconds = 0;

				long cacheL1Hit304 = 0, cacheL1Hit200 = 0, cacheL1Miss = 0;
				long cacheL2Hit304 = 0, cacheL2Hit200 = 0, cacheL2Miss = 0;

				long rpcGateCurrent = 0, rpcGateMax = 0;

				double rpcIn = 0, rpcOut = 0;
				long rpcRejected = 0, rpcEntered = 0, rpcCompleted = 0, rpcInFlight = 0;

				double rpcWeightedLatency = 0;
				long rpcTotalCompleted = 0, rpcMaxLatency = 0;

				var useL1Cache = false;
				foreach (var message in groupMessages)
				{
					useL1Cache = useL1Cache || message.UseL1Cache;

					threadpoolWorkers = Math.Max(threadpoolWorkers, message.ThreadPoolWorkers);
					threadpoolAsyncIO = Math.Max(threadpoolAsyncIO, message.ThreadPoolAsyncIO);
					threadpoolMaxWorkers = Math.Max(threadpoolMaxWorkers, message.ThreadPoolMaxWorkers);
					threadpoolMaxAsyncIO = Math.Max(threadpoolMaxAsyncIO, message.ThreadPoolMaxAsyncIO);

					requestsTotal = Math.Max(requestsTotal, message.RequestsTotal);
					requestsInFlight = Math.Max(requestsInFlight, message.RequestsInFlight);
					requestsRate += message.RequestsRate;

					cacheProvider = message.CacheProvider;
					cacheStatus = cacheStatus == "OK" && message.CacheStatus != "OK" ? message.CacheStatus : cacheStatus;
					cacheTotalQueue = message.CacheTotalQueue > cacheTotalQueue ? message.CacheTotalQueue : cacheTotalQueue;
					cacheInteractiveQueue = message.CacheInteractiveQueue > cacheInteractiveQueue ? message.CacheInteractiveQueue : cacheInteractiveQueue;
					cachePingMilliseconds = message.CachePingMilliseconds > cachePingMilliseconds ? message.CachePingMilliseconds : cachePingMilliseconds;

					cacheL1Hit304 = Math.Max(cacheL1Hit304, message.CacheL1Hit304);
					cacheL1Hit200 = Math.Max(cacheL1Hit200, message.CacheL1Hit200);
					cacheL1Miss = Math.Max(cacheL1Miss, message.CacheL1Miss);
					cacheL2Hit304 = Math.Max(cacheL2Hit304, message.CacheL2Hit304);
					cacheL2Hit200 = Math.Max(cacheL2Hit200, message.CacheL2Hit200);
					cacheL2Miss = Math.Max(cacheL2Miss, message.CacheL2Miss);

					rpcGateCurrent = Math.Max(rpcGateCurrent, message.RpcGateCurrent);
					rpcGateMax = Math.Max(rpcGateMax, message.RpcGateMax);

					rpcRejected = Math.Max(rpcRejected, message.RpcRejected);
					rpcEntered = Math.Max(rpcEntered, message.RpcEntered);
					rpcCompleted = Math.Max(rpcCompleted, message.RpcCompleted);
					rpcInFlight = Math.Max(rpcInFlight, message.RpcInFlight);

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

				var rpcAverageLatency = rpcTotalCompleted > 0 ? rpcWeightedLatency / rpcTotalCompleted : 0;
				var serviceName = groupMessages.Key;
				var groupbyServiceMessages = groupMessages.Where(msg => msg.ServiceName == serviceName);
				var nodeIDs = groupbyServiceMessages.Select(msg => msg.NodeID).Distinct(StringComparer.OrdinalIgnoreCase);
				var numberOfNodes = nodeIDs.Count();
				if (gotDetails)
				{
					requestsTotal = 0;
					requestsInFlight = 0;
					cacheL1Hit304 = 0;
					cacheL1Hit200 = 0;
					cacheL1Miss = 0;
					cacheL2Hit304 = 0;
					cacheL2Hit200 = 0;
					cacheL2Miss = 0;
					rpcGateCurrent = 0;
					rpcGateMax = 0;
					nodeIDs.ForEach(nodeID =>
					{
						requestsTotal += groupbyServiceMessages.Where(msg => msg.NodeID == nodeID).Max(msg => msg.RequestsTotal);
						requestsInFlight += groupbyServiceMessages.Where(msg => msg.NodeID == nodeID).Max(msg => msg.RequestsInFlight);
						cacheL1Hit304 += groupbyServiceMessages.Where(msg => msg.NodeID == nodeID).Max(msg => msg.CacheL1Hit304);
						cacheL1Hit200 += groupbyServiceMessages.Where(msg => msg.NodeID == nodeID).Max(msg => msg.CacheL1Hit200);
						cacheL1Miss += groupbyServiceMessages.Where(msg => msg.NodeID == nodeID).Max(msg => msg.CacheL1Miss);
						cacheL2Hit304 += groupbyServiceMessages.Where(msg => msg.NodeID == nodeID).Max(msg => msg.CacheL2Hit304);
						cacheL2Hit200 += groupbyServiceMessages.Where(msg => msg.NodeID == nodeID).Max(msg => msg.CacheL2Hit200);
						cacheL2Miss += groupbyServiceMessages.Where(msg => msg.NodeID == nodeID).Max(msg => msg.CacheL2Miss);
						rpcGateCurrent += groupbyServiceMessages.Where(msg => msg.NodeID == nodeID).Max(msg => msg.RpcGateCurrent);
						rpcGateMax += groupbyServiceMessages.Where(msg => msg.NodeID == nodeID).Max(msg => msg.RpcGateMax);
					});
				}

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
					CacheL1MissRatio = requestsTotal > 0 ? cacheL1Miss * 100.0 / requestsTotal : 0,
					CacheL2Hit304 = cacheL2Hit304,
					CacheL2Hit200 = cacheL2Hit200,
					CacheL2Miss = cacheL2Miss,
					CacheL2HitRatio = requestsTotal > 0 ? (cacheL2Hit304 + cacheL2Hit200) * 100.0 / requestsTotal : 0,
					CacheL2MissRatio = requestsTotal > 0 ? cacheL2Miss * 100.0 / requestsTotal : 0,

					RpcGateCurrent = rpcGateCurrent,
					RpcGateAvailable = rpcGateMax - rpcGateCurrent,
					RpcGateMax = rpcGateMax,
					RpcGateUsage = rpcGateMax > 0	? rpcGateCurrent * 100.0 / rpcGateMax	: 0,

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
	}
}
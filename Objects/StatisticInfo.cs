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
		public long Counters { get; set; } = 0;
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

		public double CpuUsage;
		public int MemoryUsage;

		public double CpuMin;
		public double CpuMax;
		public double CpuAverage;
		public int MemoryMin;
		public int MemoryMax;
		public double MemoryAverage;

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
		public double CacheL1HitRatio;
		public long CacheL1Miss;
		public double CacheL1MissRatio;
		public long CacheL1Bypass;
		public double CacheL1BypassRatio;

		public long CacheL2Hit304;
		public long CacheL2Hit200;
		public double CacheL2HitRatio;
		public long CacheL2Miss;
		public double CacheL2MissRatio;
		public long CacheL2Bypass;
		public double CacheL2BypassRatio;

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

		public StatisticMessage() { }
		public StatisticMessage(JToken json, DateTime time)
		{
			this.CopyFrom(json);
			this.Time = time;
		}
	}

	public static class StatisticMessageExtension
	{
		/// <summary>
		/// Gets the statistic messages for aggregating system statistics
		/// </summary>
		/// <param name="messages"></param>
		/// <param name="time"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Aggregates the system statistics
		/// </summary>
		/// <param name="messages"></param>
		/// <param name="onCompleted"></param>
		/// <returns></returns>
		public static JObject Aggregate(this IEnumerable<StatisticMessage> messages, Action<JObject> onCompleted = null)
		{
			var upstreamMessages = messages.Where(message => message.IsHttp);
			var upstreamServiceMessages = upstreamMessages.GroupBy(message => message.ServiceName);
			var upstreamNodeMessages = upstreamMessages.GroupBy(message => message.NodeID);
			var upstreamNumberOfNodes = upstreamMessages.Select(message => message.NodeID).Distinct().Count();
			var (upstreamServices, upstreamEnvironment, upstreamCache, upstreamRpc) = upstreamMessages.Aggregate(upstreamServiceMessages, upstreamNodeMessages, upstreamNumberOfNodes, true);

			var downstreamMessages = messages.Where(message => !message.IsHttp);
			var downstreamServiceMessages = downstreamMessages.GroupBy(message => message.ServiceName);
			var downstreamNodeMessages = downstreamMessages.GroupBy(message => message.NodeID);
			var downstreamNumberOfNodes = downstreamMessages.Select(message => message.NodeID).Distinct().Count();
			var (downstreamServices, downstreamEnvironment, downstreamCache, _) = downstreamMessages.Aggregate(downstreamServiceMessages, downstreamNodeMessages, downstreamNumberOfNodes, true, NotAvailableInDownstream.Concat(NotAvailableInAPIGateway));

			var statisticsJson = new JObject
			{
				["Time"] = DateTime.Now.AddMinutes(-1),
				["Upstream"] = new JObject
				{
					["Environment"] = upstreamEnvironment,
					["Cache"] = upstreamCache,
					["Router"] = upstreamRpc,
					["Services"] = upstreamServices
				},
				["Downstream"] = new JObject
				{
					["Environment"] = downstreamEnvironment,
					["Cache"] = downstreamCache,
					["Services"] = downstreamServices
				}
			};
			onCompleted?.Invoke(statisticsJson);
			return statisticsJson;
		}

		static (JToken ServicesJson, JToken EnvironmentJson, JToken CacheJson, JToken RpcJson) Aggregate(this IEnumerable<StatisticMessage> originalMessages, IEnumerable<IGrouping<string, StatisticMessage>> groupbyServiceMessages, IEnumerable<IGrouping<string, StatisticMessage>> groupbyNodeMessages, int numberOfNodes, bool addDetailOfNodes, IEnumerable<string> beRemoved = null)
		{
			var statisticsByNodes = groupbyNodeMessages.Select(group =>
			{
				var cpuUsage = group.Max(message => message.CpuUsage);
				var memoryUsage = group.Max(message => message.MemoryUsage);
				var maxThreadPoolWorkers = group.Max(message => message.ThreadPoolMaxWorkers);
				var currentThreadPoolWorkers = group.Max(message => message.ThreadPoolWorkers);
				var workersUsage = (maxThreadPoolWorkers > 0 ? (double)currentThreadPoolWorkers / maxThreadPoolWorkers : 0) * 100.0;
				var rpcGateMax = group.Max(message => message.RpcGateMax);
				return new
				{
					NodeID = group.Key,
					CpuUsage = cpuUsage,
					MemoryUsage = memoryUsage,
					Workers = currentThreadPoolWorkers,
					MaxWorkers = maxThreadPoolWorkers,
					WorkersUsage = workersUsage,
					RpcGateMax = rpcGateMax
				};
			}).ToList();
			var statistics = groupbyServiceMessages.Aggregate(!addDetailOfNodes);
			var cacheStatuses = statistics.Where(message => message.CacheStatus != "OK");

			var cpuMin = originalMessages.Min(message => message.CpuUsage);
			var cpuMax = originalMessages.Max(message => message.CpuUsage);
			var cpuAverage = originalMessages.Average(message => message.CpuUsage);

			var memoryMin = originalMessages.Min(message => message.MemoryUsage);
			var memoryMax = originalMessages.Max(message => message.MemoryUsage);
			var memoryAverage = originalMessages.Average(message => message.MemoryUsage);

			var totalWorkers = statistics.Sum(message => message.ThreadPoolWorkers);
			var totalMaxWorkers = statistics.Max(message => message.ThreadPoolMaxWorkers);
			var totalAsyncIO = statistics.Sum(message => message.ThreadPoolAsyncIO);
			var totalMaxAsyncIO = statistics.Max(message => message.ThreadPoolMaxAsyncIO);
			var totalWorkersUsage = (totalMaxWorkers > 0 ? (double)totalWorkers / totalMaxWorkers : 0) * 100.0;

			var nodeMax = statisticsByNodes.OrderByDescending(statisticsByNode => statisticsByNode.Workers).First();
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
			var cacheAverageInteractiveQueue = numberOfNodes > 0 ? (double)cacheTotalInteractiveQueue / numberOfNodes : 0;
			var cacheMaxPing = originalMessages.Max(message => message.CachePingMilliseconds);
			var cacheAveragePing = originalMessages.GroupBy(message => message.NodeID).Select(group => group.Max(msg => msg.CachePingMilliseconds)).Average();

			var totalCurrentRpcGate = statistics.Sum(message => message.RpcGateCurrent);
			var totalMaxRpcGate = statisticsByNodes.Sum(statisticsByNode => statisticsByNode.RpcGateMax);
			var totalAvailableRpcGate = totalMaxRpcGate - totalCurrentRpcGate;
			var totalRpcGateUsage = (totalMaxRpcGate > 0 ? (double)totalCurrentRpcGate / totalMaxRpcGate : 0) * 100.0;

			var totalRpcIn = statistics.Sum(message => message.RpcEnteredRate);
			var totalRpcOut = statistics.Sum(message => message.RpcCompletedRate);
			var totalRpcInFlight = statistics.Sum(message => message.RpcInFlight);
			var totalRpcRejected = statistics.Sum(message => message.RpcRejected);
			var totalRpcEntered = statistics.Sum(message => message.RpcEntered);
			var totalRpcCompleted = statistics.Sum(message => message.RpcCompleted);

			var rpcBackpressure = totalRpcIn - totalRpcOut;
			var rpcCompletionRatio = totalRpcIn > 0 ? totalRpcOut / totalRpcIn : 1;
			var rpcRejectRate = totalRpcIn > 0 ? totalRpcRejected / (totalRpcIn * 60) : 0;
			var rpcWeightedLatency = statistics.Sum(message => message.RpcAverageLatency * message.RpcCompleted);
			var rpcAverageLatency = totalRpcCompleted > 0 ? rpcWeightedLatency / totalRpcCompleted : 0;
			var rpcMaxLatency = statistics.Any() ? statistics.Max(message => message.RpcMaxLatency) : 0;

			var servicesJson = statistics.ToJArray(statistic => statistic.ToJson(json =>
			{
				json.Remove("Time");
				json.Remove("NodeID");
				json.Remove("UseL1Cache");
				json.Remove("IsHttp");
				json.Remove("CpuUsage");
				json.Remove("MemoryUsage");
				json.Remove("Nodes");

				if (addDetailOfNodes)
				{
					var serviceMessages = originalMessages.Where(message => message.ServiceName == statistic.ServiceName);
					var groupbyMessages = serviceMessages.GroupBy(message => message.NodeID);
					var numberOfServiceNodes = serviceMessages.Select(message => message.NodeID).Distinct().Count();
					var (nodesJson, _, _, _) = serviceMessages.Aggregate(groupbyMessages, groupbyMessages, numberOfServiceNodes, false, beRemoved);
					(nodesJson as JArray).ForEach(nodeJson =>
					{
						nodeJson["NodeID"] = nodeJson["ServiceName"];
						nodeJson.Remove("ServiceName");
					});
					json["Nodes"] = nodesJson;
				}

				if (statistic.ServiceName.IsEquals("APIGateway"))
				{
					NotAvailableInAPIGateway.ForEach(name => json.Remove(name));
					if (addDetailOfNodes)
						json.Get<JArray>("Nodes").ForEach(nodeJson => NotAvailableInAPIGateway.ForEach(name => nodeJson.Remove(name)));
				}

				beRemoved?.ForEach(name => json.Remove(name));
			}));

			var environmentJson = new JObject
			{
				["CPU"] = new JObject
				{
					["Min"] = cpuMin,
					["Max"] = cpuMax,
					["Average"] = cpuAverage,
					["Total"] = statisticsByNodes.Sum(statisticsByNode => statisticsByNode.CpuUsage)
				},
				["Memory"] = new JObject
				{
					["Min"] = memoryMin,
					["Max"] = memoryMax,
					["Average"] = memoryAverage,
					["Total"] = statisticsByNodes.Sum(statisticsByNode => statisticsByNode.MemoryUsage)
				},
				["ThreadPool"] = new JObject
				{
					["Usage"] = totalWorkersUsage,
					["Workers"] = totalWorkers,
					["MaxWorkers"] = totalMaxWorkers,
					["AsyncIO"] = totalAsyncIO,
					["MaxAsyncIO"] = totalMaxAsyncIO,
				},
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
					["Average"] = cacheAverageInteractiveQueue,
					["Total"] = cacheTotalInteractiveQueue
				}
			};

			var rpcJson = new JObject
			{
				["Gate"] = new JObject
				{
					["Usage"] = totalRpcGateUsage,
					["Current"] = totalCurrentRpcGate,
					["Available"] = totalAvailableRpcGate,
					["Max"] = totalMaxRpcGate
				},
				["Call"] = new JObject
				{
					["Backpressure"] = rpcBackpressure,
					["CompletionRatio"] = rpcCompletionRatio,
					["RejectRate"] = rpcRejectRate,
					["TotalRejected"] = totalRpcRejected,
					["TotalInFlight"] = totalRpcInFlight,
					["TotalCompleted"] = totalRpcCompleted,
					["TotalEntered"] = totalRpcEntered,
					["AverageLatency"] = rpcAverageLatency,
					["MaxLatency"] = rpcMaxLatency
				}
			};

			return (servicesJson, environmentJson, cacheJson, rpcJson);
		}

		static List<StatisticMessage> Aggregate(this IEnumerable<IGrouping<string, StatisticMessage>> groupbyMessages, bool isNodeScope)
		{
			var statistics = new List<StatisticMessage>();

			foreach (var groupMessages in groupbyMessages)
			{
				int threadpoolWorkers = 0, threadpoolAsyncIO = 0, threadpoolMaxWorkers = 0, threadpoolMaxAsyncIO = 0;

				double requestsRate = 0;
				long requestsTotal = 0, requestsInFlight = 0, requestsHttpTotal = 0;

				string cacheProvider = "Redis", cacheStatus = "OK";
				long cacheTotalQueue = 0, cacheInteractiveQueue = 0, cachePingMilliseconds = 0;

				long cacheL1Hit304 = 0, cacheL1Hit200 = 0, cacheL1Miss = 0, cacheL1Bypass = 0;
				long cacheL2Hit304 = 0, cacheL2Hit200 = 0, cacheL2Miss = 0, cacheL2Bypass = 0;

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
					requestsHttpTotal = Math.Max(requestsHttpTotal, message.UseL1Cache ? message.CacheL1Hit304 + message.CacheL1Hit200 + message.CacheL1Miss + message.CacheL1Bypass : message.CacheL2Hit304 + message.CacheL2Hit200 + message.CacheL2Miss + message.CacheL2Bypass);

					cacheProvider = message.CacheProvider;
					cacheStatus = cacheStatus == "OK" && message.CacheStatus != "OK" ? message.CacheStatus : cacheStatus;
					cacheTotalQueue = message.CacheTotalQueue > cacheTotalQueue ? message.CacheTotalQueue : cacheTotalQueue;
					cacheInteractiveQueue = message.CacheInteractiveQueue > cacheInteractiveQueue ? message.CacheInteractiveQueue : cacheInteractiveQueue;
					cachePingMilliseconds = message.CachePingMilliseconds > cachePingMilliseconds ? message.CachePingMilliseconds : cachePingMilliseconds;

					cacheL1Hit304 = Math.Max(cacheL1Hit304, message.CacheL1Hit304);
					cacheL1Hit200 = Math.Max(cacheL1Hit200, message.CacheL1Hit200);
					cacheL1Miss = Math.Max(cacheL1Miss, message.CacheL1Miss);
					cacheL1Bypass = Math.Max(cacheL1Bypass, message.CacheL1Bypass);

					cacheL2Hit304 = Math.Max(cacheL2Hit304, message.CacheL2Hit304);
					cacheL2Hit200 = Math.Max(cacheL2Hit200, message.CacheL2Hit200);
					cacheL2Miss = Math.Max(cacheL2Miss, message.CacheL2Miss);
					cacheL2Bypass = Math.Max(cacheL2Bypass, message.CacheL2Bypass);

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
				var sampleCount = groupMessages.Count();

				if (isNodeScope)
				{
					requestsRate = sampleCount > 0 ? requestsRate / sampleCount : requestsRate;
					rpcIn = sampleCount > 0 ? rpcIn / sampleCount : rpcIn;
					rpcOut = sampleCount > 0 ? rpcOut / sampleCount : rpcOut;
				}
				else
				{
					var nodeMessages = groupMessages.GroupBy(message => message.NodeID);
					threadpoolWorkers = nodeMessages.Sum(messages => messages.Max(message => message.ThreadPoolWorkers));
					threadpoolAsyncIO = nodeMessages.Sum(messages => messages.Max(message => message.ThreadPoolAsyncIO));

					requestsTotal = nodeMessages.Sum(messages => messages.Max(message => message.RequestsTotal));
					requestsInFlight = nodeMessages.Sum(messages => messages.Max(message => message.RequestsInFlight));
					requestsRate = nodeMessages.Sum(messages => messages.Average(message => message.RequestsRate));
					requestsHttpTotal = nodeMessages.Sum(messages => messages.Max(message => message.UseL1Cache ? message.CacheL1Hit304 + message.CacheL1Hit200 + message.CacheL1Miss + message.CacheL1Bypass : message.CacheL2Hit304 + message.CacheL2Hit200 + message.CacheL2Miss + message.CacheL2Bypass));

					cacheL1Hit304 = nodeMessages.Sum(messages => messages.Max(message => message.CacheL1Hit304));
					cacheL1Hit200 = nodeMessages.Sum(messages => messages.Max(message => message.CacheL1Hit200));
					cacheL1Miss = nodeMessages.Sum(messages => messages.Max(message => message.CacheL1Miss));
					cacheL1Bypass = nodeMessages.Sum(messages => messages.Max(message => message.CacheL1Bypass));

					cacheL2Hit304 = nodeMessages.Sum(messages => messages.Max(message => message.CacheL2Hit304));
					cacheL2Hit200 = nodeMessages.Sum(messages => messages.Max(message => message.CacheL2Hit200));
					cacheL2Miss = nodeMessages.Sum(messages => messages.Max(message => message.CacheL2Miss));
					cacheL2Bypass = nodeMessages.Sum(messages => messages.Max(message => message.CacheL2Bypass));

					rpcGateCurrent = nodeMessages.Sum(messages => messages.Max(message => message.RpcGateCurrent));
					rpcGateMax = nodeMessages.Sum(messages => messages.Max(message => message.RpcGateMax));
					rpcRejected = nodeMessages.Sum(messages => messages.Max(message => message.RpcRejected));
					rpcEntered = nodeMessages.Sum(messages => messages.Max(message => message.RpcEntered));
					rpcCompleted = nodeMessages.Sum(messages => messages.Max(message => message.RpcCompleted));
					rpcInFlight = nodeMessages.Sum(messages => messages.Max(message => message.RpcInFlight));
					rpcIn = nodeMessages.Sum(messages => messages.Average(message => message.RpcEnteredRate));
					rpcOut = nodeMessages.Sum(messages => messages.Average(message => message.RpcCompletedRate));
				}

				statistics.Add(new StatisticMessage
				{
					ServiceName = serviceName,

					CpuMin = groupMessages.Min(message => message.CpuUsage),
					CpuMax = groupMessages.Max(message => message.CpuUsage),
					CpuAverage = groupMessages.Average(message => message.CpuUsage),
					MemoryMin = groupMessages.Min(message => message.MemoryUsage),
					MemoryMax = groupMessages.Max(message => message.MemoryUsage),
					MemoryAverage = groupMessages.Average(message => message.MemoryUsage),
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
					CacheL1HitRatio = requestsHttpTotal > 0 ? (cacheL1Hit304 + cacheL1Hit200) * 100.0 / requestsHttpTotal : 0,
					CacheL1Miss = cacheL1Miss,
					CacheL1MissRatio = requestsHttpTotal > 0 ? cacheL1Miss * 100.0 / requestsHttpTotal : 0,
					CacheL1Bypass = cacheL1Bypass,
					CacheL1BypassRatio = requestsHttpTotal > 0 ? cacheL1Bypass * 100.0 / requestsHttpTotal : 0,

					CacheL2Hit304 = cacheL2Hit304,
					CacheL2Hit200 = cacheL2Hit200,
					CacheL2HitRatio = requestsHttpTotal > 0 ? (cacheL2Hit304 + cacheL2Hit200) * 100.0 / requestsHttpTotal : 0,
					CacheL2Miss = cacheL2Miss,
					CacheL2MissRatio = requestsHttpTotal > 0 ? cacheL2Miss * 100.0 / requestsHttpTotal : 0,
					CacheL2Bypass = cacheL2Bypass,
					CacheL2BypassRatio = requestsHttpTotal > 0 ? cacheL2Bypass * 100.0 / requestsHttpTotal : 0,

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

		static readonly string[] NotAvailableInAPIGateway = new[] { "CacheL1Hit304", "CacheL1Hit200", "CacheL1Miss", "CacheL1Bypass", "CacheL1HitRatio", "CacheL1MissRatio", "CacheL1BypassRatio", "CacheL2Hit304", "CacheL2Hit200", "CacheL2Miss", "CacheL2Bypass", "CacheL2HitRatio", "CacheL2MissRatio", "CacheL2BypassRatio" };
		static readonly string[] NotAvailableInDownstream = new[] { "RequestsTotal", "RequestsInFlight", "RequestsRate", "RpcGateUsage", "RpcGateCurrent", "RpcGateAvailable", "RpcGateMax", "RpcEntered", "RpcEnteredRate", "RpcCompleted", "RpcCompletedRate", "RpcInFlight", "RpcRejected", "RpcAverageLatency", "RpcMaxLatency" };
	}
}
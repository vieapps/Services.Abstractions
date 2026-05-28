using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;
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
	/// Presents a services' statistics
	/// </summary>
	public sealed class ServiceStatistics
	{
		/// <summary>
		/// Gets or Sets the window-size (milliseconds)
		/// </summary>
		public long WindowSize { get; set; } = 5000;

		long _requestsTotal;
		long _requestsHttpTotal;

		int _requestsInFlight;
		int _requestsHttpInFlight;

		long _cacheL1Hit304;
		long _cacheL1Hit200;
		long _cacheL1Miss;
		long _cacheL1Bypass;
		long _cacheL2Hit304;
		long _cacheL2Hit200;
		long _cacheL2Miss;
		long _cacheL2Bypass;

		int _rpcInFlight;

		long _rpcRejected;
		long _rpcRejectedTotal;

		long _rpcEntered;
		long _rpcEnteredTotal;

		long _rpcCompleted;
		long _rpcCompletedTotal;

		long _rpcLatency;
		long _rpcLatencyTotal;

		long _rpcMaxLatency;
		long _rpcMaxLatencyTotal;

		long _lastRequestsTotal;
		long _lastRequestsHttpTotal;

		long _lastRpcRejected;
		long _lastRpcRejectedTotal;

		long _lastRpcEntered;
		long _lastRpcEnteredTotal;

		long _lastRpcCompleted;
		long _lastRpcCompletedTotal;

		public void IncreaseRequest(bool updateHttp = true)
		{
			Interlocked.Increment(ref this._requestsTotal);
			Interlocked.Increment(ref this._requestsInFlight);

			if (updateHttp)
			{
				Interlocked.Increment(ref this._requestsHttpTotal);
				Interlocked.Increment(ref this._requestsHttpInFlight);
			}
		}

		public void DecreaseRequest(bool updateHttp = true)
		{
			if (Interlocked.Decrement(ref this._requestsInFlight) < 0)
				Interlocked.Exchange(ref this._requestsInFlight, 0);

			if (updateHttp)
			{
				if (Interlocked.Decrement(ref this._requestsHttpInFlight) < 0)
					Interlocked.Exchange(ref this._requestsHttpInFlight, 0);
			}
		}

		public long RequestsTotal => Volatile.Read(ref this._requestsTotal);

		public long RequestsHttpTotal => Volatile.Read(ref this._requestsHttpTotal);

		public int RequestsInFlight => Volatile.Read(ref this._requestsInFlight);

		public int RequestsHttpInFlight => Volatile.Read(ref this._requestsHttpInFlight);

		public double GetRequestsRate(double elapsedSeconds, bool useHttp = false)
			=> useHttp ? this.GetRequestsHttpRate(elapsedSeconds) : this.GetRate(ref this._lastRequestsTotal, this.RequestsTotal, elapsedSeconds);

		public double GetRequestsHttpRate(double elapsedSeconds)
			=> this.GetRate(ref this._lastRequestsHttpTotal, this.RequestsHttpTotal, elapsedSeconds);

		public long L1Hit304()
			=> Interlocked.Increment(ref this._cacheL1Hit304);

		public long L1Hit200()
			=> Interlocked.Increment(ref this._cacheL1Hit200);

		public long L1Hit(bool is304)
			=> is304 ? this.L1Hit304() : this.L1Hit200();

		public long CacheL1Hit304Count => Volatile.Read(ref this._cacheL1Hit304);

		public long CacheL1Hit200Count => Volatile.Read(ref this._cacheL1Hit200);

		public long CacheL1HitCount => this.CacheL1Hit304Count + this.CacheL1Hit200Count;

		public long L1Miss()
			=> Interlocked.Increment(ref this._cacheL1Miss);

		public long CacheL1MissCount => Volatile.Read(ref this._cacheL1Miss);

		public long L1Bypass()
			=> Interlocked.Increment(ref this._cacheL1Bypass);

		public long CacheL1BypassCount => Volatile.Read(ref this._cacheL1Bypass);

		public long L2Hit304()
			=> Interlocked.Increment(ref this._cacheL2Hit304);

		public long L2Hit200()
			=> Interlocked.Increment(ref this._cacheL2Hit200);

		public long L2Hit(bool is304)
			=> is304 ? this.L2Hit304() : this.L2Hit200();

		public long CacheL2Hit304Count => Volatile.Read(ref this._cacheL2Hit304);

		public long CacheL2Hit200Count => Volatile.Read(ref this._cacheL2Hit200);

		public long CacheL2HitCount => this.CacheL2Hit304Count + this.CacheL2Hit200Count;

		public long L2Miss()
			=> Interlocked.Increment(ref this._cacheL2Miss);

		public long CacheL2MissCount => Volatile.Read(ref this._cacheL2Miss);

		public long L2Bypass()
			=> Interlocked.Increment(ref this._cacheL2Bypass);

		public long CacheL2BypassCount => Volatile.Read(ref this._cacheL2Bypass);

		public double GetCacheL1HitRatio(bool useHttp = true)
			=> this.GetRatio(this.CacheL1HitCount, useHttp);

		public double GetCacheL1MissRatio(bool useHttp = true)
			=> this.GetRatio(this.CacheL1MissCount, useHttp);

		public double GetCacheL1BypassRatio(bool useHttp = true)
			=> this.GetRatio(this.CacheL1BypassCount, useHttp);

		public double GetCacheL2HitRatio(bool useHttp = true)
			=> this.GetRatio(this.CacheL2HitCount, useHttp);

		public double GetCacheL2MissRatio(bool useHttp = true)
			=> this.GetRatio(this.CacheL2MissCount, useHttp);

		public double GetCacheL2BypassRatio(bool useHttp = true)
			=> this.GetRatio(this.CacheL2BypassCount, useHttp);

		public long RpcRejectedCount => Volatile.Read(ref this._rpcRejected);

		public long RpcRejectedTotalCount => Volatile.Read(ref this._rpcRejectedTotal);

		public void RpcRejected()
		{
			Interlocked.Increment(ref this._rpcRejected);
			Interlocked.Increment(ref this._rpcRejectedTotal);
			this.TryRotate();
		}

		public int RpcInFlightCount => Volatile.Read(ref this._rpcInFlight);

		public long RpcEnteredCount => Volatile.Read(ref this._rpcEntered);

		public long RpcEnteredTotalCount => Volatile.Read(ref this._rpcEnteredTotal);

		public void RpcEntered()
		{
			Interlocked.Increment(ref this._rpcInFlight);
			Interlocked.Increment(ref this._rpcEntered);
			Interlocked.Increment(ref this._rpcEnteredTotal);
			this.TryRotate();
		}

		public long RpcCompletedCount => Volatile.Read(ref this._rpcCompleted);

		public long RpcCompletedTotalCount => Volatile.Read(ref this._rpcCompletedTotal);

		public void RpcCompleted(long elapsedMilliseconds)
		{
			if (Interlocked.Decrement(ref this._rpcInFlight) < 0)
				Interlocked.Exchange(ref this._rpcInFlight, 0);

			Interlocked.Increment(ref this._rpcCompleted);
			Interlocked.Increment(ref this._rpcCompletedTotal);

			long maxLatency;
			Interlocked.Add(ref this._rpcLatency, elapsedMilliseconds);
			do
			{
				maxLatency = Volatile.Read(ref this._rpcMaxLatency);
				if (elapsedMilliseconds <= maxLatency)
					break;
			}
			while (Interlocked.CompareExchange(ref this._rpcMaxLatency, elapsedMilliseconds, maxLatency) != maxLatency);

			Interlocked.Add(ref this._rpcLatencyTotal, elapsedMilliseconds);
			do
			{
				maxLatency = Volatile.Read(ref this._rpcMaxLatencyTotal);
				if (elapsedMilliseconds <= maxLatency)
					break;
			}
			while (Interlocked.CompareExchange(ref this._rpcMaxLatencyTotal, elapsedMilliseconds, maxLatency) != maxLatency);

			this.TryRotate();
		}

		public void RpcCompleted(Stopwatch stopwatch)
			=> this.RpcCompleted(stopwatch.ElapsedMilliseconds);

		public double RpcAverageLatency
		{
			get
			{
				var count = Volatile.Read(ref this._rpcCompleted);
				return count <= 0 ? 0 : (double)Volatile.Read(ref this._rpcLatency) / count;
			}
		}

		public double RpcAverageLatencyTotal
		{
			get
			{
				var count = Volatile.Read(ref this._rpcCompletedTotal);
				return count <= 0 ? 0 : (double)Volatile.Read(ref this._rpcLatencyTotal) / count;
			}
		}

		public long RpcMaxLatency => Volatile.Read(ref this._rpcMaxLatency);

		public long RpcMaxLatencyTotal => Volatile.Read(ref this._rpcMaxLatencyTotal);

		public double GetRpcRejectedRate(double elapsedSeconds)
			=> this.GetRate(ref this._lastRpcRejected, this.RpcRejectedCount, elapsedSeconds);

		public double GetRpcRejectedTotalRate(double elapsedSeconds)
			=> this.GetRate(ref this._lastRpcRejectedTotal, this.RpcRejectedTotalCount, elapsedSeconds);

		public double GetRpcEnteredRate(double elapsedSeconds)
			=> this.GetRate(ref this._lastRpcEntered, this.RpcEnteredCount, elapsedSeconds);

		public double GetRpcEnteredTotalRate(double elapsedSeconds)
			=> this.GetRate(ref this._lastRpcEnteredTotal, this.RpcEnteredTotalCount, elapsedSeconds);

		public double GetRpcCompletedRate(double elapsedSeconds)
			=> this.GetRate(ref this._lastRpcCompleted, this.RpcCompletedCount, elapsedSeconds);

		public double GetRpcCompletedTotalRate(double elapsedSeconds)
			=> this.GetRate(ref this._lastRpcCompletedTotal, this.RpcCompletedTotalCount, elapsedSeconds);

		public void Reset()
		{
			Interlocked.Exchange(ref this._requestsTotal, 0);
			Interlocked.Exchange(ref this._requestsHttpTotal, 0);

			Interlocked.Exchange(ref this._requestsInFlight, 0);
			Interlocked.Exchange(ref this._requestsHttpInFlight, 0);

			Interlocked.Exchange(ref this._cacheL1Hit304, 0);
			Interlocked.Exchange(ref this._cacheL1Hit200, 0);
			Interlocked.Exchange(ref this._cacheL1Miss, 0);
			Interlocked.Exchange(ref this._cacheL1Bypass, 0);
			Interlocked.Exchange(ref this._cacheL2Hit304, 0);
			Interlocked.Exchange(ref this._cacheL2Hit200, 0);
			Interlocked.Exchange(ref this._cacheL2Miss, 0);
			Interlocked.Exchange(ref this._cacheL2Bypass, 0);

			Interlocked.Exchange(ref this._rpcInFlight, 0);

			Interlocked.Exchange(ref this._rpcRejected, 0);
			Interlocked.Exchange(ref this._rpcRejectedTotal, 0);

			Interlocked.Exchange(ref this._rpcEntered, 0);
			Interlocked.Exchange(ref this._rpcEnteredTotal, 0);

			Interlocked.Exchange(ref this._rpcCompleted, 0);
			Interlocked.Exchange(ref this._rpcCompletedTotal, 0);

			Interlocked.Exchange(ref this._rpcLatency, 0);
			Interlocked.Exchange(ref this._rpcMaxLatency, 0);

			Interlocked.Exchange(ref this._rpcLatencyTotal, 0);
			Interlocked.Exchange(ref this._rpcMaxLatencyTotal, 0);

			Interlocked.Exchange(ref this._lastRequestsTotal, 0);
			Interlocked.Exchange(ref this._lastRequestsHttpTotal, 0);

			Interlocked.Exchange(ref this._lastRpcRejected, 0);
			Interlocked.Exchange(ref this._lastRpcRejectedTotal, 0);

			Interlocked.Exchange(ref this._lastRpcEntered, 0);
			Interlocked.Exchange(ref this._lastRpcEnteredTotal, 0);

			Interlocked.Exchange(ref this._lastRpcCompleted, 0);
			Interlocked.Exchange(ref this._lastRpcCompletedTotal, 0);
		}

#if NETSTANDARD2_0
		long _lastRotate = (long)Environment.TickCount;
#else
		long _lastRotate = Environment.TickCount64;
#endif

		void TryRotate()
		{
#if NETSTANDARD2_0
			var now = (long)Environment.TickCount;
#else
			var now = Environment.TickCount64;
#endif
			var last = Volatile.Read(ref this._lastRotate);
			if (now - last < this.WindowSize)
				return;

			if (Interlocked.CompareExchange(ref this._lastRotate, now, last) != last)
				return;

			Interlocked.Exchange(ref this._rpcRejected, 0);
			Interlocked.Exchange(ref this._lastRpcRejected, 0);

			Interlocked.Exchange(ref this._rpcEntered, 0);
			Interlocked.Exchange(ref this._lastRpcEntered, 0);

			Interlocked.Exchange(ref this._rpcCompleted, 0);
			Interlocked.Exchange(ref this._lastRpcCompleted, 0);

			Interlocked.Exchange(ref this._rpcLatency, 0);
			Interlocked.Exchange(ref this._rpcMaxLatency, 0);
		}

		double GetRate(ref long last, long current, double elapsed)
		{
			if (elapsed <= 0)
				return 0;

			var delta = current - Interlocked.Exchange(ref last, current);
			return delta > 0 ? delta / elapsed : 0;
		}

		double GetRatio(long count, bool useHttp = false)
		{
			var total = useHttp ? this.RequestsHttpTotal : this.RequestsTotal;
			return total > 0 ? count * 100.0 / total : 0;
		}
	}

	/// <summary>
	/// Presents a message of services' statistics
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

		public long RpcInFlight;

		public long RpcRejected;
		public double RpcRejectedRate;
		public long RpcRejectedTotal;
		public double RpcRejectedTotalRate;

		public long RpcEntered;
		public double RpcEnteredRate;
		public long RpcEnteredTotal;
		public double RpcEnteredTotalRate;

		public long RpcCompleted;
		public double RpcCompletedRate;
		public long RpcCompletedTotal;
		public double RpcCompletedTotalRate;

		public double RpcAverageLatency;
		public double RpcAverageLatencyTotal;
		public long RpcMaxLatency;
		public long RpcMaxLatencyTotal;

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
			var (upstreamServices, upstreamEnvironment, upstreamCache, router) = upstreamMessages.Aggregate(upstreamServiceMessages, upstreamNodeMessages, upstreamNumberOfNodes, true);

			var downstreamMessages = messages.Where(message => !message.IsHttp);
			var downstreamServiceMessages = downstreamMessages.GroupBy(message => message.ServiceName);
			var downstreamNodeMessages = downstreamMessages.GroupBy(message => message.NodeID);
			var downstreamNumberOfNodes = downstreamMessages.Select(message => message.NodeID).Distinct().Count();
			var (downstreamServices, downstreamEnvironment, downstreamCache, _) = downstreamMessages.Aggregate(downstreamServiceMessages, downstreamNodeMessages, downstreamNumberOfNodes, true, NotAvailableInDownstream.Concat(NotAvailableInAPIGateway));

			var statisticsJson = new JObject
			{
				["Time"] = DateTime.Now.AddMinutes(-1),
				["Router"] = router,
				["Upstream"] = new JObject
				{
					["Environment"] = upstreamEnvironment,
					["Cache"] = upstreamCache,
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
				var workersUsage = maxThreadPoolWorkers > 0 ? (double)currentThreadPoolWorkers / maxThreadPoolWorkers : 0;
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

			var threadpoolWorkers = statistics.Sum(message => message.ThreadPoolWorkers);
			var threadpoolMaxWorkers = statistics.Max(message => message.ThreadPoolMaxWorkers);
			var threadpoolAsyncIO = statistics.Sum(message => message.ThreadPoolAsyncIO);
			var threadpoolMaxAsyncIO = statistics.Max(message => message.ThreadPoolMaxAsyncIO);
			var threadpoolWorkersUsage = threadpoolMaxWorkers > 0 ? (double)threadpoolWorkers / threadpoolMaxWorkers : 0;

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

			var rpcCurrent = statistics.Sum(message => message.RpcGateCurrent);
			var rpcMax = statisticsByNodes.Sum(statisticsByNode => statisticsByNode.RpcGateMax);
			var rpcAvailable = rpcMax - rpcCurrent;
			var rpcUsage = rpcMax > 0 ? (double)rpcCurrent / rpcMax : 0;
			var rpcInFlight = statistics.Sum(message => message.RpcInFlight);

			var rpcRejected = statistics.Sum(message => message.RpcRejected);
			var rpcRejectedRate = statistics.Sum(message => message.RpcRejectedRate);
			var rpcRejectedTotal = statistics.Sum(message => message.RpcRejectedTotal);
			var rpcRejectedTotalRate = statistics.Sum(message => message.RpcRejectedTotalRate);

			var rpcEntered = statistics.Sum(message => message.RpcEntered);
			var rpcEnteredRate = statistics.Sum(message => message.RpcEnteredRate);
			var rpcEnteredTotal = statistics.Sum(message => message.RpcEnteredTotal);
			var rpcEnteredTotalRate = statistics.Sum(message => message.RpcEnteredTotalRate);

			var rpcCompleted = statistics.Sum(message => message.RpcCompleted);
			var rpcCompletedRate = statistics.Sum(message => message.RpcCompletedRate);
			var rpcCompletedTotal = statistics.Sum(message => message.RpcCompletedTotal);
			var rpcCompletedTotalRate = statistics.Sum(message => message.RpcCompletedTotalRate);

			var rpcAverageLatency = statistics.Any() ? statistics.Max(message => message.RpcAverageLatency) : 0;
			var rpcMaxLatency = statistics.Any() ? statistics.Max(message => message.RpcMaxLatency) : 0;
			var rpcAverageLatencyTotal = statistics.Any() ? statistics.Max(message => message.RpcAverageLatencyTotal) : 0;
			var rpcMaxLatencyTotal = statistics.Any() ? statistics.Max(message => message.RpcMaxLatencyTotal) : 0;

			var rpcBackpressure = rpcEntered - rpcCompleted;
			var rpcBackpressureRate = rpcEnteredRate - rpcCompletedRate;
			var rpcCompletionRatio = (rpcEnteredRate > 0 ? rpcCompletedRate / rpcEnteredRate : 1) * 100.0;
			var rpcCompletionRatioTotal = (rpcEnteredTotal > 0 ? (double)rpcCompletedTotal / rpcEnteredTotal : 1) * 100.0;

			var servicesJson = statistics.ToJArray(statistic => statistic.ToJson(json =>
			{
				json.Remove("Time");
				json.Remove("NodeID");
				json.Remove("UseL1Cache");
				json.Remove("IsHttp");
				json.Remove("CpuUsage");
				json.Remove("MemoryUsage");
				json.Remove("Nodes");

				json["CpuMin"] = Math.Round(statistic.CpuMin, 2);
				json["CpuMax"] = Math.Round(statistic.CpuMax, 2);
				json["CpuAverage"] = Math.Round(statistic.CpuAverage, 2);
				json["MemoryAverage"] = Math.Round(statistic.MemoryAverage, 2);
				json["RequestsRate"] = Math.Round(statistic.RequestsRate, 2);
				json["CacheL1HitRatio"] = Math.Round(statistic.CacheL1HitRatio, 2);
				json["CacheL1MissRatio"] = Math.Round(statistic.CacheL1MissRatio, 2);
				json["CacheL1BypassRatio"] = Math.Round(statistic.CacheL1BypassRatio, 2);
				json["CacheL2HitRatio"] = Math.Round(statistic.CacheL2HitRatio, 2);
				json["CacheL2MissRatio"] = Math.Round(statistic.CacheL2MissRatio, 2);
				json["CacheL2BypassRatio"] = Math.Round(statistic.CacheL2BypassRatio, 2);
				json["RpcGateUsage"] = Math.Round(statistic.RpcGateUsage, 4);
				json["RpcRejectedRate"] = Math.Round(statistic.RpcRejectedRate, 2);
				json["RpcRejectedTotalRate"] = Math.Round(statistic.RpcRejectedTotalRate, 2);
				json["RpcEnteredRate"] = Math.Round(statistic.RpcEnteredRate, 2);
				json["RpcEnteredTotalRate"] = Math.Round(statistic.RpcEnteredTotalRate, 2);
				json["RpcCompletedRate"] = Math.Round(statistic.RpcCompletedRate, 2);
				json["RpcCompletedTotalRate"] = Math.Round(statistic.RpcCompletedTotalRate, 2);
				json["RpcAverageLatency"] = Math.Round(statistic.RpcAverageLatency, 2);
				json["RpcAverageLatencyTotal"] = Math.Round(statistic.RpcAverageLatencyTotal, 2);

				var backpressure = statistic.RpcEntered - statistic.RpcCompleted;
				json["RpcBackpressure"] = backpressure;
				json["RpcBackpressureRate"] = Math.Round(statistic.RpcEnteredRate - statistic.RpcCompletedRate, 2);

				var completionRatio = (statistic.RpcEnteredRate > 0 ? statistic.RpcCompletedRate / statistic.RpcEnteredRate : 1) * 100.0;
				json["RpcCompletionRatio"] = Math.Round(Math.Min(100.0, completionRatio), 2);
				json["RpcCompletionRatioRaw"] = Math.Round(completionRatio, 2);

				var completionRatioTotal = (statistic.RpcEnteredTotal > 0 ? (double)statistic.RpcCompletedTotal / statistic.RpcEnteredTotal : 1) * 100.0;
				json["RpcCompletionRatioTotal"] = Math.Round(completionRatioTotal, 4);

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
					["Min"] = Math.Round(cpuMin, 4),
					["Max"] = Math.Round(cpuMax, 4),
					["Average"] = Math.Round(cpuAverage, 4),
					["Total"] = Math.Round(statisticsByNodes.Sum(statisticsByNode => statisticsByNode.CpuUsage), 4)
				},
				["Memory"] = new JObject
				{
					["Min"] = memoryMin,
					["Max"] = memoryMax,
					["Average"] = Math.Round(memoryAverage, 2),
					["Total"] = statisticsByNodes.Sum(statisticsByNode => statisticsByNode.MemoryUsage)
				},
				["ThreadPool"] = new JObject
				{
					["Usage"] = Math.Round(threadpoolWorkersUsage, 4),
					["Workers"] = threadpoolWorkers,
					["MaxWorkers"] = threadpoolMaxWorkers,
					["AsyncIO"] = threadpoolAsyncIO,
					["MaxAsyncIO"] = threadpoolMaxAsyncIO
				},
				["NodeMax"] = new JObject
				{
					["ID"] = nodeMaxID,
					["Workers"] = nodeMaxWorkers,
					["Usage"] = Math.Round(nodeMaxUsage, 4)
				}
			};

			var cacheJson = new JObject
			{
				["Provider"] = cacheProvider,
				["Status"] = cacheStatus,
				["Ping"] = new JObject
				{
					["Max"] = cacheMaxPing,
					["Average"] = Math.Round(cacheAveragePing, 2)
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
					["Average"] = Math.Round(cacheAverageInteractiveQueue, 2),
					["Total"] = cacheTotalInteractiveQueue
				}
			};

			var rpcJson = new JObject
			{
				["ConcurrencySlot"] = new JObject
				{
					["Usage"] = Math.Round(rpcUsage, 6),
					["Current"] = rpcCurrent,
					["Available"] = rpcAvailable,
					["Max"] = rpcMax
				},
				["RpcCall"] = new JObject
				{
					["Backpressure"] = rpcBackpressure,
					["BackpressureRate"] = Math.Round(rpcBackpressureRate, 2),
					["CompletionRatio"] = Math.Round(Math.Min(100.0, rpcCompletionRatio), 2),
					["CompletionRatioRaw"] = Math.Round(rpcCompletionRatio, 2),
					["CompletionRatioTotal"] = Math.Round(rpcCompletionRatioTotal, 4),
					["InFlight"] = rpcInFlight,
					["Rejected"] = rpcRejected,
					["RejectedRate"] = Math.Round(rpcRejectedRate, 2),
					["Entered"] = rpcEntered,
					["EnteredRate"] = Math.Round(rpcEnteredRate, 2),
					["Completed"] = rpcCompleted,
					["CompletedRate"] = Math.Round(rpcCompletedRate, 2),
					["AverageLatency"] = Math.Round(rpcAverageLatency, 2),
					["MaxLatency"] = rpcMaxLatency,
					["RejectedTotal"] = rpcRejectedTotal,
					["RejectedTotalRate"] = Math.Round(rpcRejectedTotalRate, 2),
					["EnteredTotal"] = rpcEnteredTotal,
					["EnteredTotalRate"] = Math.Round(rpcEnteredTotalRate, 2),
					["CompletedTotal"] = rpcCompletedTotal,
					["CompletedTotalRate"] = Math.Round(rpcCompletedTotalRate, 2),
					["AverageLatencyTotal"] = Math.Round(rpcAverageLatencyTotal, 2),
					["MaxLatencyTotal"] = rpcMaxLatencyTotal
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
				long rpcInFlight = 0;
				long rpcRejected = 0, rpcEntered = 0, rpcCompleted = 0;
				double rpcRejectedRate = 0, rpcEnteredRate = 0, rpcCompletedRate = 0;
				long rpcRejectedTotal = 0, rpcEnteredTotal = 0, rpcCompletedTotal = 0;
				double rpcRejectedTotalRate = 0, rpcEnteredTotalRate = 0, rpcCompletedTotalRate = 0;

				double rpcWeightedLatency = 0, rpcWeightedLatencyTotal = 0;
				long rpcCompletedCount = 0, rpcMaxLatency = 0, rpcCompletedTotalCount = 0, rpcMaxLatencyTotal = 0;

				var useL1Cache = false;
				foreach (var message in groupMessages)
				{
					useL1Cache = useL1Cache || message.UseL1Cache;

					threadpoolMaxWorkers = Math.Max(threadpoolMaxWorkers, message.ThreadPoolMaxWorkers);
					threadpoolMaxAsyncIO = Math.Max(threadpoolMaxAsyncIO, message.ThreadPoolMaxAsyncIO);

					if (isNodeScope)
					{
						threadpoolWorkers = Math.Max(threadpoolWorkers, message.ThreadPoolWorkers);
						threadpoolAsyncIO = Math.Max(threadpoolAsyncIO, message.ThreadPoolAsyncIO);

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
						rpcInFlight = Math.Max(rpcInFlight, message.RpcInFlight);

						rpcRejected = Math.Max(rpcRejected, message.RpcRejected);
						rpcRejectedRate += message.RpcRejectedRate;
						rpcRejectedTotal = Math.Max(rpcRejectedTotal, message.RpcRejectedTotal);
						rpcRejectedTotalRate += message.RpcRejectedTotalRate;

						rpcEntered = Math.Max(rpcEntered, message.RpcEntered);
						rpcEnteredRate += message.RpcEnteredRate;
						rpcEnteredTotal = Math.Max(rpcEnteredTotal, message.RpcEnteredTotal);
						rpcEnteredTotalRate += message.RpcEnteredTotalRate;

						rpcCompleted = Math.Max(rpcCompleted, message.RpcCompleted);
						rpcCompletedRate += message.RpcCompletedRate;
						rpcCompletedTotal = Math.Max(rpcCompletedTotal, message.RpcCompletedTotal);
						rpcCompletedTotalRate += message.RpcCompletedTotalRate;
					}

					if (message.RpcCompleted > 0)
					{
						rpcWeightedLatency += message.RpcAverageLatency * message.RpcCompleted;
						rpcCompletedCount += message.RpcCompleted;
					}

					if (message.RpcMaxLatency > rpcMaxLatency)
						rpcMaxLatency = message.RpcMaxLatency;

					if (message.RpcCompletedTotal > 0)
					{
						rpcWeightedLatencyTotal += message.RpcAverageLatencyTotal * message.RpcCompletedTotal;
						rpcCompletedTotalCount += message.RpcCompletedTotal;
					}

					if (message.RpcMaxLatencyTotal > rpcMaxLatencyTotal)
						rpcMaxLatencyTotal = message.RpcMaxLatencyTotal;
				}

				var sampleCount = groupMessages.Count();
				if (isNodeScope)
				{
					requestsRate = sampleCount > 0 ? requestsRate / sampleCount : requestsRate;
					rpcEnteredRate = sampleCount > 0 ? rpcEnteredRate / sampleCount : rpcEnteredRate;
					rpcCompletedRate = sampleCount > 0 ? rpcCompletedRate / sampleCount : rpcCompletedRate;
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
					rpcInFlight = nodeMessages.Sum(messages => messages.Max(message => message.RpcInFlight));

					rpcRejected = nodeMessages.Sum(messages => messages.Max(message => message.RpcRejected));
					rpcRejectedRate = nodeMessages.Sum(messages => messages.Average(message => message.RpcRejectedRate));
					rpcRejectedTotal = nodeMessages.Sum(messages => messages.Max(message => message.RpcRejectedTotal));
					rpcRejectedTotalRate = nodeMessages.Sum(messages => messages.Average(message => message.RpcRejectedTotalRate));

					rpcEntered = nodeMessages.Sum(messages => messages.Max(message => message.RpcEntered));
					rpcEnteredRate = nodeMessages.Sum(messages => messages.Average(message => message.RpcEnteredRate));
					rpcEnteredTotal = nodeMessages.Sum(messages => messages.Max(message => message.RpcEnteredTotal));
					rpcEnteredTotalRate = nodeMessages.Sum(messages => messages.Average(message => message.RpcEnteredTotalRate));

					rpcCompleted = nodeMessages.Sum(messages => messages.Max(message => message.RpcCompleted));
					rpcCompletedRate = nodeMessages.Sum(messages => messages.Average(message => message.RpcCompletedRate));
					rpcCompletedTotal = nodeMessages.Sum(messages => messages.Max(message => message.RpcCompletedTotal));
					rpcCompletedTotalRate = nodeMessages.Sum(messages => messages.Average(message => message.RpcCompletedTotalRate));
				}

				var rpcAverageLatency = rpcCompletedCount > 0 ? rpcWeightedLatency / rpcCompletedCount : 0;
				var rpcAverageLatencyTotal = rpcCompletedTotalCount > 0 ? rpcWeightedLatencyTotal / rpcCompletedTotalCount : 0;

				var serviceName = groupMessages.Key;
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
					RpcGateUsage = rpcGateMax > 0	? (double)rpcGateCurrent / rpcGateMax	: 0,

					RpcInFlight = rpcInFlight,

					RpcRejected = rpcRejected,
					RpcRejectedRate = rpcRejectedRate,
					RpcRejectedTotal = rpcRejectedTotal,
					RpcRejectedTotalRate = rpcRejectedTotalRate,

					RpcEntered = rpcEntered,
					RpcEnteredRate = rpcEnteredRate,
					RpcEnteredTotal = rpcEnteredTotal,
					RpcEnteredTotalRate = rpcEnteredTotalRate,

					RpcCompleted = rpcCompleted,
					RpcCompletedRate = rpcCompletedRate,
					RpcCompletedTotal = rpcCompletedTotal,
					RpcCompletedTotalRate = rpcCompletedTotalRate,

					RpcAverageLatency = rpcAverageLatency,
					RpcMaxLatency = rpcMaxLatency,
					RpcAverageLatencyTotal = rpcAverageLatencyTotal,
					RpcMaxLatencyTotal = rpcMaxLatencyTotal
				});
			}

			return statistics;
		}

		static readonly string[] NotAvailableInAPIGateway = new[] { "CacheL1Hit304", "CacheL1Hit200", "CacheL1Miss", "CacheL1Bypass", "CacheL1HitRatio", "CacheL1MissRatio", "CacheL1BypassRatio", "CacheL2Hit304", "CacheL2Hit200", "CacheL2Miss", "CacheL2Bypass", "CacheL2HitRatio", "CacheL2MissRatio", "CacheL2BypassRatio" };
		static readonly string[] NotAvailableInDownstream = new[] { "RequestsTotal", "RequestsInFlight", "RequestsRate", "RpcGateUsage", "RpcGateCurrent", "RpcGateAvailable", "RpcGateMax", "RpcInFlight", "RpcRejected", "RpcRejectedRate", "RpcRejectedTotal", "RpcRejectedTotalRate", "RpcEntered", "RpcEnteredRate", "RpcEnteredTotal", "RpcEnteredTotalRate", "RpcCompleted", "RpcCompletedRate", "RpcCompletedTotal", "RpcCompletedTotalRate", "RpcAverageLatency", "RpcMaxLatency", "RpcAverageLatencyTotal", "RpcMaxLatencyTotal", "RpcBackpressure", "RpcBackpressureRate", "RpcCompletionRatio", "RpcCompletionRatioRaw", "RpcCompletionRatioTotal" };
	}
}
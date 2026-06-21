#region Related components
using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using net.vieapps.Components.Utility;
#endregion

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

		public void RpcCompleted(DateTime time)
			=> this.RpcCompleted((DateTime.Now - time).TotalMilliseconds.As<long>());

		public double RpcAverageLatency
		{
			get
			{
				var count = Volatile.Read(ref this._rpcCompleted);
				return count <= 0 ? 0 : (double)Volatile.Read(ref this._rpcLatency) / count;
			}
		}

		public long RpcMaxLatency => Volatile.Read(ref this._rpcMaxLatency);

		public double RpcAverageLatencyTotal
		{
			get
			{
				var count = Volatile.Read(ref this._rpcCompletedTotal);
				return count <= 0 ? 0 : (double)Volatile.Read(ref this._rpcLatencyTotal) / count;
			}
		}

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

		public double RpcSlotUsage;
		public long RpcSlotCurrent;
		public long RpcSlotAvailable;
		public long RpcSlotMax;

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
}
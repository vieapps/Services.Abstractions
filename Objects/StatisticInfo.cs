using System;
using System.Diagnostics;
using net.vieapps.Components.Utility;

namespace net.vieapps.Services
{
	/// <summary>
	/// Presents information of a statistic
	/// </summary>
	[Serializable]
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
}
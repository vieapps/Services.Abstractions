using System;
using System.Diagnostics;
using net.vieapps.Components.Utility;

namespace net.vieapps.Services
{
	/// <summary>
	/// Presents information of a base counter
	/// </summary>
	[Serializable, DebuggerDisplay("Type = {Type}, Total = {Total}")]
	public class CounterBase : ServiceObjectBase
	{
		public CounterBase() { }

		/// <summary>
		/// Gets or sets the type of counter
		/// </summary>
		public string Type { get; set; } = "View";

		/// <summary>
		/// Gets or sets the total of counting number
		/// </summary>
		public int Total { get; set; } = 0;
	}

	//  -------------------------------------------------------------

	/// <summary>
	/// Presents information of a counter of an object
	/// </summary>
	[Serializable]
	public class CounterInfo : CounterBase
	{
		public CounterInfo()
			: base() { }

		/// <summary>
		/// Gets or sets the last updated time
		/// </summary>
		public DateTime LastUpdated { get; set; } = DateTime.Now;

		/// <summary>
		/// Gets or sets the total of counting number of this month
		/// </summary>
		public int Month { get; set; } = 0;

		/// <summary>
		/// Gets or sets the total of counting number of this week
		/// </summary>
		public int Week { get; set; } = 0;
	}
}
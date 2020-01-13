using System;
using System.Collections.Generic;
using net.vieapps.Components.Utility;
namespace net.vieapps.Services
{
	/// <summary>
	/// Presents information of a service
	/// </summary>
	[Serializable]
	public class ServiceInfo
	{
		public ServiceInfo() { }

		/// <summary>
		/// Gets or sets the name of the service
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the unique name of the service
		/// </summary>
		public string UniqueName { get; set; }

		/// <summary>
		/// Gets or sets the identity of the controller that hosts and controls the service
		/// </summary>
		public string ControllerID { get; set; }

		/// <summary>
		/// Gets or sets the invoking information of the service
		/// </summary>
		public string InvokeInfo { get; set; }

		/// <summary>
		/// Gets or sets the starting time of the service
		/// </summary>
		public DateTime Timestamp { get; set; } = DateTime.Now;

		/// <summary>
		/// Gets or sets the available state of the service
		/// </summary>
		public bool Available { get; set; } = false;

		/// <summary>
		/// Gets or sets the running state of the service
		/// </summary>
		public bool Running { get; set; } = false;

		/// <summary>
		/// Returns a JSON string that represents the current object
		/// </summary>
		/// <param name="formatting"></param>
		/// <returns></returns>
		public string ToString(Newtonsoft.Json.Formatting formatting)
			=> this.ToJson().ToString(formatting);

		/// <summary>
		/// Returns a string that represents the current object
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> this.ToString(Newtonsoft.Json.Formatting.None);
	}
}
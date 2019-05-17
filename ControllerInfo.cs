using System;
using System.Collections.Generic;
namespace net.vieapps.Services
{
	/// <summary>
	/// Presents information of a controller
	/// </summary>
	[Serializable]
	public class ControllerInfo
	{
		public ControllerInfo() { }

		/// <summary>
		/// Gets or sets the identity
		/// </summary>
		public string ID { get; set; }

		/// <summary>
		/// Gets or sets the name of user on the host that running the controller
		/// </summary>
		public string User { get; set; }

		/// <summary>
		/// Gets or sets the name of the host that running the controller
		/// </summary>
		public string Host { get; set; }

		/// <summary>
		/// Gets or sets the OS platform information of the host that running the controller
		/// </summary>
		public string Platform { get; set; }

		/// <summary>
		/// Gets or sets the running mode (Interactive or Background)
		/// </summary>
		public string Mode { get; set; }

		/// <summary>
		/// Gets or sets the avalable state
		/// </summary>
		public bool Available { get; set; } = false;

		/// <summary>
		/// Gets or sets the starting time
		/// </summary>
		public DateTime Timestamp { get; set; } = DateTime.Now;

		/// <summary>
		/// Gets or sets the extra information
		/// </summary>
		public Dictionary<string, string> Extra { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
	}
}
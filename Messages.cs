﻿using Newtonsoft.Json.Linq;
namespace net.vieapps.Services
{
	/// <summary>
	/// Presents a base message for updating information
	/// </summary>
	public class BaseMessage
	{
		public BaseMessage() { }

		/// <summary>
		/// Gets or sets type of the message
		/// </summary>
		public string Type { get; set; } = "";

		/// <summary>
		/// Gets or sets data of the message
		/// </summary>
		public JToken Data { get; set; } = new JObject();
	}

	//  --------------------------------------------------------------------------------------------

	/// <summary>
	/// Presents a message for updating via RTU (Real-Time Update)
	/// </summary>
	public class UpdateMessage : BaseMessage
	{
		public UpdateMessage() : this(null) { }

		public UpdateMessage(BaseMessage message) : base()
		{
			this.Type = message?.Type ?? "";
			this.Data = message?.Data ?? new JObject();
		}

		/// <summary>
		/// Gets or sets identity of device that received the message
		/// </summary>
		public string DeviceID { get; set; } = "";

		/// <summary>
		/// Gets or sets the identity of excluded devices
		/// </summary>
		public string ExcludedDeviceID { get; set; } = "";
	}

	//  --------------------------------------------------------------------------------------------

	/// <summary>
	/// Presents a message for communicating between services
	/// </summary>
	public class CommunicateMessage : BaseMessage
	{
		public CommunicateMessage() : this(null) { }

		public CommunicateMessage(string serviceName, BaseMessage message = null) : base()
		{
			this.ServiceName = serviceName ?? "";
			this.Type = message?.Type ?? "";
			this.Data = message?.Data ?? new JObject();
		}

		/// <summary>
		/// Gets or sets name of the service that received and processed the message
		/// </summary>
		public string ServiceName { get; set; }

		/// <summary>
		/// Gets or sets the identity of excluded node
		/// </summary>
		public string ExcludedNodeID { get; set; } = "";
	}
}
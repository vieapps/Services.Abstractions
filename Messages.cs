using Newtonsoft.Json.Linq;
namespace net.vieapps.Services
{
	/// <summary>
	/// Presents a message for updating information
	/// </summary>
	public interface IServiceMessage
	{
		/// <summary>
		/// Gets or sets type of update message
		/// </summary>
		string Type { get; set; }

		/// <summary>
		/// Gets or sets data of update message
		/// </summary>
		JToken Data { get; set; }
	}

	//  --------------------------------------------------------------------------------------------

	/// <summary>
	/// Presents a message for updating via RTU (Real-Time Update)
	/// </summary>
	public interface IUpdateMessage : IServiceMessage
	{
		/// <summary>
		/// Gets or sets identity of device that received the message
		/// </summary>
		string DeviceID { get; set; }

		/// <summary>
		/// Gets or sets the identity of excluded devices
		/// </summary>
		string ExcludedDeviceID { get; set; }
	}

	//  --------------------------------------------------------------------------------------------

	/// <summary>
	/// Presents a message for communicating between services
	/// </summary>
	public interface ICommunicateMessage : IServiceMessage
	{
		/// <summary>
		/// Gets or sets name of the service that received and processed the message
		/// </summary>
		string ServiceName { get; set; }
	}
}
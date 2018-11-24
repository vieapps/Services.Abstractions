using net.vieapps.Components.Security;
namespace net.vieapps.Services
{
	/// <summary>
	/// Presents a working session
	/// </summary>
	public interface ISession
	{
		/// <summary>
		/// Gets or sets the identity of session
		/// </summary>
		string SessionID { get; set; }

		/// <summary>
		/// Gets or sets the device's identity (Device UUID) that associates with this session
		/// </summary>
		string DeviceID { get; set; }

		/// <summary>
		/// Gets or sets the IP address of client's device
		/// </summary>
		string IP { get; set; }

		/// <summary>
		/// Gets or sets the name of the the app that associates with this session
		/// </summary>
		string AppName { get; set; }

		/// <summary>
		/// Gets or sets the name of the platform of the app that associates with this session
		/// </summary>
		string AppPlatform { get; set; }

		/// <summary>
		/// Gets or sets the agent info (agent string) of the app that associates with this session
		/// </summary>
		string AppAgent { get; set; }

		/// <summary>
		/// Gets or sets the origin info (origin or refer url) of the app that associates with this session
		/// </summary>
		string AppOrigin { get; set; }

		/// <summary>
		/// Gets or sets the mode of the app that associates with this session
		/// </summary>
		string AppMode { get; set; }

		/// <summary>
		/// Gets or sets the information of user who performs the action in the sesssion
		/// </summary>
		User User { get; set; }

		/// <summary>
		/// Gets or sets two-factors verification status
		/// </summary>
		bool Verification { get; set; }
	}
}
using net.vieapps.Components.Security;
namespace net.vieapps.Services
{
	/// <summary>
	/// Presents a working session
	/// </summary>
	[System.Diagnostics.DebuggerDisplay("ID = {SessionID}, IP = {IP}")]
	public class Session : ServiceObjectBase
	{
		/// <summary>
		/// Initializes a new session
		/// </summary>
		public Session()
			: this(null) { }

		/// <summary>
		/// Initializes a new session
		/// </summary>
		/// <param name="session"></param>
		public Session(Session session)
		{
			this.SessionID = session?.SessionID ?? "";
			this.User = session?.User ?? User.GetDefault(this.SessionID);
			this.Verified = session != null && session.Verified;
			this.DeviceID = session?.DeviceID ?? "";
			this.IP = session?.IP ?? "";
			this.DeveloperID = session?.DeveloperID ?? "";
			this.AppID = session?.AppID ?? "";
			this.AppName = session?.AppName ?? "";
			this.AppPlatform = session?.AppPlatform ?? "";
			this.AppAgent = session?.AppAgent ?? "";
			this.AppOrigin = session?.AppOrigin ?? "";
			this.AppMode = session?.AppMode ?? "Client";
		}

		#region Properties
		/// <summary>
		/// Gets or sets the identity of the session
		/// </summary>
		public string SessionID { get; set; }

		/// <summary>
		/// Gets or sets the information of user who performs the action in the sesssion
		/// </summary>
		public User User { get; set; }

		/// <summary>
		/// Gets or sets the verification status of two-factors authentication
		/// </summary>
		public bool Verified { get; set; }

		/// <summary>
		/// Gets or sets the device's identity (Device UUID) that associates with this session
		/// </summary>
		public string DeviceID { get; set; }

		/// <summary>
		/// Gets or sets the IP address of the remote device that associates with this session
		/// </summary>
		public string IP { get; set; }

		/// <summary>
		/// Gets or sets the identity of the developer that associates with this session
		/// </summary>
		public string DeveloperID { get; set; }

		/// <summary>
		/// Gets or sets the identity of the app that associates with this session
		/// </summary>
		public string AppID { get; set; }

		/// <summary>
		/// Gets or sets the name of the the app that associates with this session
		/// </summary>
		public string AppName { get; set; }

		/// <summary>
		/// Gets or sets the name of the platform of the app that associates with this session
		/// </summary>
		public string AppPlatform { get; set; }

		/// <summary>
		/// Gets or sets the agent info (agent string) of the app that associates with this session
		/// </summary>
		public string AppAgent { get; set; }

		/// <summary>
		/// Gets or sets the origin info (origin or refer url) of the app that associates with this session
		/// </summary>
		public string AppOrigin { get; set; }

		/// <summary>
		/// Gets or sets the mode of the app that associates with this session
		/// </summary>
		public string AppMode { get; set; }
		#endregion

	}
}
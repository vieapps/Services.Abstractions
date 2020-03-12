using System;
using net.vieapps.Components.Utility;
using Newtonsoft.Json;
namespace net.vieapps.Services
{
	/// <summary>
	/// Presents the base of a service object
	/// </summary>
	[Serializable]
	public abstract class ServiceObjectBase
	{
		/// <summary>
		/// Returns a JSON string that represents the current object
		/// </summary>
		/// <param name="formatting"></param>
		/// <returns></returns>
		public virtual string ToString(Formatting formatting)
			=> this.ToJson().ToString(formatting);

		/// <summary>
		/// Returns a string that represents the current object
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> this.ToString(Formatting.None);
	}
}
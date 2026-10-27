using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using net.vieapps.Components.Utility;

namespace net.vieapps.Services
{
	/// <summary>
	/// Presents the base of a service object
	/// </summary>
	public abstract class ServiceObjectBase
	{
		/// <summary>
		/// Returns a JSON string that represents the current object
		/// </summary>
		/// <param name="formatting"></param>
		/// <returns></returns>
		public virtual string ToString(Formatting formatting)
			=> this.ToJson(null).AsString(formatting);

		/// <summary>
		/// Returns a string that represents the current object
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> this.ToString(Formatting.None);

		/// <summary>
		/// Converts and save the JSON text into a file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="formatting"></param>
		/// <returns></returns>
		public virtual Task SaveToFileAsync(string filePath, CancellationToken cancellationToken = default, Formatting formatting = Formatting.Indented)
			=> this.ToJson(null).SaveAsTextAsync(filePath, cancellationToken, formatting);
	}
}
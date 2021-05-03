using System.Diagnostics;
namespace net.vieapps.Services
{
	/// <summary>
	/// Presents information of a rating
	/// </summary>
	[DebuggerDisplay("Type = {Type}, Total = {Total}")]
	public class RatingInfo : ServiceObjectBase
	{
		public RatingInfo() { }

		/// <summary>
		/// Gets or sets the type of rating
		/// </summary>
		public string Type { get; set; } = "General";

		/// <summary>
		/// Gets or sets the points of ratings
		/// </summary>
		public double Points { get; set; } = 0.0d;

		/// <summary>
		/// Gets or sets the total of ratings
		/// </summary>
		public int Total { get; set; } = 0;
	}
}
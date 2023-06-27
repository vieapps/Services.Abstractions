using System;
namespace net.vieapps.Services
{
	/// <summary>
	/// Presents the state of a service
	/// </summary>
	public interface ISearchingObject
	{
		/// <summary>
		/// Gets or Sets the identity
		/// </summary>
		string ID { get; set; }

		/// <summary>
		/// Gets or Sets the name of the service
		/// </summary>
		string ServiceName { get; set; }

		/// <summary>
		/// Gets or Sets the name of the object
		/// </summary>
		string ObjectName { get; set; }

		/// <summary>
		/// Gets or Sets the identity of a system
		/// </summary>
		string SystemID { get; set; }

		/// <summary>
		/// Gets or Sets the identity of a repository
		/// </summary>
		string RepositoryID { get; set; }

		/// <summary>
		/// Gets or Sets the identity of a repository entity
		/// </summary>
		string RepositoryEntityID { get; set; }

		/// <summary>
		/// Gets or Sets the title
		/// </summary>
		string Title { get; set; }

		/// <summary>
		/// Gets or Sets the summary
		/// </summary>
		string Summary { get; set; }

		/// <summary>
		/// Gets or Sets the details
		/// </summary>
		string Details { get; set; }

		/// <summary>
		/// Gets or Sets the tags
		/// </summary>
		string Tags { get; set; }

		/// <summary>
		/// Gets or Sets the identity of categories
		/// </summary>
		string Categories { get; set; }

		/// <summary>
		/// Gets or Sets the last-updated time
		/// </summary>
		DateTime LastUpdated { get; set; }
	}
}
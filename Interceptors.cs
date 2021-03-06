﻿using System.Reflection;
using WampSharp.V2;
using WampSharp.V2.Core.Contracts;
namespace net.vieapps.Services
{
	/// <summary>
	/// Presents an interceptor for registering a service
	/// </summary>
	public class RegistrationInterceptor : CalleeRegistrationInterceptor
	{
		readonly string _name;

		/// <summary>
		/// Initializes an interceptor for registering a service
		/// </summary>
		/// <param name="name">The string that presents name of the service (for registering with right URI)</param>
		/// <param name="options">The options for registering (default is round robin)</param>
		public RegistrationInterceptor(string name = null, RegisterOptions options = null)
			: base(options ?? new RegisterOptions { Invoke = WampInvokePolicy.Roundrobin })
			=> this._name = name;

		public override string GetProcedureUri(MethodInfo method)
			=> string.IsNullOrWhiteSpace(this._name)
				? base.GetProcedureUri(method)
				: string.Format(base.GetProcedureUri(method), this._name.Trim().ToLower());

		/// <summary>
		/// Creates an interceptor for registering a service
		/// </summary>
		/// <param name="name">The string that presents name of service (for marking URI)</param>
		/// <returns></returns>
		public static RegistrationInterceptor Create(string name = null, string invokePolicy = WampInvokePolicy.Roundrobin)
			=> new RegistrationInterceptor(name, new RegisterOptions { Invoke = invokePolicy });
	}

	//  --------------------------------------------------------------------------------------------

	/// <summary>
	/// Presents an interceptor for calling a service
	/// </summary>
	public class ProxyInterceptor : CalleeProxyInterceptor
	{
		readonly string _name;

		/// <summary>
		/// Initializes an interceptor for calling a service
		/// </summary>
		/// <param name="name">The string that presents name of the service (for registering with right URI)</param>
		/// <param name="options">The options for calling</param>
		public ProxyInterceptor(string name = null, CallOptions options = null)
			: base(options ?? new CallOptions { DiscloseMe = false, ReceiveProgress = false })
			=> this._name = name;

		public override string GetProcedureUri(MethodInfo method)
			=> string.IsNullOrWhiteSpace(this._name)
				? base.GetProcedureUri(method)
				: string.Format(base.GetProcedureUri(method), this._name.Trim().ToLower());

		/// <summary>
		/// Creates an interceptor for calling a service
		/// </summary>
		/// <param name="name">The string that presents name of service (for marking URI)</param>
		/// <returns></returns>
		public static CachedCalleeProxyInterceptor Create(string name = null)
			=> new CachedCalleeProxyInterceptor(new ProxyInterceptor(name));
	}
}
using System;
using Skahal.Infrastructure.Framework.Domain;

namespace Buildron.Domain.Users
{
	/// <summary>
	/// Defines an interface to an user that can authenticate at CI sserver.
	/// </summary>
	public interface IAuthUser : IAggregateRoot
	{
		#region Properties
		/// <summary>
		/// Gets or sets the domain.
		/// </summary>
		string Domain { get; set; }

		/// <summary>
		/// Gets the name of the domain and user.
		/// </summary>
		string DomainAndUserName { get; } 
	
		/// <summary>
		/// Gets or sets the username.
		/// </summary>
		string UserName { get; set; }

		/// <summary>
		/// Gets or sets the password.
		/// </summary>
		string Password { get; set; }
		#endregion
	}
}
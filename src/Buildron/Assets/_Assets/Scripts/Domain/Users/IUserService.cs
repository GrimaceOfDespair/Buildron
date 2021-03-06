﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Buildron.Domain.Builds;
using Buildron.Domain.Users;

namespace Buildron.Domain.Users
{
	/// <summary>
	/// Defines an interface to a user servce.
	/// </summary>
    public interface IUserService
    {
		/// <summary>
		/// Occurs when a user is found.
		/// </summary>
        event EventHandler<UserFoundEventArgs> UserFound;

		/// <summary>
		/// Occurs when a user is removed.
		/// </summary>
        event EventHandler<UserRemovedEventArgs> UserRemoved;

		/// <summary>
		/// Occurs when a user has triggered build.
		/// </summary>
        event EventHandler<UserTriggeredBuildEventArgs> UserTriggeredBuild;

		/// <summary>
		/// Occurs when user is updated.
		/// </summary>
        event EventHandler<UserUpdatedEventArgs> UserUpdated;
        
        /// <summary>
        /// Occurs when an user authentication is completed.
        /// </summary>
        event EventHandler<UserAuthenticationCompletedEventArgs> UserAuthenticationCompleted;

		/// <summary>
		/// Gets the users.
		/// </summary>
		/// <value>The users.</value>
		IList<IUser> Users { get; }


		/// <summary>
		/// Initialize the service.
		/// </summary>
		/// <param name="buildsProvider">Builds provider.</param>
		void Initialize(IBuildsProvider buildsProvider);
	}
}
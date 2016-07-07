using System;
using System.Collections.Generic;
using System.Linq;
using Skahal.Common;
using Skahal.Logging;
using UnityEngine;
using Buildron.Domain.Builds;

namespace Buildron.Domain.Users
{
	/// <summary>
	/// Domain service to user from continuous integration server.
	/// </summary>
	public class UserService : IUserService
	{
		#region Events
		/// <summary>
		/// Occurs when an user is found.
		/// </summary>
		public event EventHandler<UserFoundEventArgs> UserFound;

		/// <summary>
		/// Occurs when an user is updated.
		/// </summary>
		public event EventHandler<UserUpdatedEventArgs> UserUpdated;

		/// <summary>
		/// Occurs when an user triggered a build.
		/// </summary>
		public event EventHandler<UserTriggeredBuildEventArgs> UserTriggeredBuild;

		/// <summary>
		/// Occurs when an user is removed.
		/// </summary>
		public event EventHandler<UserRemovedEventArgs> UserRemoved;

		/// <summary>
		/// Occurs when an user authentication is completed.
		/// </summary>
		public event EventHandler<UserAuthenticationCompletedEventArgs> UserAuthenticationCompleted;
		#endregion

		#region Fields
		private StaticUserAvatarProvider m_userAvatarCache;
		private IList<IUserAvatarProvider> m_humanUserAvatarProviders;
		private IList<IUserAvatarProvider> m_nonHumanUserAvatarProviders;
		private ISHLogStrategy m_log;
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Buildron.Domain.Users.UserService"/> class.
		/// </summary>
		/// <param name="humanUserAvatarProviders">Human user avatar providers.</param>
		/// <param name="nonHumanAvatarProviders">Non human avatar providers.</param>
		/// <param name="log">Log.</param>
		public UserService (
			IUserAvatarProvider[] humanUserAvatarProviders, 
			IUserAvatarProvider[] nonHumanAvatarProviders,
			ISHLogStrategy log)
		{
			Users = new List<IUser> ();

			// Use a StaticUserAvatarProvier as a cached user avatar providers.
			m_userAvatarCache = new StaticUserAvatarProvider ();
			m_humanUserAvatarProviders = new List<IUserAvatarProvider> (humanUserAvatarProviders);
			m_humanUserAvatarProviders.Insert (0, m_userAvatarCache);

			m_nonHumanUserAvatarProviders = new List<IUserAvatarProvider> (nonHumanAvatarProviders);
			m_nonHumanUserAvatarProviders.Insert (0, m_userAvatarCache);

			m_log = log;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets the users.
		/// </summary>
		public IList<IUser> Users { get; private set; }
		#endregion

		#region Methods
		/// <summary>
		/// Initialize the service.
		/// </summary>
		/// <param name="buildsProvider">Builds provider.</param>
		public void Initialize (IBuildsProvider buildsProvider)
		{
			var serviceSender = typeof(UserService);
			var usersInLastBuildsUpdate = new List<IUser> ();
		
			buildsProvider.BuildUpdated += (sender, e) =>
			{
				var user = e.Build.TriggeredBy;

				if (user != null)
				{
					var previousUser = Users.FirstOrDefault (f => f == user);

					if (previousUser == null)
					{
						// New user found.
						Users.Add (user);
						UserFound.Raise (serviceSender, new UserFoundEventArgs (user));
						RaiseUserTriggeredBuildEvents (serviceSender, user, user.Builds);                       
					}
					else
					{
						var triggeredBuilds = user.Builds.Except (previousUser.Builds);
						RaiseUserTriggeredBuildEvents (serviceSender, user, triggeredBuilds);

						Users.Remove (previousUser);
						Users.Add (user);
					}

					UserUpdated.Raise (serviceSender, new UserUpdatedEventArgs (user));
					usersInLastBuildsUpdate.Add (user);
				}
			};

			buildsProvider.BuildsRefreshed += delegate
			{
				var removedUsers = Users.Except (usersInLastBuildsUpdate).ToArray ();

				m_log.Warning ("UserService.BuildsRefreshed: there is {0} users and {1} were refreshed. {2} will be removed", Users.Count, usersInLastBuildsUpdate.Count (), removedUsers.Length);

				foreach (var user in removedUsers)
				{
					Users.Remove (user);
					UserRemoved.Raise (typeof(BuildService), new UserRemovedEventArgs (user));
				}

				usersInLastBuildsUpdate.Clear ();
			};

			buildsProvider.UserAuthenticationSuccessful += delegate
			{
				// TODO: change buildsProvider.UserAuthenticationSuccessful to pass user.
				UserAuthenticationCompleted.Raise (this, new UserAuthenticationCompletedEventArgs (null, true));                
			};

			buildsProvider.UserAuthenticationFailed += delegate
			{
				UserAuthenticationCompleted.Raise (this, new UserAuthenticationCompletedEventArgs (null, false));
			};
		}

		/// <summary>
		/// Gets the user photo.
		/// </summary>
		/// <param name="user">User.</param>
		/// <param name="photoReceived">Photo received callback.</param>
		public void GetUserPhoto (IUser user, Action<Texture2D> photoReceived)
		{
			if (user != null)
			{
				if (user.Kind == UserKind.Human)
				{
					GetUserPhoto (user, photoReceived, m_humanUserAvatarProviders);
				}
				else
				{
					GetUserPhoto (user, photoReceived, m_nonHumanUserAvatarProviders);
				}
			}
		}

		private void GetUserPhoto (IUser user, Action<Texture2D> photoReceived, IList<IUserAvatarProvider> providersChain, int providerStartIndex = 0)
		{
			if (providerStartIndex < providersChain.Count)
			{
				var currentProvider = providersChain [providerStartIndex];

				currentProvider.GetUserPhoto (user, (photo) =>
				{
					if (photo == null)
					{
						GetUserPhoto (user, photoReceived, providersChain, ++providerStartIndex);
					}
					else
					{
						// Add found photo to cache.
						if (!object.ReferenceEquals (currentProvider, m_userAvatarCache))
						{
							m_userAvatarCache.AddPhoto (user.UserName, photo);
						}

						// Callback photo recieved.
						photoReceived (photo);
					}
				});
			}
		}

		private void RaiseUserTriggeredBuildEvents (Type serviceSender, IUser user, IEnumerable<IBuild> triggeredBuilds)
		{
			foreach (var build in triggeredBuilds)
			{
				UserTriggeredBuild.Raise (serviceSender, new UserTriggeredBuildEventArgs (user, build));
			}
		}
		#endregion
	}
}
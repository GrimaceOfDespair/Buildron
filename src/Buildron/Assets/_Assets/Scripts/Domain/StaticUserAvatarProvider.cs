#region Usings
using System;
using System.Collections.Generic;
using Buildron.Domain;
using UnityEngine;
#endregion

namespace Buildron.Domain
{
    /// <summary>
    /// An IUserAvatarProvider that use a static list of images to avatars.
    /// </summary>
    public class StaticUserAvatarProvider : IUserAvatarProvider
	{
		#region Fields
		private Dictionary<string, Texture2D> m_photosByUsername= new Dictionary<string, Texture2D> ();
		private Dictionary<UserKind, Texture2D> m_photosByKind = new Dictionary<UserKind, Texture2D> ();
		#endregion
		
		#region IUserAvatarProvider implementation
		public void GetUserPhoto (User user, Action<Texture2D> photoReceived)
		{
			if (user == null) {
				return;
			}
			
			if (m_photosByUsername.ContainsKey (user.UserName)) {
				photoReceived (m_photosByUsername [user.UserName]);
			}
			else if (m_photosByKind.ContainsKey (user.Kind)) {
				photoReceived (m_photosByKind [user.Kind]);
			}

			photoReceived (null);
		}
		#endregion
		
		#region Methods
		public void AddPhoto(string userName, Texture2D photo)
		{
			m_photosByUsername[userName] = photo;
		}

		public void AddPhoto(UserKind kind, Texture2D photo)
		{
			m_photosByKind[kind] = photo;
		}
		#endregion
	}
}
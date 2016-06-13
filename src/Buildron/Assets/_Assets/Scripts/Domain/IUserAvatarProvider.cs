#region Usings
using UnityEngine;
using System;
#endregion

namespace Buildron.Domain
{
	/// <summary>
	/// Defines a provider to user avatar.
	/// </summary>
	public interface IUserAvatarProvider
	{
        /// <summary>
        /// Gets the user photo.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="photoReceived">The photo received callback.</param>
        void GetUserPhoto(User user, Action<Texture2D> photoReceived);
	}
}
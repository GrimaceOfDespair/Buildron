﻿using System;
using Buildron.Domain.Builds;
using Buildron.Domain.Users;
using Skahal.Common;
using System.Collections.Generic;

public class EmulatorUser : IUser
{
	#region Fields
	private static int s_usersCount;
	#endregion

	#region Constructors
	public EmulatorUser()
	{
		Builds = new List<IBuild> ();

		var id = (++s_usersCount).ToString ();
		UserName = "User {0}".With (id);
		Kind = UserKind.Human;
	}
	#endregion

	#region IUser implementation
	public event EventHandler PhotoUpdated;

	public IList<IBuild> Builds { get; private set; }

	public string UserName { get; set; }

	public string Name  { get; set; }

	public string Email  { get; set; }

	public UserKind Kind { get; set; }

	public UnityEngine.Texture2D Photo  { get; set; }
	#endregion

	#region IComparable implementation
	public int CompareTo (IUser other)
	{
		throw new NotImplementedException ();
	}
	#endregion
	#region IEquatable implementation
	public bool Equals (IUser other)
	{
		throw new NotImplementedException ();
	}
	#endregion
	
}

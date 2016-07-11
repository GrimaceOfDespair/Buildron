﻿using System;
using Buildron.Domain.Mods;
using System.Collections.Generic;
using System.Linq;
using Skahal.Logging;
using Buildron.Infrastructure.AssetsProxies;

namespace Buildron.Infrastructure.ModsProvider
{
	public class AppDomainModsProvider : IModsProvider
	{
		#region Fields
		private ISHLogStrategy m_log;
		#endregion

		#region Constructors
		public AppDomainModsProvider (ISHLogStrategy log)
		{
			m_log = log;
		}
		#endregion

		#region Methods
		public IEnumerable<ModInfo> GetModInfos ()
		{
			var modInfos = new List<ModInfo> ();
			m_log.Debug ("Looking for IMod implementations in AppDomain.CurrentDomain assemblies...");
			var allTypes = AppDomain.CurrentDomain.GetAssemblies ().SelectMany (a => a.GetTypes ());
			var buildronAssemblyModTypes = allTypes.Where (t => !t.IsAbstract && typeof(IMod).IsAssignableFrom (t)).ToArray ();
		
			foreach (var modType in buildronAssemblyModTypes) 
			{
				modInfos.Add (new ModInfo(modType.Namespace));
			}

			return modInfos;
		}

		public ModInstanceInfo CreateInstance (ModInfo modInfo)
		{
			var typeName = "{0}.Mod".With (modInfo.Name);
			m_log.Debug ("Looking for type {0}...", typeName);

			var modType = Type.GetType (typeName);

			if (modType == null) {
				throw new ArgumentException ("Cannot find type '{0}'".With (typeName));
			}

			m_log.Debug ("Loading mod from class '{0}'...", modType);

			var mod = Activator.CreateInstance (modType) as IMod;

			if (mod == null) {
				throw new InvalidOperationException ("Cannot create mod, there is no default constructor.");
			}

			return new ModInstanceInfo (mod, modInfo, new ResourcesFolderAssetsProxy());
		}
		#endregion
	}
}


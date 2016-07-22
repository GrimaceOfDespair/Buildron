﻿using System;
using Buildron.Domain.Mods;
using System.Collections.Generic;
using Skahal.Logging;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Linq;
using Buildron.Infrastructure.AssetsProxies;
using Skahal.Common;
using Buildron.Infrastructure.GameObjectsProxies;
using Buildron.Infrastructure.UIProxies;

namespace Buildron.Infrastructure.ModsProvider
{
	public class FileSystemModsProvider : IModsProvider
	{
		#region Fields
		private readonly string m_rootFolder;
		private readonly ISHLogStrategy m_log;
		private readonly IUIProxy m_uiProxy;
        private Dictionary<string, AppDomain> m_createdMods = new Dictionary<string, AppDomain>();
		#endregion

		public FileSystemModsProvider (string rootFolder, ISHLogStrategy log, IUIProxy uiProxy)
		{
			Throw.AnyNull (new { rootFolder, log });

			m_rootFolder = rootFolder;
			m_log = log;
			m_uiProxy = uiProxy;
		}

		#region Methods
		public IEnumerable<ModInfo> GetModInfos()
		{
			m_log.Debug ("Getting mods informations from folder {0}...", m_rootFolder);
			var modInfos = new List<ModInfo>();
				
			var modsFolders = Directory.GetDirectories (m_rootFolder);
			m_log.Debug ("{0} mods folders", modsFolders.Length);

			foreach (var modFolder in modsFolders) {
				var modFolderName = Path.GetFileName (modFolder);

				if (modFolderName.EndsWith (".disabled", StringComparison.OrdinalIgnoreCase)) {
					m_log.Debug (modFolderName);
					continue;
				}

				modInfos.Add (new ModInfo(modFolderName));
			}

			return modInfos;
		}

		public ModInstanceInfo CreateInstance(ModInfo modInfo)
		{
			var modFolderName = modInfo.Name;
			var modFolder = Path.Combine(m_rootFolder, modFolderName);
            var modsInstancesFolder = Path.Combine(modFolder, "__instances__");
            var modInstanceFolder = Path.Combine(modsInstancesFolder,  DateTime.UtcNow.Ticks.ToString());

            if (Directory.Exists(modsInstancesFolder))
            {
                // Clear old instances and ignore the latest one, because Unity can be holding it yet.
                var oldInstanceFolders = Directory.GetDirectories(modsInstancesFolder).OrderByDescending(f => f).Skip(1).ToArray();

                foreach (var oldFolder in oldInstanceFolders)
                {
                    Directory.Delete(oldFolder, true);
                }
            }

			CopyFolder (modFolder, modInstanceFolder);
//            Directory.CreateDirectory(modInstanceFolder);
//            var filesToCopy = Directory.GetFiles(modFolder);
//            
//            foreach(var f in filesToCopy)
//            {
//                File.Copy(f, Path.Combine(modInstanceFolder, Path.GetFileName(f)));
//            }

			var modAssemblyPath = Path.Combine(modInstanceFolder, "{0}.dll".With(modFolderName));
			var modTypeFullName = "{0}.Mod".With(modFolderName);
			var modAssetBundlePath = Path.Combine(modInstanceFolder, modFolderName.ToLowerInvariant());


            //var modAssembly = Assembly.LoadFile(modAssemblyPath);
            m_log.Debug("Creating ModsAppDomain with ApplicationBase '{0}'...", modInstanceFolder);

            var domainSetup = new AppDomainSetup();
            domainSetup.ApplicationBase = modInstanceFolder;
            domainSetup.LoaderOptimization = LoaderOptimization.MultiDomainHost;
            domainSetup.ShadowCopyFiles = "true";
            var evidence = AppDomain.CurrentDomain.Evidence;
            AppDomain modAppDomain = AppDomain.CreateDomain("{0}AppDomain".With(modInfo.Name), evidence, domainSetup);

            m_log.Debug("Creating proxy...");

            var proxy = new Proxy();            

            m_log.Debug("Loading assembly '{0}'...", modAssemblyPath);
            var modAssembly = proxy.GetAssembly(modAssemblyPath);
            // AppDomain.Unload(domain);


            m_log.Debug ("Assembly loaded. Looking for type '{0}' on mod assembly {1}...", modTypeFullName, modAssembly.FullName);
			var modType = modAssembly.GetType (modTypeFullName, false, true);

			if (modType == null) {
				var availableTypes = modAssembly
					.GetTypes ()
					.Where(t => t.Name.IndexOf("mod", StringComparison.OrdinalIgnoreCase) != -1)
					.Select (t => t.Name)
					.ToArray ();
				
				throw new InvalidOperationException (
					"Mod type {0} not found. Available types on assembly are: {1}"
					.With(modTypeFullName, String.Join(", ", availableTypes)));
			}
				
			var mod = Activator.CreateInstance(modType) as IMod;

			if (mod == null)
			{
				throw new InvalidOperationException("{0} does not implement IMod interface. Will not be loaded.".With(modTypeFullName));
			}
			else 
			{
				AssetBundle assetBundle = null;

				if (File.Exists(modAssetBundlePath)) {
					m_log.Debug("Loading mod asset bundle from {0}...", modAssetBundlePath);
					assetBundle = AssetBundle.LoadFromFile(modAssetBundlePath);

					m_log.Debug("{0} Assets loaded.", assetBundle.GetAllAssetNames().Length);
				}
                
                var modInstance = new ModInstanceInfo(mod, modInfo, this, new AssetBundleAssetsProxy(assetBundle), new ModGameObjectsProxy(modInfo), m_uiProxy);
                m_createdMods.Add(modInfo.Name, modAppDomain);

                return modInstance;
			}
		}

		private void CopyFolder(string sourceFolder, string destFolder)
		{
			Debug.LogFormat ("FOLDER: {0} | {1}", sourceFolder, destFolder);
			Directory.CreateDirectory(destFolder);
			var filesToCopy = Directory.GetFiles(sourceFolder);

			foreach(var f in filesToCopy)
			{
				File.Copy(f, Path.Combine(destFolder, Path.GetFileName(f)));
			}

			var subFolders = Directory.GetDirectories (sourceFolder).Where (s => !s.EndsWith ("__instances__"));

			foreach (var subFolder in subFolders) {
				CopyFolder (subFolder, Path.Combine (destFolder, Path.GetFileName (subFolder))); 
			}
		}

        public void DestroyInstance(ModInstanceInfo modInstanceInfo)
        {
            var key = modInstanceInfo.Info.Name;

            if (m_createdMods.ContainsKey(key))
            {
                m_log.Debug("Unloading {0} AppDomain...", key);
                var modAppDomain = m_createdMods[key];
                AppDomain.Unload(modAppDomain);
                m_createdMods.Remove(key);
            }            
        }
        #endregion

        public class Proxy : MarshalByRefObject
        {
            public Assembly GetAssembly(string assemblyPath)
            {
                try
                {
                    return Assembly.LoadFile(assemblyPath);
                }
                catch (Exception)
                {
                    return null;
                    // throw new InvalidOperationException(ex);
                }
            }
        }
    }
}


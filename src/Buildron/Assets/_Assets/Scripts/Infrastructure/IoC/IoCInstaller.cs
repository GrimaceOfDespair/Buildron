using UnityEngine;
using Zenject;
using Skahal.Logging;
using Buildron.Infrastructure.UserAvatarProviders;
using Buildron.Domain;
using System;
using Skahal.Infrastructure.Framework.Commons;
using Buildron.Infrastructure.Repositories;
using Buildron.Domain.Versions;
using Buildron.Infrastructure.Clients;
using Buildron.Domain.Notifications;
using Buildron.Application;
using Buildron.Domain.EasterEggs;

public class IoCInstaller : MonoInstaller
{
	#region Editor properties
	public Texture2D ScheduledTriggerAvatar;
	public Texture2D RetryTriggerAvatar;
	public Texture2D UnunknownAvatar;
	#endregion

	public override void InstallBindings ()
	{
		var log = InstallLog ();

		log.Debug ("IOC :: Installing user bindings...");
		InstallUser ();

		log.Debug ("IOC :: Installing build bindings...");
		InstallBuild ();  

		log.Debug ("IOC :: Installing misc bindings...");
		InstallMisc ();


        DependencyService.Register<IServerStateRepository>(new PlayerPrefsServerStateRepository());
        DependencyService.Register<IVersionClient>(() => {
            return new BackEndClient();
        });

        DependencyService.Register<IVersionRepository>(() => {
            return new PlayerPrefsVersionRepository();
        });

        DependencyService.Register<INotificationClient>(() => {
            return new BackEndClient();
        });

		log.Debug ("IOC :: Bindings installed.");
	}


	ISHLogStrategy InstallLog ()
	{
		var logStrategy = SHLog.LogStrategy;
		Container.Bind<ISHLogStrategy> ().FromInstance (logStrategy).AsSingle ();

		return logStrategy;
	}

	void InstallUser ()
	{
		var humanFallbackUserAvatarProvider = new StaticUserAvatarProvider ();
		humanFallbackUserAvatarProvider.AddPhoto(UserKind.Human, UnunknownAvatar);

		var nonHumanUserAvatarProviders = new StaticUserAvatarProvider ();
		nonHumanUserAvatarProviders.AddPhoto(UserKind.ScheduledTrigger, ScheduledTriggerAvatar);
		nonHumanUserAvatarProviders.AddPhoto(UserKind.RetryTrigger, RetryTriggerAvatar);

		var userService = new UserService (new IUserAvatarProvider[] {
			new GravatarUserAvatarProvider (),
			new AcronymUserAvatarProvider (),
			humanFallbackUserAvatarProvider
		}, new IUserAvatarProvider[] {
			nonHumanUserAvatarProviders
		}, Container.Resolve<ISHLogStrategy> ());
		Container.Bind<IUserService> ().FromInstance (userService);

		Container.BindFactory<UserController, UserController.Factory>()
			.FromPrefabResource ("UserPrefab")
			.UnderGameObjectGroup("Users");
	}

	void InstallBuild ()
	{
		Container.Bind<BuildGOService> ().AsSingle();
		Container.BindFactory<BuildController, BuildController.Factory>()
			.FromPrefabResource ("BuildPrefab")
			.UnderGameObjectGroup("Builds");
	}

	void InstallMisc ()
	{
		var easterEggService = new EasterEggService (
			new IEasterEggProvider[] { 
				GameObject.Find ("MatrixEasterEggController").GetComponent<MatrixEasterEggController>(),
				GameObject.Find ("KickEasterEggController").GetComponent<KickEasterEggController>()
			}, 
			Container.Resolve<ISHLogStrategy> ());
		Container.Bind<EasterEggService> ().FromInstance(easterEggService).AsSingle();

		var serverMessagesListener = new GameObject ("ServerMessagesListener").AddComponent<ServerMessagesListener> ();
		Container.Inject (serverMessagesListener);

		Container.Bind<IInitializable> ().FromInstance (serverMessagesListener).AsSingle ();
		Container.Bind<ServerMessagesListener> ().FromInstance (serverMessagesListener).AsSingle ();
	}
}
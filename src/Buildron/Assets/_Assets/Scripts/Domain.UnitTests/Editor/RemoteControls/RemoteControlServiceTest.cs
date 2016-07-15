﻿using Buildron.Domain;
using Buildron.Domain.CIServers;
using Buildron.Domain.RemoteControls;
using Buildron.Domain.Users;
using NUnit.Framework;
using Rhino.Mocks;
using Skahal.Infrastructure.Framework.Repositories;

namespace Buildron.Domain.UnitTests.RemoteControls
{
    [Category("Buildron.Domain")]
    public class RemoteControlServiceTest
    {      
        [Test]
        public void Initialize_UserAuthenticationCompletedNotSuccess_NoRemoteControlConnected()
        {
            var ciServerService = MockRepository.GenerateMock<ICIServerService>();
            var userService = MockRepository.GenerateMock<IUserService>();
            var repository = MockRepository.GenerateMock<IRepository<IRemoteControl>>();

            var target = new RemoteControlService(ciServerService, userService, repository);
            var rc = new RemoteControl() { UserName = "u1" };
            target.ConnectRemoteControl(rc);
            Assert.AreEqual(rc, target.GetConnectedRemoteControl());
            Assert.IsTrue(rc.Connected);
            Assert.IsTrue(target.HasRemoteControlConnectedSomeDay);

            target.Initialize();
            userService.Raise(u => u.UserAuthenticationCompleted += null, null, new UserAuthenticationCompletedEventArgs(new RemoteControl(), false));

            Assert.IsNull(target.GetConnectedRemoteControl());
            Assert.IsFalse(rc.Connected);
            Assert.IsTrue(target.HasRemoteControlConnectedSomeDay);
        }

		[Test]
		public void ConnectRemoteControl_RemoteControl_EventRaised()
		{
			var ciServerService = MockRepository.GenerateMock<ICIServerService>();
			var userService = MockRepository.GenerateMock<IUserService>();
			var repository = MockRepository.GenerateMock<IRepository<IRemoteControl>>();

			var target = new RemoteControlService(ciServerService, userService, repository);
			var rc = new RemoteControl() { UserName = "u1" };

			var remoteControlChangedRaised = target.CreateAssert<RemoteControlChangedEventArgs> ("RemoteControlChanged", 1);
			target.ConnectRemoteControl(rc);
			Assert.AreEqual(rc, target.GetConnectedRemoteControl());
			Assert.IsTrue(rc.Connected);
			Assert.IsTrue(target.HasRemoteControlConnectedSomeDay);

			remoteControlChangedRaised.Assert ();
		}

		[Test]
		public void DisconnectRemoteControl_RemoteControlAlreadyConnected_EventRaised()
		{
			var ciServerService = MockRepository.GenerateMock<ICIServerService>();
			var userService = MockRepository.GenerateMock<IUserService>();
			var repository = MockRepository.GenerateMock<IRepository<IRemoteControl>>();

			var target = new RemoteControlService(ciServerService, userService, repository);
			var rc = new RemoteControl() { UserName = "u1" };

			var remoteControlChangedRaised = target.CreateAssert<RemoteControlChangedEventArgs> ("RemoteControlChanged", 2);
			target.ConnectRemoteControl(rc);
			Assert.AreEqual(rc, target.GetConnectedRemoteControl());
			Assert.IsTrue(rc.Connected);
			Assert.IsTrue(target.HasRemoteControlConnectedSomeDay);

			target.DisconnectRemoteControl ();
			Assert.IsNull(target.GetConnectedRemoteControl());
			Assert.IsFalse(rc.Connected);
			Assert.IsTrue(target.HasRemoteControlConnectedSomeDay);

			remoteControlChangedRaised.Assert ();
		}
    }
}
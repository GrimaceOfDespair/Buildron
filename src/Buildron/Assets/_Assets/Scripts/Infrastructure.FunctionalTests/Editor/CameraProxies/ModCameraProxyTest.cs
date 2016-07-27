﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Buildron.Domain.Mods;
using Buildron.Infrastructure.CameraProxies;
using NUnit.Framework;
using UnityEngine;

namespace Buildron.Infrastructure.FunctionalTests.CameraProxies
{
    [Category("Buildron.Infrastructure")]
    [Category("Unity")]
    public class ModCameraProxyTest
    {
        [Test]
        public void RegisterController_FirstOne_RegisteredAndEnabled()
        {
            var camera = new GameObject().AddComponent<Camera>();
            var target = new ModCameraProxy(camera);
            var actual = target.RegisterController<Stub1CameraController>(CameraControllerKind.Background, true);
            Assert.IsNotNull(actual);

            actual = camera.GetComponent<Stub1CameraController>();
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.enabled);
        }

        [Test]
        public void RegisterController_TwoControllersNotExclusive_TwoRegisteredAndEnabled()
        {
             var camera = new GameObject().AddComponent<Camera>();
            var target = new ModCameraProxy(camera);
            target.RegisterController<Stub1CameraController>(CameraControllerKind.Background, false);
            target.RegisterController<Stub2CameraController>(CameraControllerKind.Background, false);

            var actual1 = camera.GetComponent<Stub1CameraController>();
            Assert.IsTrue(actual1.enabled);

            var actual2 = camera.GetComponent<Stub2CameraController>();
            Assert.IsTrue(actual2.enabled);
        }

        [Test]
        public void RegisterController_TwoControllersExclusiveButDiffKind_TwoRegisteredAndEnabled()
        {
            var camera = new GameObject().AddComponent<Camera>();
            var target = new ModCameraProxy(camera);
            target.RegisterController<Stub1CameraController>(CameraControllerKind.Background, true);
            target.RegisterController<Stub2CameraController>(CameraControllerKind.Position | CameraControllerKind.Rotation, true);

            var actual1 = camera.GetComponent<Stub1CameraController>();
            Assert.IsTrue(actual1.enabled);

            var actual2 = camera.GetComponent<Stub2CameraController>();
            Assert.IsTrue(actual2.enabled);

            //
            camera = new GameObject().AddComponent<Camera>();
            target = new ModCameraProxy(camera);
            target.RegisterController<Stub1CameraController>(CameraControllerKind.Background, true);
            target.RegisterController<Stub2CameraController>(CameraControllerKind.Position, true);

            actual1 = camera.GetComponent<Stub1CameraController>();
            Assert.IsTrue(actual1.enabled);

            actual2 = camera.GetComponent<Stub2CameraController>();
            Assert.IsTrue(actual2.enabled);
        }

        [Test]
        public void RegisterController_TwoControllersExclusiveSameKind_TwoRegisteredAndLastEnabled()
        {
            var camera = new GameObject().AddComponent<Camera>();
            var target = new ModCameraProxy(camera);
            target.RegisterController<Stub1CameraController>(CameraControllerKind.Rotation, true);
            target.RegisterController<Stub2CameraController>(CameraControllerKind.Position | CameraControllerKind.Rotation, true);

            var actual1 = camera.GetComponent<Stub1CameraController>();
            Assert.IsFalse(actual1.enabled);

            var actual2 = camera.GetComponent<Stub2CameraController>();
            Assert.IsTrue(actual2.enabled);

            //
            camera = new GameObject().AddComponent<Camera>();
            target = new ModCameraProxy(camera);
            target.RegisterController<Stub1CameraController>(CameraControllerKind.Position, true);
            target.RegisterController<Stub2CameraController>(CameraControllerKind.Position, true);

            actual1 = camera.GetComponent<Stub1CameraController>();
            Assert.IsFalse(actual1.enabled);

            actual2 = camera.GetComponent<Stub2CameraController>();
            Assert.IsTrue(actual2.enabled);
        }

        [Test]
        public void RegisterController_ThreControllersNonExclusiveSameKind_ThreeRegisteredAndLastEnabled()
        {
            var camera = new GameObject().AddComponent<Camera>();
            var target = new ModCameraProxy(camera);
            target.RegisterController<Stub1CameraController>(CameraControllerKind.Rotation, false);
            target.RegisterController<Stub2CameraController>(CameraControllerKind.Position, true);
            target.RegisterController<Stub3CameraController>(CameraControllerKind.Rotation | CameraControllerKind.Background, true);

            var actual1 = camera.GetComponent<Stub1CameraController>();
            Assert.IsFalse(actual1.enabled);

            var actual2 = camera.GetComponent<Stub2CameraController>();
            Assert.IsTrue(actual2.enabled);

            var actual3 = camera.GetComponent<Stub3CameraController>();
            Assert.IsTrue(actual3.enabled);

            // Second config.
            camera = new GameObject().AddComponent<Camera>();
            target = new ModCameraProxy(camera);
            target.RegisterController<Stub3CameraController>(CameraControllerKind.Rotation | CameraControllerKind.Background, true);
            target.RegisterController<Stub1CameraController>(CameraControllerKind.Rotation, false);
            target.RegisterController<Stub2CameraController>(CameraControllerKind.Position, true);

            actual3 = camera.GetComponent<Stub3CameraController>();
            Assert.IsFalse(actual3.enabled);

            actual1 = camera.GetComponent<Stub1CameraController>();
            Assert.IsTrue(actual1.enabled);

            actual2 = camera.GetComponent<Stub2CameraController>();
            Assert.IsTrue(actual2.enabled);           
        }

        [Test]
        public void RegisterController_AlreadyRegisteredController_Exception()
        {
            var camera = new GameObject().AddComponent<Camera>();
            var target = new ModCameraProxy(camera);
            target.RegisterController<Stub1CameraController>(CameraControllerKind.Rotation, true);
            target.RegisterController<Stub1CameraController>(CameraControllerKind.Position | CameraControllerKind.Rotation, true);
            var actual = camera.GetComponent<Stub1CameraController>();
            Assert.IsTrue(actual.enabled);
        }

        [Test]
        public void UnregisterController_NotRegistered_Nothing()
        {
            var camera = new GameObject().AddComponent<Camera>();
            var target = new ModCameraProxy(camera);            
            target.UnregisterController<Stub1CameraController>();            
        }

        [Test]
        public void UnregisterController_RegisteredAndNotEnabled_UnregisterAndKeepAlreadyEnabled()
        {
             var camera = new GameObject().AddComponent<Camera>();
            var target = new ModCameraProxy(camera);

            var actual1 = new Stub1CameraController();
            target.RegisterController<Stub1CameraController>(CameraControllerKind.Rotation, true);
            target.RegisterController<Stub2CameraController>(CameraControllerKind.Position | CameraControllerKind.Rotation, true);

            target.UnregisterController<Stub1CameraController>();

            actual1 = camera.GetComponent<Stub1CameraController>();
            Assert.IsFalse(actual1.enabled);

            var actual2 = camera.GetComponent<Stub2CameraController>();
            Assert.IsTrue(actual2.enabled);
        }

        [Test]
        public void UnregisterController_RegisteredAndEnabledButThereIsNoOtherController_UnregisterOnly()
        {
             var camera = new GameObject().AddComponent<Camera>();
            var target = new ModCameraProxy(camera);
            var actual1 = new Stub1CameraController();
            target.RegisterController<Stub1CameraController>(CameraControllerKind.Rotation, true);

            target.UnregisterController<Stub1CameraController>();

            actual1 = camera.GetComponent<Stub1CameraController>();
            Assert.IsFalse(actual1.enabled);
        }

        [Test]
        public void UnregisterController_RegisteredAndEnabled_UnregisterAndEnableTheLastOne()
        {
             var camera = new GameObject().AddComponent<Camera>();
            var target = new ModCameraProxy(camera);

            target.RegisterController<Stub1CameraController>(CameraControllerKind.Rotation, true);
            target.RegisterController<Stub2CameraController>(CameraControllerKind.Position | CameraControllerKind.Rotation, true);
            target.RegisterController<Stub3CameraController>(CameraControllerKind.Position | CameraControllerKind.Rotation, true);

            target.UnregisterController<Stub3CameraController>();

            var actual1 = camera.GetComponent<Stub1CameraController>();
            Assert.IsFalse(actual1.enabled);

            var actual2 = camera.GetComponent<Stub2CameraController>();
            Assert.IsTrue(actual2.enabled);

            var actual3 = camera.GetComponent<Stub3CameraController>();
            Assert.IsFalse(actual3.enabled);
        }

        [Test]
        public void UnregisterController_RegisteredAndUnregisterAndRegisterAgain_SameInstanceUsed()
        {
            var camera = new GameObject().AddComponent<Camera>();
            var target = new ModCameraProxy(camera);

            target.RegisterController<Stub1CameraController>(CameraControllerKind.Rotation, true);
            target.RegisterController<Stub2CameraController>(CameraControllerKind.Position | CameraControllerKind.Rotation, true);
            var actual1 = target.RegisterController<Stub3CameraController>(CameraControllerKind.Position | CameraControllerKind.Rotation, true);

            target.UnregisterController<Stub3CameraController>();
            var actual2 = target.RegisterController<Stub3CameraController>(CameraControllerKind.Position | CameraControllerKind.Rotation, false);

            Assert.AreSame(actual1, actual2);
            actual1 = camera.GetComponent<Stub3CameraController>();
            Assert.IsTrue(actual1.enabled);
        }
    }
}

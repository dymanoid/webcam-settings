using System;
using System.Collections.Generic;
using System.Linq;
using DirectShowLib;

namespace WebcamSettings
{
    internal sealed class CameraController
    {
        public Camera GetCameraSettings(int cameraIndex)
        {
            (string devicePath, object camera) = GetCamera(cameraIndex);

            using var cleanup = new CleanupContainer();
            cleanup.Add(camera);

            var propertyPages = (ISpecifyPropertyPages)camera;
            if (((IBaseFilter)camera).QueryFilterInfo(out var cameraFilterInfo) != 0)
            {
                throw new InvalidOperationException("Failed to obtain the camera information.");
            }

            cleanup.Add(cameraFilterInfo);

            if (propertyPages.GetPages(out var pages) != 0)
            {
                throw new InvalidOperationException("Failed to obtain the camera properties.");
            }

            cleanup.Add(pages);

            if (NativeMethods.OleCreatePropertyFrame(IntPtr.Zero, 0, 0, cameraFilterInfo.achName, 1, ref camera, pages.cElems, pages.pElems, 0, 0, IntPtr.Zero) != 0)
            {
                throw new InvalidOperationException("Failed to show camera settings window.");
            }

            var videoSettings = (IAMVideoProcAmp)camera;
            var cameraControl = (IAMCameraControl)camera;

            var cameraSettings = new Camera { DevicePath = devicePath };

            StoreSettings<CameraControlProperty>(Enum.GetValues(typeof(CameraControlProperty)), cameraSettings.CameraControls, QueryCameraControls);
            StoreSettings<VideoProcAmpProperty>(Enum.GetValues(typeof(VideoProcAmpProperty)), cameraSettings.VideoControls, QueryVideoControls);

            return cameraSettings;

            (int value, bool auto)? QueryCameraControls(CameraControlProperty property)
            {
                return cameraControl.Get(property, out var value, out var flags) == 0
                    ? (value, flags == CameraControlFlags.Auto)
                    : null;
            }

            (int value, bool auto)? QueryVideoControls(VideoProcAmpProperty property)
            {
                return videoSettings.Get(property, out var value, out var flags) == 0
                    ? (value, flags == VideoProcAmpFlags.Auto)
                    : null;
            }
        }

        public void ApplySettings(Camera settings)
        {
            var camera = GetCamera(settings.DevicePath);

            using var cleanup = new CleanupContainer();
            cleanup.Add(camera);

            var videoSettings = (IAMVideoProcAmp)camera;
            var cameraControl = (IAMCameraControl)camera;

            ApplySettings<CameraControlProperty>(Enum.GetValues(typeof(CameraControlProperty)), settings.CameraControls, SetCameraControls);
            ApplySettings<VideoProcAmpProperty>(Enum.GetValues(typeof(VideoProcAmpProperty)), settings.VideoControls, SetVideoControls);

            void SetCameraControls(CameraControlProperty property, int value, bool auto)
            {
                cameraControl.Set(property, value, auto ? CameraControlFlags.Auto : CameraControlFlags.Manual);
            }

            void SetVideoControls(VideoProcAmpProperty property, int value, bool auto)
            {
                videoSettings.Set(property, value, auto ? VideoProcAmpFlags.Auto : VideoProcAmpFlags.Manual);
            }
        }

        private static void StoreSettings<T>(Array properties, IDictionary<string, CameraSetting> target, Func<T, (int value, bool auto)?> query)
        {
            foreach (T property in properties)
            {
                var queryResult = query(property);

                if (queryResult is not null)
                {
                    var setting = new CameraSetting
                    {
                        Value = queryResult.Value.value,
                        IsAuto = queryResult.Value.auto
                    };

                    target.Add(property.ToString(), setting);
                }
            }
        }

        private static void ApplySettings<T>(Array properties, IDictionary<string, CameraSetting> settings, Action<T, int, bool> setter)
        {
            foreach (T property in properties)
            {
                if (settings.TryGetValue(property.ToString(), out var setting))
                {
                    setter(property, setting.Value, setting.IsAuto);
                }
            }
        }

        private static (string devicePath, object camera) GetCamera(int cameraIndex)
        {
            var devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            if (cameraIndex >= devices.Length)
            {
                throw new InvalidOperationException($"Requested camera with index {cameraIndex} but only {devices.Length} devices detected.");
            }

            var cameraDevice = devices[cameraIndex];
            return (cameraDevice.DevicePath, GetCameraFilter(cameraDevice));
        }

        private static object GetCamera(string devicePath)
        {
            var devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            var cameraDevice = devices.FirstOrDefault(d => d.DevicePath == devicePath);
            if (cameraDevice == null)
            {
                throw new InvalidOperationException("Specified camera was not found.");
            }

            return GetCameraFilter(cameraDevice);
        }

        private static object GetCameraFilter(DsDevice cameraDevice)
        {
            Guid filterId = typeof(IBaseFilter).GUID;
            cameraDevice.Mon.BindToObject(null, null, ref filterId, out var camera);

            if (camera == null)
            {
                throw new InvalidOperationException("Failed to obtain the camera configuration.");
            }

            return camera;
        }
    }
}

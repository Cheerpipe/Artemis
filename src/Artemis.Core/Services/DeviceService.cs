﻿using System;
using System.Threading.Tasks;
using Artemis.Core.Events;
using Artemis.Core.Services.Interfaces;
using RGB.NET.Brushes;
using RGB.NET.Core;
using RGB.NET.Devices.CoolerMaster;
using RGB.NET.Devices.Corsair;
using RGB.NET.Devices.Logitech;
using RGB.NET.Groups;

namespace Artemis.Core.Services
{
    public class DeviceService : IDeviceService, IDisposable
    {
        public DeviceService()
        {
            Surface = RGBSurface.Instance;
            LoadingDevices = false;

            // Let's throw these for now
            Surface.Exception += SurfaceOnException;
        }

        public bool LoadingDevices { get; private set; }

        public RGBSurface Surface { get; set; }

        public async Task LoadDevices()
        {
            OnStartedLoadingDevices();

            await Task.Run(() =>
            {
                // TODO SpoinkyNL 8-1-18: Keep settings into account
//                Surface.LoadDevices(AsusDeviceProvider.Instance);
                Surface.LoadDevices(CorsairDeviceProvider.Instance);
                Surface.LoadDevices(LogitechDeviceProvider.Instance);
                Surface.LoadDevices(CoolerMasterDeviceProvider.Instance);
//                Surface.LoadDevices(NovationDeviceProvider.Instance);

                // TODO SpoinkyNL 8-1-18: Load alignment
                Surface.AlignDevies();

                // Do some testing, why does this work, how does it know I want to target the surface? Check source!
                var ledGroup = new RectangleLedGroup(Surface.SurfaceRectangle)
                {
                    Brush = new SolidColorBrush(new Color(255, 0, 0)) {BrushCalculationMode = BrushCalculationMode.Absolute}
                };
                Surface.UpdateMode = UpdateMode.Continuous;
            });

            OnFinishedLoadedDevices();
        }

        public void Dispose()
        {
            Surface.Dispose();
        }

        private void SurfaceOnException(ExceptionEventArgs args)
        {
            throw args.Exception;
        }

        #region Events

        /// <summary>
        ///     Occurs when a single device has loaded
        /// </summary>
        public event EventHandler<DeviceEventArgs> DeviceLoaded;

        /// <summary>
        ///     Occurs when a single device has reloaded
        /// </summary>
        public event EventHandler<DeviceEventArgs> DeviceReloaded;

        /// <summary>
        ///     Occurs when loading all devices has started
        /// </summary>
        public event EventHandler StartedLoadingDevices;

        /// <summary>
        ///     Occurs when loading all devices has finished
        /// </summary>
        public event EventHandler FinishedLoadedDevices;

        private void OnDeviceLoaded(DeviceEventArgs e)
        {
            DeviceLoaded?.Invoke(this, e);
        }

        private void OnDeviceReloaded(DeviceEventArgs e)
        {
            DeviceReloaded?.Invoke(this, e);
        }

        private void OnStartedLoadingDevices()
        {
            LoadingDevices = true;
            StartedLoadingDevices?.Invoke(this, EventArgs.Empty);
        }

        private void OnFinishedLoadedDevices()
        {
            LoadingDevices = false;
            FinishedLoadedDevices?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }

    public interface IDeviceService : IArtemisService
    {
        Task LoadDevices();
    }
}
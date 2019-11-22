﻿using Artemis.Core.Services;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Settings.Debug;
using Artemis.UI.Screens.Settings.Tabs.Devices;
using Ninject;
using Stylet;

namespace Artemis.UI.Screens.Settings
{
    public class SettingsViewModel : Screen, IScreenViewModel
    {
        private readonly IDeviceSettingsViewModelFactory _deviceSettingsViewModelFactory;
        private readonly IKernel _kernel;
        private readonly ISettingsService _settingsService;
        private readonly ISurfaceService _surfaceService;
        private readonly IWindowManager _windowManager;

        public SettingsViewModel(IKernel kernel,
            ISurfaceService surfaceService,
            IWindowManager windowManager,
            ISettingsService settingsService,
            IDeviceSettingsViewModelFactory deviceSettingsViewModelFactory)
        {
            _kernel = kernel;
            _surfaceService = surfaceService;
            _windowManager = windowManager;
            _settingsService = settingsService;
            _deviceSettingsViewModelFactory = deviceSettingsViewModelFactory;

            DeviceSettingsViewModels = new BindableCollection<DeviceSettingsViewModel>();
        }

        public BindableCollection<DeviceSettingsViewModel> DeviceSettingsViewModels { get; set; }

        public double RenderScale
        {
            get => _settingsService.GetSetting("RenderScale", 1.0).Value;
            set
            {
                _settingsService.GetSetting("RenderScale", 1.0).Value = value;
                _settingsService.GetSetting("RenderScale", 1.0).Save();
            }
        }

        public int TargetFrameRate
        {
            get => _settingsService.GetSetting("TargetFrameRate", 25).Value;
            set
            {
                _settingsService.GetSetting("TargetFrameRate", 25).Value = value;
                _settingsService.GetSetting("TargetFrameRate", 25).Save();
            }
        }

        public string Title => "Settings";

        protected override void OnActivate()
        {
            DeviceSettingsViewModels.Clear();
            foreach (var device in _surfaceService.ActiveSurface.Devices)
                DeviceSettingsViewModels.Add(_deviceSettingsViewModelFactory.Create(device));

            base.OnActivate();
        }

        public void ShowDebugger()
        {
            _windowManager.ShowWindow(_kernel.Get<DebugViewModel>());
        }
    }
}
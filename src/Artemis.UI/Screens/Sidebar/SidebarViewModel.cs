﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Events;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Ninject.Factories;
using MaterialDesignExtensions.Controls;
using MaterialDesignExtensions.Model;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens.Sidebar
{
    public class SidebarViewModel : PropertyChangedBase
    {
        private readonly IModuleVmFactory _moduleVmFactory;
        private readonly IPluginService _pluginService;

        public SidebarViewModel(List<MainScreenViewModel> defaultSidebarItems, IModuleVmFactory moduleVmFactory, IPluginService pluginService)
        {
            _moduleVmFactory = moduleVmFactory;
            _pluginService = pluginService;

            DefaultSidebarItems = defaultSidebarItems;
            SidebarModules = new Dictionary<INavigationItem, Core.Plugins.Abstract.Module>();
            SidebarItems = new BindableCollection<INavigationItem>();

            SetupSidebar();
            _pluginService.PluginEnabled += PluginServiceOnPluginEnabled;
            _pluginService.PluginDisabled += PluginServiceOnPluginDisabled;
        }

        public List<MainScreenViewModel> DefaultSidebarItems { get; set; }
        public BindableCollection<INavigationItem> SidebarItems { get; set; }
        public Dictionary<INavigationItem, Core.Plugins.Abstract.Module> SidebarModules { get; set; }
        public IScreen SelectedItem { get; set; }

        public void SetupSidebar()
        {
            SidebarItems.Clear();
            SidebarModules.Clear();

            // Add all default sidebar items
            SidebarItems.Add(new DividerNavigationItem());
            SidebarItems.Add(new FirstLevelNavigationItem {Icon = PackIconKind.Home, Label = "Home"});
            SidebarItems.Add(new FirstLevelNavigationItem {Icon = PackIconKind.Newspaper, Label = "News"});
            SidebarItems.Add(new FirstLevelNavigationItem {Icon = PackIconKind.TestTube, Label = "Workshop"});
            SidebarItems.Add(new FirstLevelNavigationItem {Icon = PackIconKind.Edit, Label = "Surface Editor"});
            SidebarItems.Add(new FirstLevelNavigationItem {Icon = PackIconKind.Settings, Label = "Settings"});

            // Add all activated modules
            SidebarItems.Add(new DividerNavigationItem());
            SidebarItems.Add(new SubheaderNavigationItem {Subheader = "Modules"});
            var modules = _pluginService.GetPluginsOfType<Core.Plugins.Abstract.Module>().ToList();
            foreach (var module in modules)
                AddModule(module);

            // Select the top item, which will be one of the defaults
            Task.Run(() => SelectSidebarItem(SidebarItems[1]));
        }

        private async Task SelectSidebarItem(INavigationItem sidebarItem)
        {
            if (SelectedItem != null)
            {
                var canClose = await SelectedItem.CanCloseAsync();
                if (!canClose)
                    return;
                SelectedItem.Close();
            }

            // A module was selected if the dictionary contains the selected item
            if (SidebarModules.ContainsKey(sidebarItem))
            {
                SelectedItem = _moduleVmFactory.Create(SidebarModules[sidebarItem]);
            }
            else
            {
                SelectedItem = null;
            }

            // TODO: Remove this bad boy, just testing
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        // ReSharper disable once UnusedMember.Global - Called by view
        public async Task SelectItem(WillSelectNavigationItemEventArgs args)
        {
            if (args.NavigationItemToSelect == null)
            {
                SelectedItem = null;
                return;
            }

            await SelectSidebarItem(args.NavigationItemToSelect);
        }

        public void AddModule(Core.Plugins.Abstract.Module module)
        {
            // Ensure the module is not already in the list
            if (SidebarModules.Any(io => io.Value == module))
                return;

            // Icon is provided as string to avoid having to reference MaterialDesignThemes
            var parsedIcon = Enum.TryParse<PackIconKind>(module.DisplayIcon, true, out var iconEnum);
            if (parsedIcon == false)
                iconEnum = PackIconKind.QuestionMarkCircle;
            var sidebarItem = new FirstLevelNavigationItem {Icon = iconEnum, Label = module.DisplayName};
            SidebarItems.Add(sidebarItem);
            SidebarModules.Add(sidebarItem, module);
        }

        public void RemoveModule(Core.Plugins.Abstract.Module module)
        {
            // If not in the list there's nothing to do
            if (SidebarModules.All(io => io.Value != module))
                return;

            var existing = SidebarModules.First(io => io.Value == module);
            SidebarItems.Remove(existing.Key);
            SidebarModules.Remove(existing.Key);
        }

        #region Event handlers

        private void PluginServiceOnPluginEnabled(object sender, PluginEventArgs e)
        {
            if (e.PluginInfo.Instance is Core.Plugins.Abstract.Module module)
                AddModule(module);
        }

        private void PluginServiceOnPluginDisabled(object sender, PluginEventArgs e)
        {
            if (e.PluginInfo.Instance is Core.Plugins.Abstract.Module module)
                RemoveModule(module);
        }

        #endregion
    }
}
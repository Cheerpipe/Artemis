﻿using Artemis.Core.Plugins.Modules;
using Artemis.Core.Plugins.Modules.ActivationRequirements;

namespace Artemis.Plugins.Modules.Overlay
{
    // The core of your module. Hover over the method names to see a description.
    public class OverlayModule : ProfileModule
    {
        // This is the beginning of your plugin life cycle. Use this instead of a constructor.
        public override void EnablePlugin()
        {
            DisplayName = "Overlay";
            DisplayIcon = "ArrangeBringToFront";
            DefaultPriorityCategory = ModulePriorityCategory.Overlay;

            ActivationRequirements.Add(new ProcessActivationRequirement("taskmgr"));
        }

        // This is the end of your plugin life cycle.
        public override void DisablePlugin()
        {
            // Make sure to clean up resources where needed (dispose IDisposables etc.)
        }

        public override void ModuleActivated(bool isOverride)
        {
            // When this gets called your activation requirements have been met and the module will start displaying
        }

        public override void ModuleDeactivated(bool isOverride)
        {
            // When this gets called your activation requirements are no longer met and your module will stop displaying
        }
    }
}
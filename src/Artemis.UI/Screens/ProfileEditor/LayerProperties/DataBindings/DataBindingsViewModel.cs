﻿using System.Linq;
using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings
{
    public class DataBindingsViewModel : PropertyChangedBase
    {
        private DataBindingsTabsViewModel _dataBindingsTabsViewModel;
        private DataBindingViewModel _dataBindingViewModel;

        public DataBindingsViewModel(BaseLayerProperty layerProperty)
        {
            LayerProperty = layerProperty;
            Initialise();
        }

        public BaseLayerProperty LayerProperty { get; }

        public DataBindingViewModel DataBindingViewModel
        {
            get => _dataBindingViewModel;
            set => SetAndNotify(ref _dataBindingViewModel, value);
        }

        public DataBindingsTabsViewModel DataBindingsTabsViewModel
        {
            get => _dataBindingsTabsViewModel;
            set => SetAndNotify(ref _dataBindingsTabsViewModel, value);
        }

        private void Initialise()
        {
            var properties = LayerProperty.GetDataBindingProperties();
            if (properties.Count == 0)
                return;

            if (properties.Count == 1)
                DataBindingViewModel = new DataBindingViewModel(LayerProperty, properties.First());
            else
            {
                DataBindingsTabsViewModel = new DataBindingsTabsViewModel();
                foreach (var dataBindingProperty in LayerProperty.GetDataBindingProperties())
                    DataBindingsTabsViewModel.Tabs.Add(new DataBindingViewModel(LayerProperty, dataBindingProperty));
            }
        }
    }
}
﻿using System;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Abstract;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Tree
{
    public class TreePropertyViewModel<T> : TreePropertyViewModel
    {
        private readonly IProfileEditorService _profileEditorService;
        private PropertyInputViewModel<T> _propertyInputViewModel;

        public TreePropertyViewModel(LayerPropertyBaseViewModel layerPropertyBaseViewModel, PropertyInputViewModel<T> propertyInputViewModel,
            IProfileEditorService profileEditorService) : base(layerPropertyBaseViewModel)
        {
            _profileEditorService = profileEditorService;
            LayerPropertyViewModel = (LayerPropertyViewModel<T>) layerPropertyBaseViewModel;
            PropertyInputViewModel = propertyInputViewModel;

            _profileEditorService.SelectedDataBindingChanged += ProfileEditorServiceOnSelectedDataBindingChanged;
        }

        public LayerPropertyViewModel<T> LayerPropertyViewModel { get; }

        public PropertyInputViewModel<T> PropertyInputViewModel
        {
            get => _propertyInputViewModel;
            set => SetAndNotify(ref _propertyInputViewModel, value);
        }

        public bool KeyframesEnabled
        {
            get => LayerPropertyViewModel.LayerProperty.KeyframesEnabled;
            set => ApplyKeyframesEnabled(value);
        }

        public bool DataBindingsOpen
        {
            get => _profileEditorService.SelectedDataBinding == LayerPropertyViewModel.BaseLayerProperty;
            set => _profileEditorService.ChangeSelectedDataBinding(value ? LayerPropertyViewModel.BaseLayerProperty : null);
        }

        public override void Dispose()
        {
            PropertyInputViewModel.Dispose();
        }

        private void ProfileEditorServiceOnSelectedDataBindingChanged(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(nameof(DataBindingsOpen));
        }

        private void ApplyKeyframesEnabled(bool enable)
        {
            // If enabling keyframes for the first time, add a keyframe with the current value at the current position
            if (enable && !LayerPropertyViewModel.LayerProperty.Keyframes.Any())
            {
                LayerPropertyViewModel.LayerProperty.AddKeyframe(new LayerPropertyKeyframe<T>(
                    LayerPropertyViewModel.LayerProperty.CurrentValue,
                    _profileEditorService.CurrentTime,
                    Easings.Functions.Linear,
                    LayerPropertyViewModel.LayerProperty
                ));
            }
            // If disabling keyframes, set the base value to the current value
            else if (!enable && LayerPropertyViewModel.LayerProperty.Keyframes.Any())
                LayerPropertyViewModel.LayerProperty.BaseValue = LayerPropertyViewModel.LayerProperty.CurrentValue;

            LayerPropertyViewModel.LayerProperty.KeyframesEnabled = enable;

            _profileEditorService.UpdateSelectedProfileElement();
        }
    }

    public abstract class TreePropertyViewModel : PropertyChangedBase, IDisposable
    {
        protected TreePropertyViewModel(LayerPropertyBaseViewModel layerPropertyBaseViewModel)
        {
            LayerPropertyBaseViewModel = layerPropertyBaseViewModel;
        }

        public LayerPropertyBaseViewModel LayerPropertyBaseViewModel { get; }

        public abstract void Dispose();
    }
}
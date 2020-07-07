﻿using System.Linq;
using System.Reflection;
using Artemis.Core.Plugins.Abstract.DataModels;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared.DataModelVisualization.Shared
{
    public class DataModelPropertyViewModel : DataModelVisualizationViewModel
    {
        private object _displayValue;
        private DataModelDisplayViewModel _displayViewModel;
        private bool _showNull;
        private bool _showToString;
        private bool _showViewModel;

        internal DataModelPropertyViewModel(DataModel dataModel, DataModelVisualizationViewModel parent, PropertyInfo propertyInfo) : base(dataModel, parent, propertyInfo)
        {
        }

        public object DisplayValue
        {
            get => _displayValue;
            set => SetAndNotify(ref _displayValue, value);
        }

        public DataModelDisplayViewModel DisplayViewModel
        {
            get => _displayViewModel;
            set => SetAndNotify(ref _displayViewModel, value);
        }

        public bool ShowToString
        {
            get => _showToString;
            set => SetAndNotify(ref _showToString, value);
        }

        public bool ShowNull
        {
            get => _showNull;
            set => SetAndNotify(ref _showNull, value);
        }

        public bool ShowViewModel
        {
            get => _showViewModel;
            set => SetAndNotify(ref _showViewModel, value);
        }

        public override void Update(IDataModelVisualizationService dataModelVisualizationService)
        {
            if (DisplayViewModel == null && dataModelVisualizationService.RegisteredDataModelDisplays.Any(d => d.SupportedType == PropertyInfo.PropertyType))
                dataModelVisualizationService.GetDataModelDisplayViewModel(PropertyInfo.PropertyType);

            DisplayValue = GetCurrentValue();
            UpdateDisplayParameters();
        }

        protected void UpdateDisplayParameters()
        {
            ShowToString = DisplayValue != null && DisplayViewModel == null;
            ShowNull = DisplayValue == null;
            ShowViewModel = DisplayValue != null && DisplayViewModel != null;

            DisplayViewModel?.UpdateValue(DisplayValue);
        }
    }
}
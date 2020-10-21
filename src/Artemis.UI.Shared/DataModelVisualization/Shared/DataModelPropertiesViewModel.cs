﻿using System;
using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared
{
    public class DataModelPropertiesViewModel : DataModelVisualizationViewModel
    {
        private Type _displayValueType;
        private object _displayValue;

        internal DataModelPropertiesViewModel(DataModel dataModel, DataModelVisualizationViewModel parent, DataModelPath dataModelPath) : base(dataModel, parent, dataModelPath)
        {
        }

        public Type DisplayValueType
        {
            get => _displayValueType;
            set => SetAndNotify(ref _displayValueType, value);
        }

        public object DisplayValue
        {
            get => _displayValue;
            set => SetAndNotify(ref _displayValue, value);
        }

        public override void Update(IDataModelUIService dataModelUIService)
        {
            DisplayValueType = DataModelPath?.GetPropertyType();

            // Only set a display value if ToString returns useful information and not just the type name
            object currentValue = GetCurrentValue();
            if (currentValue != null && currentValue.ToString() != currentValue.GetType().ToString())
                DisplayValue = currentValue.ToString();
            else
                DisplayValue = null;

            // Always populate properties
            PopulateProperties(dataModelUIService);

            // Only update children if the parent is expanded
            if (Parent != null && !Parent.IsVisualizationExpanded && !Parent.IsRootViewModel)
                return;

            foreach (DataModelVisualizationViewModel dataModelVisualizationViewModel in Children)
                dataModelVisualizationViewModel.Update(dataModelUIService);
        }

        public override object GetCurrentValue()
        {
            if (Parent == null)
                return null;
            return Parent.IsRootViewModel ? DataModel : base.GetCurrentValue();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return DisplayPath ?? Path;
        }

        internal override int GetChildDepth()
        {
            return PropertyDescription != null && !PropertyDescription.ResetsDepth ? Depth + 1 : 1;
        }
    }
}
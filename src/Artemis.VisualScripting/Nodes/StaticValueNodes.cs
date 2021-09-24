﻿using Artemis.Core;
using Artemis.VisualScripting.Nodes.CustomViewModels;

namespace Artemis.VisualScripting.Nodes
{
    [Node("Integer-Value", "Outputs an configurable integer value.", "Static", OutputType = typeof(int))]
    public class StaticIntegerValueNode : Node<int, StaticIntegerValueNodeCustomViewModel>
    {
        #region Properties & Fields

        public OutputPin<int> Output { get; }

        #endregion

        #region Constructors

        public StaticIntegerValueNode()
            : base("Integer", "Outputs an configurable integer value.")
        {
            Output = CreateOutputPin<int>();
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            Output.Value = Storage;
        }

        #endregion
    }

    [Node("Double-Value", "Outputs a configurable double value.", "Static", OutputType = typeof(double))]
    public class StaticDoubleValueNode : Node<double, StaticDoubleValueNodeCustomViewModel>
    {
        #region Properties & Fields

        public OutputPin<double> Output { get; }

        #endregion

        #region Constructors

        public StaticDoubleValueNode()
            : base("Double", "Outputs a configurable double value.")
        {
            Output = CreateOutputPin<double>();
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            Output.Value = Storage as double? ?? 0.0;
        }

        #endregion
    }

    [Node("Float-Value", "Outputs a configurable float value.", "Static", OutputType = typeof(float))]
    public class StaticFloatValueNode : Node<float, StaticFloatValueNodeCustomViewModel>
    {
        #region Properties & Fields

        public OutputPin<float> Output { get; }

        #endregion

        #region Constructors

        public StaticFloatValueNode()
            : base("Float", "Outputs a configurable float value.")
        {
            Output = CreateOutputPin<float>();
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            Output.Value = Storage;
        }

        #endregion
    }

    [Node("String-Value", "Outputs a configurable string value.", "Static", OutputType = typeof(string))]
    public class StaticStringValueNode : Node<string, StaticStringValueNodeCustomViewModel>
    {
        #region Properties & Fields

        public OutputPin<string> Output { get; }

        #endregion

        #region Constructors

        public StaticStringValueNode()
            : base("String", "Outputs a configurable string value.")
        {
            Output = CreateOutputPin<string>();
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            Output.Value = Storage;
        }
        
        #endregion
    }
}
﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Interaction logic for DraggableFloat.xaml
    /// </summary>
    public partial class DraggableFloat : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(float), typeof(DraggableFloat),
            new FrameworkPropertyMetadata(default(float), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, FloatPropertyChangedCallback));

        public static readonly DependencyProperty StepSizeProperty = DependencyProperty.Register(nameof(StepSize), typeof(float), typeof(DraggableFloat));

        public static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(Value),
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<float>),
                typeof(DraggableFloat));

        private bool _inCallback;
        private Point _mouseDragStartPoint;
        private float _startValue;

        public DraggableFloat()
        {
            InitializeComponent();
        }

        public float Value
        {
            get => (float) GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public float StepSize
        {
            get => (float) GetValue(StepSizeProperty);
            set => SetValue(StepSizeProperty, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static void FloatPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var draggableFloat = (DraggableFloat) d;
            if (draggableFloat._inCallback)
                return;

            draggableFloat._inCallback = true;
            draggableFloat.OnPropertyChanged(nameof(Value));
            draggableFloat._inCallback = false;
        }

        private void InputMouseDown(object sender, MouseButtonEventArgs e)
        {
            _startValue = Value;
            ((IInputElement) sender).CaptureMouse();
            _mouseDragStartPoint = e.GetPosition((IInputElement) sender);
        }

        private void InputMouseUp(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition((IInputElement) sender);
            if (position == _mouseDragStartPoint)
                DisplayInput();

            ((IInputElement) sender).ReleaseMouseCapture();
        }

        private void InputMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Use decimals for everything to avoid floating point errors
                var startValue = new decimal(_startValue);
                var startX = new decimal(_mouseDragStartPoint.X);
                var x = new decimal(e.GetPosition((IInputElement) sender).X);
                var stepSize = new decimal(StepSize);
                
                Value = (float) (Math.Round(startValue + stepSize * (x - startX) / stepSize) * stepSize);
            }
        }

        private void InputLostFocus(object sender, RoutedEventArgs e)
        {
            DisplayDragHandle();
        }

        private void InputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                DisplayDragHandle();
            else if (e.Key == Key.Escape)
            {
                Input.Text = _startValue.ToString();
                DisplayDragHandle();
            }
        }

        private void DisplayInput()
        {
            DragHandle.Visibility = Visibility.Collapsed;
            Input.Visibility = Visibility.Visible;
            Input.Focus();
            Input.SelectAll();
        }

        private void DisplayDragHandle()
        {
            Input.Visibility = Visibility.Collapsed;
            DragHandle.Visibility = Visibility.Visible;
        }

        private void Input_OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }
    }
}
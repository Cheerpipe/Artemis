﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Artemis.Core.Models.Surface;

namespace Artemis.UI.Shared.Controls
{
    public class DeviceVisualizer : FrameworkElement, IDisposable
    {
        public static readonly DependencyProperty DeviceProperty = DependencyProperty.Register(nameof(Device), typeof(ArtemisDevice), typeof(DeviceVisualizer),
            new FrameworkPropertyMetadata(default(ArtemisDevice), FrameworkPropertyMetadataOptions.AffectsRender, DevicePropertyChangedCallback));

        public static readonly DependencyProperty ShowColorsProperty = DependencyProperty.Register(nameof(ShowColors), typeof(bool), typeof(DeviceVisualizer),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender, ShowColorsPropertyChangedCallback));

        public static readonly DependencyProperty HighlightedLedsProperty = DependencyProperty.Register(nameof(HighlightedLeds), typeof(IEnumerable<ArtemisLed>), typeof(DeviceVisualizer),
            new FrameworkPropertyMetadata(default(IEnumerable<ArtemisLed>)));

        private readonly DrawingGroup _backingStore;
        private readonly List<DeviceVisualizerLed> _deviceVisualizerLeds;
        private readonly DispatcherTimer _timer;
        private BitmapImage _deviceImage;
        private ArtemisDevice _oldDevice;

        public DeviceVisualizer()
        {
            _backingStore = new DrawingGroup();
            _deviceVisualizerLeds = new List<DeviceVisualizerLed>();

            // Run an update timer at 25 fps
            _timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(40)};
            _timer.Tick += TimerOnTick;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        public ArtemisDevice Device
        {
            get => (ArtemisDevice) GetValue(DeviceProperty);
            set => SetValue(DeviceProperty, value);
        }

        public bool ShowColors
        {
            get => (bool) GetValue(ShowColorsProperty);
            set => SetValue(ShowColorsProperty, value);
        }

        public IEnumerable<ArtemisLed> HighlightedLeds
        {
            get => (IEnumerable<ArtemisLed>) GetValue(HighlightedLedsProperty);
            set => SetValue(HighlightedLedsProperty, value);
        }

        public void Dispose()
        {
            _timer.Stop();
        }

        public static Size ResizeKeepAspect(Size src, double maxWidth, double maxHeight)
        {
            double scale;
            if (maxWidth == double.PositiveInfinity && maxHeight != double.PositiveInfinity)
                scale = maxHeight / src.Height;
            else if (maxWidth != double.PositiveInfinity && maxHeight == double.PositiveInfinity)
                scale = maxWidth / src.Width;
            else if (maxWidth == double.PositiveInfinity && maxHeight == double.PositiveInfinity)
                return src;

            scale = Math.Min(maxWidth / src.Width, maxHeight / src.Height);

            return new Size(src.Width * scale, src.Height * scale);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Device == null)
                return;

            // Determine the scale required to fit the desired size of the control
            var measureSize = MeasureDevice();
            var scale = Math.Min(DesiredSize.Width / measureSize.Width, DesiredSize.Height / measureSize.Height);
            var scaledRect = new Rect(0, 0, measureSize.Width * scale, measureSize.Height * scale);

            // Center and scale the visualization in the desired bounding box
            if (DesiredSize.Width > 0 && DesiredSize.Height > 0)
            {
                drawingContext.PushTransform(new TranslateTransform(DesiredSize.Width / 2 - scaledRect.Width / 2, DesiredSize.Height / 2 - scaledRect.Height / 2));
                drawingContext.PushTransform(new ScaleTransform(scale, scale));
            }

            // Determine the offset required to rotate within bounds
            var rotationRect = new Rect(0, 0, Device.RgbDevice.ActualSize.Width, Device.RgbDevice.ActualSize.Height);
            rotationRect.Transform(new RotateTransform(Device.Rotation).Value);

            // Apply device rotation
            drawingContext.PushTransform(new TranslateTransform(0 - rotationRect.Left, 0 - rotationRect.Top));
            drawingContext.PushTransform(new RotateTransform(Device.Rotation));

            // Apply device scale
            drawingContext.PushTransform(new ScaleTransform(Device.Scale, Device.Scale));

            // Render device and LED images 
            if (_deviceImage != null)
                drawingContext.DrawImage(_deviceImage, new Rect(0, 0, Device.RgbDevice.Size.Width, Device.RgbDevice.Size.Height));

            foreach (var deviceVisualizerLed in _deviceVisualizerLeds)
                deviceVisualizerLed.RenderImage(drawingContext);

            drawingContext.DrawDrawing(_backingStore);
        }

        private Size MeasureDevice()
        {
            if (Device == null)
                return Size.Empty;

            var rotationRect = new Rect(0, 0, Device.RgbDevice.ActualSize.Width, Device.RgbDevice.ActualSize.Height);
            rotationRect.Transform(new RotateTransform(Device.Rotation).Value);

            return rotationRect.Size;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (Device == null)
                return Size.Empty;

            var deviceSize = MeasureDevice();
            return ResizeKeepAspect(deviceSize, availableSize.Width, availableSize.Height);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _timer.Stop();

            if (_oldDevice != null)
            {
                Device.RgbDevice.PropertyChanged -= DevicePropertyChanged;
                _oldDevice = null;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _timer.Start();
        }

        private void TimerOnTick(object sender, EventArgs e)
        {
            if (ShowColors && Visibility == Visibility.Visible)
                Render();
        }

        private void UpdateTransform()
        {
            InvalidateVisual();
            InvalidateMeasure();
        }

        private static void DevicePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var deviceVisualizer = (DeviceVisualizer) d;
            deviceVisualizer.Dispatcher.Invoke(() => { deviceVisualizer.SetupForDevice(); });
        }

        private static void ShowColorsPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var deviceVisualizer = (DeviceVisualizer) d;
            deviceVisualizer.Dispatcher.Invoke(() => { deviceVisualizer.SetupForDevice(); });
        }

        private void SetupForDevice()
        {
            _deviceImage = null;
            _deviceVisualizerLeds.Clear();

            if (Device == null)
                return;

            if (_oldDevice != null)
                Device.RgbDevice.PropertyChanged -= DevicePropertyChanged;
            _oldDevice = Device;

            Device.RgbDevice.PropertyChanged += DevicePropertyChanged;
            UpdateTransform();

            // Load the device main image
            if (Device.RgbDevice?.DeviceInfo?.Image?.AbsolutePath != null && File.Exists(Device.RgbDevice.DeviceInfo.Image.AbsolutePath))
                _deviceImage = new BitmapImage(Device.RgbDevice.DeviceInfo.Image);

            // Create all the LEDs
            foreach (var artemisLed in Device.Leds)
                _deviceVisualizerLeds.Add(new DeviceVisualizerLed(artemisLed));

            if (!ShowColors)
            {
                InvalidateMeasure();
                return;
            }

            // Create the opacity drawing group
            var opacityDrawingGroup = new DrawingGroup();
            var drawingContext = opacityDrawingGroup.Open();
            foreach (var deviceVisualizerLed in _deviceVisualizerLeds)
                deviceVisualizerLed.RenderOpacityMask(drawingContext);
            drawingContext.Close();

            // Render the store as a bitmap 
            var drawingImage = new DrawingImage(opacityDrawingGroup);
            var image = new Image {Source = drawingImage};
            var bitmap = new RenderTargetBitmap(
                Math.Max(1, (int) (opacityDrawingGroup.Bounds.Width * 2.5)),
                Math.Max(1, (int) (opacityDrawingGroup.Bounds.Height * 2.5)),
                96,
                96,
                PixelFormats.Pbgra32
            );
            image.Arrange(new Rect(0, 0, bitmap.Width, bitmap.Height));
            bitmap.Render(image);
            bitmap.Freeze();

            // Set the bitmap as the opacity mask for the colors backing store
            var bitmapBrush = new ImageBrush(bitmap);
            bitmapBrush.Freeze();
            _backingStore.OpacityMask = bitmapBrush;

            InvalidateMeasure();
        }

        private void DevicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Device.RgbDevice.Scale) || e.PropertyName == nameof(Device.RgbDevice.Rotation))
                UpdateTransform();
        }


        private void Render()
        {
            var drawingContext = _backingStore.Open();

            if (HighlightedLeds != null && HighlightedLeds.Any())
            {
                foreach (var deviceVisualizerLed in _deviceVisualizerLeds)
                    deviceVisualizerLed.RenderColor(drawingContext, !HighlightedLeds.Contains(deviceVisualizerLed.Led));
            }
            else
            {
                foreach (var deviceVisualizerLed in _deviceVisualizerLeds)
                    deviceVisualizerLed.RenderColor(drawingContext, false);
            }

            drawingContext.Close();
        }
    }
}
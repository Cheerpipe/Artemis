﻿using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Artemis.Utilities;
using CUE.NET;
using CUE.NET.Devices.Generic.Enums;
using CUE.NET.Exceptions;
using Ninject.Extensions.Logging;

namespace Artemis.DeviceProviders.Corsair
{
    internal class CorsairHeadsets : DeviceProvider
    {
        public CorsairHeadsets(ILogger logger)
        {
            Logger = logger;
            Type = DeviceType.Headset;
        }

        public ILogger Logger { get; set; }

        public override bool TryEnable()
        {
            CanUse = CanInitializeSdk();
            if (CanUse)
                CueSDK.HeadsetSDK.UpdateMode = UpdateMode.Manual;

            Logger.Debug("Attempted to enable Corsair headset. CanUse: {0}", CanUse);
            return CanUse;
        }

        public override void Disable()
        {
            if (CueSDK.ProtocolDetails != null)
                CueSDK.Reinitialize();
        }

        public override void UpdateDevice(Brush brush)
        {
            if (!CanUse || brush == null)
                return;

            var leds = CueSDK.HeadsetSDK.Leds.Count();
            var rect = new Rect(new Size(leds*20, leds*20));
            var img = brush.Dispatcher.Invoke(() =>
            {
                var visual = new DrawingVisual();
                using (var c = visual.RenderOpen())
                    c.DrawRectangle(brush, null, rect);
                return ImageUtilities.DrawinVisualToBitmap(visual, rect);
            });

            var ledIndex = 0;
            // Color each LED according to one of the pixels
            foreach (var corsairLed in CueSDK.HeadsetSDK.Leds)
            {
                corsairLed.Color = ledIndex == 0
                    ? img.GetPixel(0, 0)
                    : img.GetPixel((ledIndex + 1)*20 - 1, (ledIndex + 1)*20 - 1);
                ledIndex++;
            }
            CueSDK.HeadsetSDK.Update(true);
        }

        private static bool CanInitializeSdk()
        {
            // Try for about 10 seconds, in case CUE isn't started yet
            var tries = 0;
            while (tries < 9)
            {
                try
                {
                    if (CueSDK.ProtocolDetails == null)
                        CueSDK.Initialize();
                    else
                        return true;
                }
                catch (CUEException e)
                {
                    if (e.Error != CorsairError.ServerNotFound)
                        return true;

                    tries++;
                    Thread.Sleep(1000);
                    continue;
                }
                catch (WrapperException)
                {
                    CueSDK.Reinitialize();
                    return true;
                }

                return true;
            }

            return false;
        }
    }
}
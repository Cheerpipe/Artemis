﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.Core.Services;
using SkiaSharp;

namespace Artemis.Plugins.Modules.General.DataModels.Windows
{
    public class WindowDataModel
    {
        public WindowDataModel(Process process, IColorQuantizerService _quantizerService)
        {
            Process = process;
            WindowTitle = process.MainWindowTitle;
            ProcessName = process.ProcessName;

            // Accessing MainModule requires admin privileges, this way does not
            ProgramLocation = process.GetProcessFilename();

            // Get Icon colors
            if(File.Exists(ProgramLocation)) 
            {
                using MemoryStream mem = new MemoryStream();
                Icon.ExtractAssociatedIcon(ProgramLocation).Save(mem);
                mem.Seek(0, SeekOrigin.Begin);
                using SKBitmap skbm = SKBitmap.Decode(mem);
                mem.Close();

                List<SKColor> skClrs = _quantizerService.Quantize(skbm.Pixels.ToList(), 256).ToList();
                Colors = new IconColorsDataModel 
                {
                    Vibrant = _quantizerService.FindColorVariation(skClrs, ColorType.Vibrant, true),
                    LightVibrant = _quantizerService.FindColorVariation(skClrs, ColorType.LightVibrant, true),
                    DarkVibrant = _quantizerService.FindColorVariation(skClrs, ColorType.DarkVibrant, true),
                    Muted = _quantizerService.FindColorVariation(skClrs, ColorType.Muted, true),
                    LightMuted = _quantizerService.FindColorVariation(skClrs, ColorType.LightMuted, true),
                    DarkMuted = _quantizerService.FindColorVariation(skClrs, ColorType.DarkMuted, true),
                };
            }
        }

        [DataModelIgnore]
        public Process Process { get; }

        public string WindowTitle { get; set; }
        public string ProcessName { get; set; }
        public string ProgramLocation { get; set; }
        public IconColorsDataModel Colors { get; set; }
    }
}
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BisTranslator.Windows
{
    public class Widget : Window, IDisposable
    {
        private Configuration _config;
        private IPluginLog _log;

        private float progress = 0f;

        public Widget(Configuration configuration, IPluginLog log) : base(
        "BisTranslatorWidget", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDecoration 
            | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoBringToFrontOnFocus 
            | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoMouseInputs 
            | ImGuiWindowFlags.NoMove)
        {
            this.Size = new Vector2(500, 200);
            this.SizeCondition = ImGuiCond.Always;
            this.PositionCondition = ImGuiCond.Always;
            this.Position = new Vector2(0, 100);
            this.AllowClickthrough = true;

            _log = log;
            _config = configuration;
        }

        public void Dispose() { }

        public override void Draw()
        {
            if (_config.GilOverflow)
            {
                ImGui.TextColored(new Vector4(1.0f, 0f, 0f, 1.0f), "Gil Overflow");
            }
            //ImGui.ProgressBar(progress, new Vector2(500, 30), "Fancy Progress Bar");
            //progress += 0.01f;
            //if (progress > 1) { progress = 0; }
        }
    }
}

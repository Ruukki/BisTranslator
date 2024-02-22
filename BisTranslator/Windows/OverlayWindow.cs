using Dalamud.Interface.Internal;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
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
    public class OverlayWindow : Window, IDisposable
    {
        private Configuration _config;
        private IPluginLog _log;
        DalamudPluginInterface _pluginInterface;

        private IDalamudTextureWrap? _textureWrap;

        private float progress = 0f;

        public unsafe OverlayWindow(Configuration configuration, IPluginLog log, DalamudPluginInterface pluginInterface) : base(
        "OverlayWindow", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDecoration
            | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoBringToFrontOnFocus
            | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoMouseInputs
            | ImGuiWindowFlags.NoMove)
        {
            this.Size = new Vector2(Device.Instance()->Width, Device.Instance()->Height);
            this.SizeCondition = ImGuiCond.Always;
            this.PositionCondition = ImGuiCond.Always;
            this.Position = new Vector2(0, 0);
            this.AllowClickthrough = true;

            _log = log;
            _config = configuration;
            _pluginInterface = pluginInterface;

            var imagePath = Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, "oip.gif");
            _textureWrap = this._pluginInterface.UiBuilder.LoadImage(imagePath);
        }

        public void Dispose() { }

        public override void Draw()
        {
            if (_textureWrap != null)
            {
                //ImGui.Image(_textureWrap.ImGuiHandle, new Vector2(_textureWrap.Width, _textureWrap.Height));
            }
            //ImGui.Image()
            //ImGui.ProgressBar(progress, new Vector2(500, 30), "Fancy Progress Bar");
            //progress += 0.01f;
            //if (progress > 1) { progress = 0; }
        }
    }
}

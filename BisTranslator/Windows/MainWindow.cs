using System;
using System.Numerics;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace BisTranslator.Windows;

public class MainWindow : Window, IDisposable
{
    private IDalamudTextureWrap GoatImage;
    private Plugin Plugin;
    private Configuration _config;

    public MainWindow(Plugin plugin, IDalamudTextureWrap goatImage) : base(
        "My Amazing Window", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.GoatImage = goatImage;
        this.Plugin = plugin;
        _config = new Configuration();
    }

    public void Dispose()
    {
        this.GoatImage.Dispose();
    }

    public override void Draw()
    {
        if (ImGui.Button("Save"))
        {
            this.Plugin.Configuration.Save();
        }
        if (ImGui.Button("Show Settings"))
        {
            this.Plugin.DrawConfigUI();
        }

        ImGui.Spacing();

        string name = _config.Name;
        if (ImGui.InputText("new name", ref name, 50))
        {
            _config.Name = name;
        }

        string regex = _config.CommandRegex;
        if(ImGui.InputText("Command match regex", ref regex, 50))
        {
            _config.CommandRegex = regex;
        }

        

        //ImGui.Text("Have a goat:");
        //ImGui.Indent(55);
        //ImGui.Image(this.GoatImage.ImGuiHandle, new Vector2(this.GoatImage.Width, this.GoatImage.Height));
        //ImGui.Unindent(55);
    }
}

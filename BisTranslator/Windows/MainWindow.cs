using System;
using System.Numerics;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace BisTranslator.Windows;

public class MainWindow : Window, IDisposable
{
    private Configuration _config;
    private ConfigWindow _configWindow;

    public MainWindow(ConfigWindow configWindow, Configuration config) : base(
        "Miki Mod Workshop", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        _config = config;
        _configWindow = configWindow;
    }

    public void Dispose()
    {
       // this.GoatImage.Dispose();
    }

    public override void Draw()
    {
        if (ImGui.Button("Save"))
        {
            _config.Save();
        }
        ImGui.SameLine();
        if (ImGui.Button("Show Settings"))
        {
            _config.Save();
            _configWindow.Toggle();
        }

        ImGui.Spacing();

        string name = _config.Name;
        if (ImGui.InputText("new name", ref name, 50))
        {
            _config.Name = name.ToLowerInvariant();
        }

        ImGui.Text($"Command match: {_config.CommandMatch}");
        ImGui.Text($"Command regex: {_config.CommandRegex}");



        //ImGui.Text("Have a goat:");
        //ImGui.Indent(55);
        //ImGui.Image(this.GoatImage.ImGuiHandle, new Vector2(this.GoatImage.Width, this.GoatImage.Height));
        //ImGui.Unindent(55);
    }
}

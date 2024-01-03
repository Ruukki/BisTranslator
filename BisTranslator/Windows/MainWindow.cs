using System;
using System.Linq;
using System.Numerics;
using BisTranslator.Permissions;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.Havok;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace BisTranslator.Windows;

public class MainWindow : Window, IDisposable
{
    private Configuration _config;
    private ConfigWindow _configWindow;
    private AbilitiesWindow _abilitiesWindow;

    public MainWindow(ConfigWindow configWindow, Configuration config, AbilitiesWindow abilitiesWindow) : base(
        "Miki Mod Workshop", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        _config = config;
        _configWindow = configWindow;
        _abilitiesWindow = abilitiesWindow;
    }

    public void Dispose()
    {
       // this.GoatImage.Dispose();
    }

    public override void Draw()
    {
        if (_config.lockedUiOverride)
        {
            ImGui.TextColored(new Vector4(1.0f, 0f, 0f, 1.0f), "Hell mode");
        }
        else
        {
            if (ImGui.Button("Save"))
            {
                _config.Save();
            }
            ImGui.SameLine();
        }

        if (ImGui.Button("Show Settings"))
        {
            _configWindow.Toggle();
        }

        ImGui.Spacing();

        string name = _config.Name;
        if (_config.lockedUiOverride)
        {
            ImGui.Text($"your new name: {name}");
        }
        else
        {
            if (ImGui.InputText("new name", ref name, 50))
            {
                _config.Name = name.ToLowerInvariant();
            }
        }       


        ImGui.Text($"Command match: {_config.CommandMatch}");
        ImGui.Text($"Command regex: {_config.CommandRegex}");
        /*ImGui.BeginListBox("Testing");        
        ImGui.EndListBox();*/

        if (_config.tester)
        {
            if (ImGui.Button("Abilities"))
            {
                _abilitiesWindow.Toggle();
            }
            var lvl = (int)_config.AbilityRestrictionLevel;
            var names = Enum.GetValues(typeof(AbilityRestrictionLevel))
                                         .Cast<AbilityRestrictionLevel>()
                                         .Select(e => e.ToString())
                                         .ToArray();
            if (ImGui.ListBox("Testing", ref lvl, names, names.Length))
            {
                _config.AbilityRestrictionLevel = (AbilityRestrictionLevel)lvl;
            }
            ImGui.Text(_config.AbilityRestrictionLevel.ToString());
        }


        //ImGui.Text("Have a goat:");
        //ImGui.Indent(55);
        //ImGui.Image(this.GoatImage.ImGuiHandle, new Vector2(this.GoatImage.Width, this.GoatImage.Height));
        //ImGui.Unindent(55);
    }
}

using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace BisTranslator.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    public ConfigWindow(Plugin plugin) : base(
        "Translations list",
        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.Size = new Vector2(300, 300);
        this.SizeCondition = ImGuiCond.Always;

        this.Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var test = "x123";
        var translations = this.Configuration.Translations;
        ImGui.BeginTable("tr", 2, ImGuiTableFlags.Reorderable);
        //ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 36);
        ImGui.TableSetupColumn("Find");
        ImGui.TableSetupColumn("Replace");
        ImGui.TableHeadersRow();
        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        foreach (var item in translations)
        {
            ImGui.Text(item.Key);
            ImGui.TableNextColumn();
            ImGui.Text(item.Value);
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
        }
        
        ImGui.EndTable();
    }
}

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Transactions;
using BisTranslator.Translator;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using ImGuiNET;

namespace BisTranslator.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration _config;
    private IPluginLog _log;

    public ConfigWindow(Configuration configuration, IPluginLog log) : base(
        "Translations list",
        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.Size = new Vector2(300, 400);
        this.SizeCondition = ImGuiCond.Always;

        _log = log;
        _config = configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var pussy = _config.BigPussy;
        ImGui.Checkbox("I am a big pussy", ref pussy);
        _config.BigPussy = pussy;
        if (!_config.BigPussy) {
            ImGui.BeginTable("tr", 2, ImGuiTableFlags.Reorderable);
            ImGui.TableSetupColumn("Find");
            ImGui.TableSetupColumn("Replace");
            ImGui.TableHeadersRow();
            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            if (_config != null && !_config.Name.Trim().IsNullOrEmpty())
            {
                foreach (var item in Translations.Named)
                {
                    ImGui.Text(item.Key);
                    ImGui.TableNextColumn();
                    ImGui.Text(item.Value);
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                }
            }

            ImGui.EndTable();
        }
        else
        {
            ImGui.Text("WHY SO WEAK?");
            ImGui.Text("WHY SO WEAK?");
            ImGui.Text("WHY SO WEAK?");
            ImGui.Text("WHY SO WEAK?");
            ImGui.Text("WHY SO WEAK?");
            ImGui.Text("WHY SO WEAK?");
            ImGui.Text("WHY SO WEAK?");
            ImGui.Text("WHY SO WEAK?");
        }
    }
}

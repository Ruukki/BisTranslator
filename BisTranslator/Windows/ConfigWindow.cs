using System;
using System.Collections.Generic;
using System.Numerics;
using System.Transactions;
using BisTranslator.Permissions;
using BisTranslator.Services;
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
    private PlugService _plugService;

    public ConfigWindow(Configuration configuration, IPluginLog log, PlugService plugService) : base(
        "Translations list",
        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.Size = new Vector2(300, 400);
        this.SizeCondition = ImGuiCond.Always;

        _log = log;
        _config = configuration;
        _plugService = plugService;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var secretFeature = _config.SuperSecretFeature;
        ImGui.Checkbox("Super secret feature", ref secretFeature);
        _config.SuperSecretFeature = secretFeature;
        if (_config.SuperSecretFeature)
        {           
            if (!_plugService.Connected)
            {
                ImGui.SameLine();
                if (ImGui.Button("Connect"))
                {
                    _plugService?.ConnectAsync();
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Disconnect"))
            {
                _plugService?.DisconnectAsync();
            }
        }

        var pussy = _config.BigPussy;
        if (!_config.lockedUiOverride)
        {
            ImGui.Checkbox("I am a big pussy", ref pussy);
            _config.BigPussy = pussy;

            var gilCheck = _config.GilCheck;
            ImGui.Checkbox("GilCheck", ref gilCheck);
            _config.GilCheck = gilCheck;

            /*var fullMatch = _config.UseFullMatch;
            ImGui.Checkbox("UseFullMatch", ref fullMatch);
            _config.UseFullMatch = fullMatch;*/
        }
        if (!_config.BigPussy) {
            ImGui.BeginTable("tr", 2, ImGuiTableFlags.Reorderable);
            ImGui.TableSetupColumn("Find");
            ImGui.TableSetupColumn("Replace");
            ImGui.TableHeadersRow();
            ImGui.TableNextRow();
            ImGui.TableNextColumn();


            foreach (var item in Translations.Named)
            {
                ImGui.Text(item.Key);
                ImGui.TableNextColumn();
                ImGui.Text(item.Value);
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
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

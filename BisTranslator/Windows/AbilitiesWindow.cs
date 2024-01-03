using BisTranslator.Services;
using BisTranslator.Translator;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BisTranslator.Windows
{
    public class AbilitiesWindow : Window, IDisposable
    {
        private Configuration _config;
        private IPluginLog _log;

        private int abilitiesListbox = 0;

        public AbilitiesWindow(Configuration configuration, IPluginLog log) : base(
        "Actions", ImGuiWindowFlags.None)
        {
            this.Size = new Vector2(300, 400);
            this.SizeCondition = ImGuiCond.Always;

            _log = log;
            _config = configuration;
        }

        public void Dispose() { }

        public override void Draw()
        {
            int current = abilitiesListbox-1;
            var names = Enum.GetValues(typeof(AbilityRestrictionLevel)).Cast<AbilityRestrictionLevel>().Where(x => x != AbilityRestrictionLevel.None).Select(x => x.ToString()).ToArray();
            ImGui.ListBox("", ref current, names, names.Length);
            abilitiesListbox = current+1;

            switch ((AbilityRestrictionLevel)abilitiesListbox)
            {
                case AbilityRestrictionLevel.Hardcore:
                    ImGui.Text("Hardcore");
                    ImGui.BeginTable("tr", 2, ImGuiTableFlags.Reorderable);
                    ImGui.TableSetupColumn("ID");
                    ImGui.TableSetupColumn("NAME");
                    ImGui.TableHeadersRow();
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    foreach (var item in Abilities.minimalHealer)
                    {
                        ImGui.Text(item.Key.ToString());
                        ImGui.TableNextColumn();
                        ImGui.Text(item.Value);
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                    }
                    ImGui.EndTable();
                    break;
                case AbilityRestrictionLevel.Minimal:
                    ImGui.Text("Minimal");
                    ImGui.BeginTable("tr", 2, ImGuiTableFlags.Reorderable);
                    ImGui.TableSetupColumn("ID");
                    ImGui.TableSetupColumn("NAME");
                    ImGui.TableHeadersRow();
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    foreach (var item in Abilities.minimalHealer)
                    {
                        ImGui.Text(item.Key.ToString());
                        ImGui.TableNextColumn();
                        ImGui.Text(item.Value);
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                    }
                    foreach (var item in Abilities.minimalHealerDps)
                    {
                        ImGui.Text(item.Key.ToString());
                        ImGui.TableNextColumn();
                        ImGui.Text(item.Value);
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                    }
                    ImGui.EndTable();
                    break;
                case AbilityRestrictionLevel.Advanced:
                    ImGui.Text("Advanced");
                    ImGui.BeginTable("tr", 2, ImGuiTableFlags.Reorderable);
                    ImGui.TableSetupColumn("ID");
                    ImGui.TableSetupColumn("NAME");
                    ImGui.TableHeadersRow();
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    foreach (var item in Abilities.advancedHealer)
                    {
                        ImGui.Text(item.Key.ToString());
                        ImGui.TableNextColumn();
                        ImGui.Text(item.Value);
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                    }
                    foreach (var item in Abilities.advancedHealerDps)
                    {
                        ImGui.Text(item.Key.ToString());
                        ImGui.TableNextColumn();
                        ImGui.Text(item.Value);
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                    }
                    ImGui.EndTable();
                    break;
                case AbilityRestrictionLevel.Spec:
                    ImGui.Text("Spec");
                    ImGui.BeginTable("tr", 2, ImGuiTableFlags.Reorderable);
                    ImGui.TableSetupColumn("ID");
                    ImGui.TableSetupColumn("NAME");
                    ImGui.TableHeadersRow();
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    foreach (var item in Abilities.specHealer)
                    {
                        ImGui.Text(item.Key.ToString());
                        ImGui.TableNextColumn();
                        ImGui.Text(item.Value);
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                    }
                    ImGui.EndTable();
                    break;
            }            
        }
    }
}


using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using Dalamud.Memory;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Moodles.Data;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.Enums;
using Lumina.Excel.Sheets;
using Dalamud.Game.Text.SeStringHandling;
using ECommons;
using Dalamud.Game.Gui.NamePlate;
using System.Diagnostics;
using static BisTranslator.Services.Actions.ActionManager;

namespace BisTranslator.Services
{
    internal unsafe class NameChanger : IDisposable
    {
        [Signature("40 53 56 41 56 41 57 48 81 EC ?? ?? ?? ?? 48 8B 84 24", DetourName = nameof(UpdateNameplateDetour))]
        private Hook<UpdateNameplateDelegate>? updateNameplateHook;

        private delegate void* UpdateNameplateDelegate(RaptureAtkModule* raptureAtkModule, RaptureAtkModule.NamePlateInfo* namePlateInfo, NumberArrayData* numArray, StringArrayData* stringArray, BattleChara* battleChara, int numArrayIndex, int stringArrayIndex);

        private readonly IPluginLog _log;
        private readonly ICondition _condition;
        private readonly IClientState _clientState;
        private readonly IObjectTable _objectTable;
        private readonly IGameInteropProvider _gameInteropProvider;

        public NameChanger(IPluginLog log, ICondition condition, IClientState clientState, IObjectTable objectTable, IGameInteropProvider gameInteropProvider)
        {
            _log = log;
            _condition = condition;
            _clientState = clientState;
            _objectTable = objectTable;
            _gameInteropProvider = gameInteropProvider;

            _gameInteropProvider.InitializeFromAttributes(this);

            updateNameplateHook?.Enable();
            _log.Debug($"[NameChanger] updateNameplateHook {updateNameplateHook == null}");
            
        }

        public void Dispose()
        {
            updateNameplateHook?.Disable();
            updateNameplateHook?.Dispose();
            updateNameplateHook = null;
        }

        public void* UpdateNameplateDetour(RaptureAtkModule* raptureAtkModule, RaptureAtkModule.NamePlateInfo* namePlateInfo, NumberArrayData* numArray, StringArrayData* stringArray, BattleChara* battleChara, int numArrayIndex, int stringArrayIndex)
        {
            //_log.Debug($"[NameChanger] UpdateNameplateDetour");
            try
            {
                CleanupNamePlate(namePlateInfo);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error in Cleanup of BattleChara Nameplate");
            }
            var r = updateNameplateHook!.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, battleChara, numArrayIndex, stringArrayIndex);
            try
            {
                if (_clientState.IsPvPExcludingDen) return r;
                if (_condition[ConditionFlag.BetweenAreas] ||
                    _condition[ConditionFlag.BetweenAreas51] ||
                    _condition[ConditionFlag.LoggingOut] ||
                    _condition[ConditionFlag.OccupiedInCutSceneEvent] ||
                    _condition[ConditionFlag.WatchingCutscene] ||
                    _condition[ConditionFlag.WatchingCutscene78]) return r;

                var gameObject = &battleChara->Character.GameObject;
                if (gameObject->ObjectKind == FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind.Pc && gameObject->SubKind == 4)
                {
                    //_log.Debug($"{namePlateInfo->Name.GetText()}");
                    if (namePlateInfo->Name.GetText().StartsWith("Suna Furukane"))
                    {
                        AfterNameplateUpdate(namePlateInfo, battleChara);
                    }
                    else
                    {
                        return r;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error in AfterNameplateUpdate");
            }

            return r;
        }

        private void CleanupNamePlate(RaptureAtkModule.NamePlateInfo* namePlateInfo, bool force = false)
        {
            /*
            if (ModifiedNamePlates.TryGetValue((ulong)namePlateInfo, out var owner) && (force || owner != namePlateInfo->ObjectId.ObjectId))
            {
                using var _ = PerformanceMonitors.Run("Cleanup");
                PluginService.Log.Verbose($"Cleanup NamePlate: {MemoryHelper.ReadSeString(&namePlateInfo->Name).TextValue}");
                var title = MemoryHelper.ReadSeString(&namePlateInfo->Title);
                if (title.TextValue.Length > 0)
                {
                    title.Payloads.Insert(0, new TextPayload("《"));
                    title.Payloads.Add(new TextPayload("》"));
                }
                namePlateInfo->DisplayTitle.SetString(title.EncodeNullTerminated());
                namePlateInfo->IsDirty = true;
                ModifiedNamePlates.Remove((ulong)namePlateInfo);
            }*/
        }

        public void AfterNameplateUpdate(RaptureAtkModule.NamePlateInfo* namePlateInfo, BattleChara* battleChara)
        {
            if (namePlateInfo->ObjectId.ObjectId == 0) return;
            var gameObject = &battleChara->Character.GameObject;
            if (gameObject->ObjectKind != FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind.Pc || gameObject->SubKind != 4) return;
            var player = _objectTable.CreateObjectReference((nint)gameObject) as IPlayerCharacter;
            if (player == null) return;

            var titleChanged = false;



            var currentDisplayTitle = MemoryHelper.ReadSeString(&namePlateInfo->DisplayTitle);

            var newNameBuilder = new SeStringBuilder();
            newNameBuilder.AddText("Test Name");
            var newName = newNameBuilder.Build();

            //_log.Debug($"[NameChanger] Original:{namePlateInfo->Name.GetText()}");
            namePlateInfo->Name.SetString("Miki's Cow");
            //_log.Debug($"[NameChanger] New:{namePlateInfo->Name.GetText()}");

            titleChanged = true;

        }

    }
}

using BisTranslator.Permissions;
using BisTranslator.Services.Chat;
using ChatTwo.Movement;
using Dalamud.Game;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.Interop;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BisTranslator.Services.Actions
{
    public unsafe class ActionManager : IDisposable
    {
        private readonly Configuration _config;
        private readonly IClientState _clientState;
        private readonly IPluginLog _log;
        private readonly IFramework _framework;
        private readonly IKeyState _keyState;
        private readonly IGameInteropProvider _gameInteropProvider;
        private readonly ICondition _condition;
        private readonly IObjectTable _objectTable;

        private FFXIVClientStructs.FFXIV.Client.Game.Control.Control* gameControl = FFXIVClientStructs.FFXIV.Client.Game.Control.Control.Instance();

        delegate ref int GetRefValue(int vkCode);
        static GetRefValue getRefValue;

        internal delegate bool UseActionDelegate(FFXIVClientStructs.FFXIV.Client.Game.ActionManager* am, ActionType type, uint acId, long target, uint a5, uint a6, uint a7, void* a8);
        internal Hook<UseActionDelegate> UseActionHook;

        public unsafe ActionManager(Configuration config, IClientState clientState, IPluginLog log, IFramework framework, IKeyState keyState, IGameInteropProvider interop, ICondition condition, IObjectTable gameObjects)
        {
            _config = config;
            _clientState = clientState;
            _log = log;
            _framework = framework;
            _keyState = keyState;
            _gameInteropProvider = interop;


            _framework.Update += framework_Update;

            UseActionHook = _gameInteropProvider.HookFromAddress<UseActionDelegate>((nint)FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Addresses.UseAction.Value, UseActionDetour);
            UseActionHook.Enable();

            getRefValue = (GetRefValue)Delegate.CreateDelegate(typeof(GetRefValue), _keyState,
                        _keyState.GetType().GetMethod("GetRefValue",
                        BindingFlags.NonPublic | BindingFlags.Instance,
                        null, new Type[] { typeof(int) }, null));
            _condition = condition;
            _objectTable = gameObjects;

            //_log.Debug($"[Action Manager]: x {FFXIVClientStructs.FFXIV.Client.UI.AddonContentsFinder.Addresses.VTable.}");
            

        }

        public void Dispose()
        {
            _framework.Update -= framework_Update;
        }

        private void framework_Update(IFramework framework)
        {
            //_log.Debug($"_condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty] {_condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty]}");
            //_log.Debug($"_condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty56] {_condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty56]}");
            //_log.Debug($"_condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty95] {_condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty95]}");
            //_log.Debug($"_condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty97] {_condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundToDuty97]}");

            uint isWalking = Marshal.ReadByte((IntPtr)gameControl, 23163);
            if (_condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.Mounted] || 
                _condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty] || 
                _condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat] ||
                _condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty56] ||
                _condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty95] ||
                _condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundToDuty97])
            {
                if (isWalking == 1)
                {
                    Marshal.WriteByte((IntPtr)gameControl, 23163, 0x0);
                }
            }
            else if (isWalking == 0)
            {
                Marshal.WriteByte((IntPtr)gameControl, 23163, 0x1);
            }

            //FFXIVClientStructs.FFXIV.Client.UI.AddonSelectYesno

            /*var skip = false;
            if (!skip && _clientState != null && _clientState.LocalPlayer != null && _clientState.LocalPlayer.IsCasting)
            {
                //_keyState.SetRawValue(Dalamud.Game.ClientState.Keys.VirtualKey.ESCAPE, 1);
                //_keyState.SetRawValue(Dalamud.Game.ClientState.Keys.VirtualKey.ESCAPE, 0);
                var raw = _keyState.GetRawValue(VirtualKey.ESCAPE);
                getRefValue((int)VirtualKey.SPACE) = 3;
                skip = true;
                //getRefValue((int)VirtualKey.ESCAPE) = raw;
                _log.Debug($"[Action Manager]: aa");
                //_log.Debug($"[Action Manager]: {_clientState.LocalPlayer.CastActionId} {_clientState.LocalPlayer.CastActionType} {_clientState.LocalPlayer.CastTargetObjectId} {_clientState.LocalPlayer.ObjectId}");

            }else if (skip && !_clientState.LocalPlayer.IsCasting)
            {
                skip = false;
            }*/
        }

        private bool UseActionDetour(FFXIVClientStructs.FFXIV.Client.Game.ActionManager* am, ActionType type, uint acId, long target, uint a5, uint a6, uint a7, void* a8)
        {
            try
            {
                _log.Debug($"[Action Manager]: {type} {acId} {target} {a5} {a6} {a7}");
                if (ActionType.Action == type && acId > 7 && !Abilities.general.ContainsKey(acId))
                {
                    if (_clientState != null
                        && _clientState.LocalPlayer != null
                        && _clientState.LocalPlayer.ClassJob != null
                        && _clientState.LocalPlayer.ClassJob.GameData != null)
                    {
                        var role = _clientState.LocalPlayer.ClassJob.GameData.Role;
                        if (_config.BannedActionRoles.Contains((ActionRoles)role))
                        {
                            return false;
                        }

                        if (_config.canSelfCast)
                        {
                            //_log.Debug($"[Action Manager]: {_objectTable.FirstOrDefault(x => x.ObjectId.Equals(target))?.ObjectKind}");
                            if (_clientState.LocalPlayer.ObjectId == target /*|| _objectTable.FirstOrDefault(x => x.ObjectId.Equals((uint)target))?.ObjectKind != Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player*/)
                            {
                                return false;
                            }
                        }

                        if (_config.AbilityRestrictionLevel > 0)
                        {                            
                            switch ((ActionRoles)role)
                            {
                                case ActionRoles.NonCombat:
                                    break;
                                case ActionRoles.Tank:
                                    break;
                                case ActionRoles.MeleeDps:
                                    break;
                                case ActionRoles.RangedDps:
                                    break;
                                case ActionRoles.Healer:
                                    switch (_config.AbilityRestrictionLevel)
                                    {
                                        case AbilityRestrictionLevel.Hardcore:
                                            if (!Abilities.SpellsHardcore.Any(x => acId == x))
                                            {
                                                return false;
                                            }
                                            break;
                                        case AbilityRestrictionLevel.Minimal:
                                            if (!Abilities.SpellsMinimal.Any(x => acId == x))
                                            {
                                                return false;
                                            }
                                            break;
                                        case AbilityRestrictionLevel.Advanced:
                                            if (!Abilities.SpellsAdvanced.Any(x => acId == x))
                                            {
                                                return false;
                                            }
                                            break;
                                        case AbilityRestrictionLevel.Spec:
                                            if (!Abilities.SpellsSpec.Any(x => acId == x))
                                            {
                                                return false;
                                            }
                                            break;
                                    }
                                    break;
                            }
                        }
                    }
                    if (_config.GilCheck)
                    {
                        var gil = InventoryManager.Instance()->GetGil();
                        if (gil > _config.GilLimit) return false;
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e.ToString());
            }
            var ret = UseActionHook.Original(am, type, acId, target, a5, a6, a7, a8);
            return ret;
        }

    }
}

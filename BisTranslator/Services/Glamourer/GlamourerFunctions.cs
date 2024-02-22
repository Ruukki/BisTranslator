using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Glamourer.Gui.Equipment;
using System.Diagnostics;
using BisTranslator.ClientData;

namespace BisTranslator.Services.Glamourer;

/// <summary> the type of statechange provided by glamourerIPC </summary>
public enum StateChangeType
{
    Model,
    EntireCustomize,
    Customize,
    Equip,
    Weapon,
    Stain,
    Crest,
    Parameter,
    Design,
    Reset,
    Other,
}

/// <summary>
/// Create a sealed class for our interop manager.
/// </summary>
public class GlamourerFunctions : IDisposable
{
    private readonly GlamourerService _Interop;               // the glamourer interop class
    private readonly IFramework _framework;             // the framework for running tasks on the main thread
    private readonly ClientUserInfo _charaDataHelpers;      // character data updates/helpers (framework based)
    private Action<int, nint, Lazy<string>> _ChangedDelegate;       // delgate for  GlamourChanged, so it is subscribed and disposed properly
    private string? _lastCustomizationData; // store the last customization data
    private CancellationTokenSource _cts;
    private readonly IPluginLog _log;

    private bool finishedDrawingGlamChange = true;
    private bool isGagSpeakGlamourEventExecuting = false;
    private bool disableGlamChangeEvent = false;

    public GlamourerFunctions(ClientUserInfo ClientUserInfo, GlamourerService GlamourerService, IFramework framework, IPluginLog log)
    {
        // initialize the glamourer interop class
        _Interop = GlamourerService;
        _charaDataHelpers = ClientUserInfo;
        _framework = framework;
        _log = log;


        // initialize delegate for the glamourer changed event
        _ChangedDelegate = (type, address, customize) => GlamourerChanged(type, address);
        _lastCustomizationData = "";
        _cts = new CancellationTokenSource(); // for handling gearset changes

        _Interop._StateChangedSubscriber.Subscribe(_ChangedDelegate);    // to know when glamourer state changes
        _framework.Update += FrameworkUpdate;         // to know when we should process the last glamour data

        _log.Debug($"[GlamourerService]: GlamourerFunctions initialized!");
        _ = Task.Run(WaitForPlayerToLoad); // wait for player load, then get the player object        
    }

    public void Dispose()
    {
        _log.Debug($"[GlamourerService]: Disposing of GlamourerService");
        _Interop._StateChangedSubscriber.Unsubscribe(_ChangedDelegate);
        _framework.Update -= FrameworkUpdate;
    }

    public void FrameworkUpdate(IFramework framework)
    {
        // if we have finiahed drawing
        if (finishedDrawingGlamChange)
        {
            // and we have disabled the glamour change event still
            if (disableGlamChangeEvent)
            {
                // make sure to turn that off and reset it
                finishedDrawingGlamChange = false;
                disableGlamChangeEvent = false;
                _log.Debug($"[FrameworkUpdate] Re-Allowing Glamour Change Event");
            }
        }
    }

    private async Task WaitForPlayerToLoad()
    {
        try
        {
            while (!await _charaDataHelpers.GetIsPlayerPresentAsync().ConfigureAwait(false))
            {
                await Task.Delay(100).ConfigureAwait(false);
            }
            // fire the event for a refresh
            GlamourEventFired(UpdateType.Login);
        }
        catch (Exception ex)
        {
            _log.Error($"[WaitForPlayerToLoad]: Error waiting for player to load: {ex.Message}");
        }
    }


    // we must account for certain situations, and perform an early exit return or access those functions if they are true.
    private unsafe void GlamourerChanged(int type, nint address)
    {
        // just know the type and address
        if (address != _charaDataHelpers.Address)
        {
            _log.Verbose($"[GlamourerChanged]: Change not from Character, IGNORING"); return;
        }
        // if the address is us, check to see if we changed jobs
        var chara = (Character*)address;
        var classJob = chara->CharacterData.ClassJob;

        // if the class job is different than the one stored, then we have a class job change (CRITICAL TO UPDATING PROPERLY)
        if (classJob != _charaDataHelpers.ClassJobId)
        {
            _log.Verbose($"[CHARA HANDLER UPDATE] classjob changed from {_charaDataHelpers.ClassJobId} to {classJob}");
            // update the stored class job
            _charaDataHelpers.ClassJobId = classJob;
            // invoke jobChangedEvent to call the glamourerRevert
            GlamourEventFired(UpdateType.JobChange);
            return;
        }

        // if it is a design change, then we should reapply the gags and restraint sets
        if (type == (int)StateChangeType.Design)
        {
            _log.Verbose($"[GlamourerChanged]: StateChangeType is Design, Re-Applying any Gags or restraint sets configured if conditions are satisfied");
            // process the latest glamourerData and append our alterations
            GlamourEventFired(UpdateType.RefreshAll);
            return;
        }

        // CONDITION FIVE: The StateChangeType is an equip or Stain, meaning that the player changed classes in game, or gearsets, causing multiple events to trigger
        if (type == (int)StateChangeType.Equip || type == (int)StateChangeType.Stain || type == (int)StateChangeType.Weapon)
        {
            var enumType = (StateChangeType)type;
            _log.Verbose($"[GlamourerChanged]: StateChangeType is {enumType}");
            GlamourEventFired(UpdateType.RefreshAll);
        }
        else
        {
            var enumType = (StateChangeType)type;
            _log.Verbose($"[GlamourerChanged]: GlamourerChangedEvent was not equipmenttype, stain, or weapon; but rather {enumType}");
        }
    }

    public async void GlamourEventFired(UpdateType updateType)
    {
        // Otherwise, fire the events!
        _cts.Cancel();
        disableGlamChangeEvent = true;
        // only execute if our wardrobe is enabled
        if (true)
        {
            _log.Debug($"================= [ " + updateType.ToString().ToUpper() + " GLAMOUR EVENT FIRED ] ====================");
            // conditionals:
            // condition 1 --> It was an update restraint set event. In this case, we should recall the restraint set applier
            try
            {
                if (updateType == UpdateType.UpdateRestraintSet)
                {
                    _log.Debug($"[GlamourEventFired]: Processing Restraint Set Update");
                    await ApplyRestrainSetToCachedCharacterData(); // correct
                }
                // condition 2 --> it was an disable restraint set event, we should revert back to automation, but then reapply the gags
                if (updateType == UpdateType.DisableRestraintSet)
                {
                    try
                    {
                        _log.Debug($"[GlamourEventFired]: Processing Restraint Set Disable, reverting to automation");
                        // revert based on our setting
                        /*if (_characterHandler.playerChar._revertStyle == RevertStyle.ToAutomationOnly)
                        {
                            // if we want to just revert to automation, then do just that.
                            await _Interop.GlamourerRevertCharacterToAutomation(_charaDataHelpers.Address);
                        }
                        if (_characterHandler.playerChar._revertStyle == RevertStyle.ToGameOnly)
                        {
                            // if we want to always revert to game, then do just that
                            await _Interop.GlamourerRevertCharacter(_charaDataHelpers.Address);
                        }*/
                        if (true/*_characterHandler.playerChar._revertStyle == RevertStyle.ToGameThenAutomation*/)
                        {
                            // finally, if we want to revert to the game, then to any automation for this class after, then do just that
                            await _Interop.GlamourerRevertCharacter(_charaDataHelpers.Address);
                            await _Interop.GlamourerRevertCharacterToAutomation(_charaDataHelpers.Address);
                        }
                        // dont know how to tell if it was successful, so we will just assume it was
                    }
                    catch (Exception)
                    {
                        _log.Error($"Error reverting glamourer to automation");
                    }
                }

                // condition 6 --> it was a job change event, refresh all, but wait for the framework thread first
                if (updateType == UpdateType.JobChange)
                {
                    _log.Debug($"[GlamourEventFired]: Processing Job Change");
                    await Task.Run(() => _charaDataHelpers.RunOnFrameworkThread(UpdateCachedCharacterData));
                }

                // condition 7 --> it was a refresh all event, we should reapply all the gags and restraint sets
                if (updateType == UpdateType.RefreshAll || updateType == UpdateType.ZoneChange || updateType == UpdateType.Login)
                {
                    _log.Debug($"[GlamourEventFired]: Processing Refresh All // Zone Change // Login // Job Change");
                    await UpdateCachedCharacterData();
                }
                // condition 8 --> it was a safeword event, we should revert to the game, then to game and disable toys
                if (updateType == UpdateType.Safeword)
                {
                    _log.Debug($"[GlamourEventFired]: Processing Safeword");
                    await _Interop.GlamourerRevertCharacter(_charaDataHelpers.Address);
                    // disable all toys
                }

            }
            catch (Exception ex)
            {
                _log.Error($"[GlamourEventFired]: Error processing glamour event: {ex.Message}");
            }
            finally
            {
                _log.Debug($"[GlamourEventFired]: re-allowing GlamourChangedEvent");
                isGagSpeakGlamourEventExecuting = false;
                finishedDrawingGlamChange = true;
            }
        }
        else
        {
            _log.Debug($"[GlamourEventFired]: Wardrobe is disabled, so we wont be updating/applying any gag items");
        }
    }

    /// <summary> Updates the raw glamourer customization data with our gag items and restraint sets, if applicable </summary>
    public async Task UpdateCachedCharacterData()
    {
        // for privacy reasons, we must first make sure that our options for allowing such things are enabled.
        if (true)
        {
            await ApplyRestrainSetToCachedCharacterData();
        }
        else
        {
            _log.Debug($"[GlamourerChanged]: Restraint Set Auto-Equip disabled, IGNORING");
        }
    }

    /// <summary> Applies the only enabled restraint set to your character on update trigger. </summary>
    public async Task ApplyRestrainSetToCachedCharacterData()
    { // dummy placeholder line
        // Find the restraint set with the matching name
        List<Task> tasks = new List<Task>();
        /*foreach (var restraintSet in _restraintSetManager._restraintSets)
        {
            // If the restraint set is enabled
            if (restraintSet._enabled)
            {
                // Iterate over each EquipDrawData in the restraint set
                foreach (var pair in restraintSet._drawData)
                {
                    // see if the item is enabled or not (controls it's visibility)
                    if (pair.Value._isEnabled)
                    {
                        // because it is enabled, we will still apply nothing items
                        tasks.Add(_Interop.SetItemToCharacterAsync(
                            _charaDataHelpers.Address,
                            Convert.ToByte(pair.Key), // the key (EquipSlot)
                            pair.Value._gameItem.Id.Id, // Set this slot to nothing (naked)
                            pair.Value._gameStain.Id, // The _drawData._gameStain.Id
                            0));
                    }
                    else
                    {
                        // Because it was disabled, we will treat it as an overlay, ignoring it if it is a nothing item
                        if (!pair.Value._gameItem.Equals(ItemIdVars.NothingItem(pair.Value._slot)))
                        {
                            // Apply the EquipDrawData
                            tasks.Add(_Interop.SetItemToCharacterAsync(
                                _charaDataHelpers.Address,
                                Convert.ToByte(pair.Key), // the key (EquipSlot)
                                pair.Value._gameItem.Id.Id, // The _drawData._gameItem.Id.Id
                                pair.Value._gameStain.Id, // The _drawData._gameStain.Id
                                0));
                        }
                        else
                        {
                            _log.Debug($"[ApplyRestrainSetToData] Skipping over {pair.Key}!");
                        }
                    }
                }
                // early exit, we only want to apply one restraint set
                _log.Debug($"[ApplyRestrainSetToData]: Applying Restraint Set to Cached Character Data");
                await Task.WhenAll(tasks);
                return;
            }
        }*/
        _log.Debug($"[ApplyRestrainSetToData]: No restraint sets are enabled, skipping!");
    }

    public enum UpdateType
    {
        // Used for enabling a restraint set, or updating it
        UpdateRestraintSet,
        // used when disabling a restraint set
        DisableRestraintSet,
        // gag equipped
        GagEquipped,
        // used when removing a gag or having it reset to none
        GagUnEquipped,
        UpdateGags,
        // used for updating a characters information in general
        RefreshAll,
        // used for when we detect a job change
        JobChange,
        // used for when we detect a login
        Login,
        // used for when we detect a zone change
        ZoneChange,
        // for safeword
        Safeword,
    }
}

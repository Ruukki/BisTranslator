using Dalamud.Plugin.Services;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Plugin.Ipc;
using Glamourer.Api.Enums;
using Glamourer.Api.Api;
using Glamourer.Api.Helpers;
using Glamourer.Api.IpcSubscribers;

namespace BisTranslator.Glamourer
{
    public class GlamourerManager : IDisposable
    {
        private IDalamudPluginInterface _pluginInterface;
        private IPluginLog _log;

        private ICallGateSubscriber<int> apiVersion;
        private ICallGateSubscriber<int, ApiEquipSlot, ulong, IReadOnlyList<byte>, uint, ApplyFlag, GlamourerApiEc> setItem;

        public EventSubscriber<nint, StateChangeType> StateWasChanged;

        private bool isThrottled = false;

        public GlamourerManager(IDalamudPluginInterface pi, IPluginLog log)
        {
            _pluginInterface = pi;
            _log = log;

            apiVersion = _pluginInterface.GetIpcSubscriber<int>("Glamourer.ApiVersion");
            setItem = _pluginInterface.GetIpcSubscriber<int, ApiEquipSlot, ulong, IReadOnlyList<byte>, uint, ApplyFlag, GlamourerApiEc>("Glamourer.SetItem.V3");

            
            //StateWasChanged = StateChangedWithType.Subscriber(_pluginInterface, OnStateChanged);
            //StateWasChanged.Enable();
        }

        public int GetVersion()
        {
            return apiVersion.InvokeFunc();
        }

        public string SetItem()
        {
            return setItem.InvokeFunc(0, ApiEquipSlot.Body, 0, new List<byte>(), 0, ApplyFlag.Equipment).ToString();
        }

        private void OnStateChanged(nint handle, StateChangeType type)
        {
            if (!isThrottled)
            {
                _log.Debug($"[GlamourerManager] {handle} {type.ToString()}");
                if (type == StateChangeType.Equip)
                {
                    SetItem();
                }
                isThrottled = true;
                Task.Delay(1000).ContinueWith(_ =>
                {
                    isThrottled = false;
                });
            }            
        }


        public void Dispose()
        {            
            //StateWasChanged.Disable();
            //StateWasChanged?.Dispose();
        }

    }
}

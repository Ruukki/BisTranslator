using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Services;
using ECommons.Configuration;
using ECommons.Logging;
using MemoryPack;
using Moodles.Data;
using System;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BisTranslator.Moodles
{
    public class MoodleManager
    {
        private IDalamudPluginInterface _pluginInterface;
        private IPluginLog _log;
        private string fullnameWorld;

        private ICallGateSubscriber<int>? _moodlesApiVersion;
        private ICallGateSubscriber<string, string> GetStatusManagerByName;
        private ICallGateSubscriber<string, string, object> _setStatusManager;
        private static ICallGateProvider<MoodlesStatusInfo, object?>? GagSpeakTryMoodleStatus;
        private readonly ICallGateSubscriber<List<Guid>, string, object> _removeStatusByGuids;
        private MyStatus curse;

        private static readonly MemoryPackSerializerOptions SerializerOptions = new()
        {
            StringEncoding = StringEncoding.Utf16,
        };

        public MoodleManager(IDalamudPluginInterface pi, IPluginLog log, string fullnameWithWorld)
        {
            _pluginInterface = pi;
            _log = log;
            fullnameWorld = fullnameWithWorld;

            _moodlesApiVersion = _pluginInterface.GetIpcSubscriber<int>("Moodles.Version");
            GetStatusManagerByName = _pluginInterface.GetIpcSubscriber<string, string>("Moodles.GetStatusManagerByName");
            _setStatusManager = _pluginInterface.GetIpcSubscriber<string, string, object>("Moodles.SetStatusManagerByName");
            GagSpeakTryMoodleStatus = _pluginInterface.GetIpcProvider<MoodlesStatusInfo, object?>("GagSpeak.TryOnMoodleStatus");
            _removeStatusByGuids = pi.GetIpcSubscriber<List<Guid>, string, object>("Moodles.RemoveMoodlesByGUIDByName");
            //_log.Warning($"Moodles: {_moodlesApiVersion.InvokeFunc()}");

            curse = new MyStatus()
            {
                IconID = 217861,
                Title = "[color=555]»[/color] [glow=537]Accursed Mark[/glow] [color=555]« [/color] ",
                Description = "[color=9]A[/color][color=48] strange and insidious[/color][color=9] curse lingers in the air and on the[/color][glow=56] Wearer[/glow]. [color=9]An Inescapable grip holding the wearer gently, but firmly. [/color]",
                Type = StatusType.Special,
                Dispelable = false,
                Stacks = 1,
                CustomFXPath = "dk05ht_grv0h",
                StacksIncOnReapply = 1,
                NoExpire = true,
                AsPermanent = true,
                Hours = 1,
            };
        }

        private bool isApiAvailable()
        {
            return _moodlesApiVersion?.InvokeFunc() >= 1;
        }

        private string getStatusManagerByName()
        {
            try
            {
                return GetStatusManagerByName.InvokeFunc(fullnameWorld);
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
                return string.Empty;
            }
        }

        private void setStatus(string statusBase64)
        {
            try
            {
                _setStatusManager.InvokeAction(fullnameWorld, statusBase64);
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }

        private void setStatusGagSpeak(MoodlesStatusInfo statusInfo)
        {
            try
            {
                GagSpeakTryMoodleStatus.SendMessage(statusInfo);
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }

        public void SetMoodle(int stacks = 1)
        {
            if(stacks > 16)
            {
                stacks = 16;
            }
            if (isApiAvailable())
            {
                //_log.Warning($"{base64}");
                //_log.Warning($"{x.Count}");
                curse.Stacks = stacks;
                setStatusGagSpeak(curse.ToStatusInfoTuple());
            }
        }

        public void ClearMoodle()
        {
            var base64 = getStatusManagerByName();
            var data = Convert.FromBase64String(base64);
            var x = MemoryPackSerializer.Deserialize<List<MyStatus>>(data)??new List<MyStatus>();
            var toRemove = x.FirstOrDefault(y => y.Title.Equals(curse.Title))?.GUID;

            if (toRemove != null) {
                try
                {
                    _removeStatusByGuids.InvokeAction(new List<Guid>() { toRemove.Value }.ToList(), fullnameWorld);
                }
                catch (Exception e)
                {
                    _log.Error(e.Message);
                }
            }
        }
    }
}

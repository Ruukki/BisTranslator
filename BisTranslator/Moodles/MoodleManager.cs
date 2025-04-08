using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using ECommons.Configuration;
using ECommons.Logging;
using MemoryPack;
using Moodles.Data;
using System;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BisTranslator.Moodles
{
    public class MoodleManager : IDisposable
    {
        private IDalamudPluginInterface _pluginInterface;
        private IPluginLog _log;
        private ICondition _condition;
        private Configuration _config;

        private ICallGateSubscriber<int>? _moodlesApiVersion;
        private ICallGateSubscriber<string, string> GetStatusManagerByName;
        private ICallGateSubscriber<string, string, object> _setStatusManager;
        private static ICallGateProvider<MoodlesStatusInfo, object?>? GagSpeakTryMoodleStatus;
        private readonly ICallGateSubscriber<List<Guid>, string, object> _removeStatusByGuids;        

        private DateTime lastUpdate = DateTime.MinValue;

        //Moodles
        private MyStatus curse = MoodleLib.Curse;
        private MyStatus lightPockets = MoodleLib.LightPockets;
        private MyStatus heavyPockets = MoodleLib.Burdened;

        private static readonly MemoryPackSerializerOptions SerializerOptions = new()
        {
            StringEncoding = StringEncoding.Utf16,
        };

        public MoodleManager(IDalamudPluginInterface pi, IPluginLog log, Configuration config, ICondition condition)
        {
            _pluginInterface = pi;
            _log = log;
            _config = config;
            _condition = condition;

            _moodlesApiVersion = _pluginInterface.GetIpcSubscriber<int>("Moodles.Version");
            GetStatusManagerByName = _pluginInterface.GetIpcSubscriber<string, string>("Moodles.GetStatusManagerByName");
            _setStatusManager = _pluginInterface.GetIpcSubscriber<string, string, object>("Moodles.SetStatusManagerByName");
            GagSpeakTryMoodleStatus = _pluginInterface.GetIpcProvider<MoodlesStatusInfo, object?>("GagSpeak.TryOnMoodleStatus");
            _removeStatusByGuids = pi.GetIpcSubscriber<List<Guid>, string, object>("Moodles.RemoveMoodlesByGUIDByName");
            //_log.Warning($"Moodles: {_moodlesApiVersion.InvokeFunc()}");

            _condition.ConditionChange += ConditionChanged;

            //Load moodles ovrrides            
            
        }

        private bool isApiAvailable()
        {
            return _moodlesApiVersion?.InvokeFunc() >= 1;
        }

        private string getStatusManagerByName()
        {
            try
            {
                string result = GetStatusManagerByName.InvokeFunc(_config.FullNameWithServer);
                //_log.Debug($"MoodleManager Name:{_config.FullNameWithServer} result:{result.IsNullOrEmpty()}");
                return result;
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
                _setStatusManager.InvokeAction(_config.FullNameWithServer, statusBase64);
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

        public void ClearMoodle(MyStatus moodle)
        {
            var x = GetMoodleList();
            var toRemove = x.Where(y => y.Title.Equals(moodle.Title));

            foreach (var item in toRemove)
            {
                try
                {
                    _removeStatusByGuids.InvokeAction(new List<Guid>() { item.GUID }.ToList(), _config.FullNameWithServer);
                }
                catch (Exception e)
                {
                    _log.Error(e.Message);
                }
            }
        }

        public void ConditionChanged(ConditionFlag flag, bool value)
        {
            if ((DateTime.Now - lastUpdate).TotalSeconds < 1) { return; }
            lastUpdate = DateTime.Now;
            //Giloverflow
            _log.Debug($"[MoodleManager] GilCheck {_config.GilCheck}");
            if (_config.GilCheck)
            {
                var moodles = GetMoodleList();
                var lightWeight = moodles.Any(x=> x.Title.Equals(MoodleLib.LightPockets.Title));
                var heavyWeight = moodles.Any(x => x.Title.Equals(MoodleLib.Burdened.Title));
                _log.Debug($"[MoodleManager] lightWeight {lightWeight}  heavyWeight {heavyWeight}");
                _log.Debug($"[MoodleManager] GilOverflow {_config.GilOverflow}");
                if (_config.GilOverflow)
                {
                    if (lightWeight) { ClearMoodle(MoodleLib.LightPockets); }
                    if (!heavyWeight) { setStatusGagSpeak(MoodleLib.Burdened.ToStatusInfoTuple()); }
                }else
                {
                    if (!lightWeight) { setStatusGagSpeak(MoodleLib.LightPockets.ToStatusInfoTuple()); }
                    if (heavyWeight) { ClearMoodle(MoodleLib.Burdened); }
                }
            }
            else
            {
                ClearMoodle(MoodleLib.LightPockets);
                ClearMoodle(MoodleLib.Burdened);
            }
        }

        public void Dispose()
        {

        }

        private List<MyStatus> GetMoodleList()
        {
            var base64 = getStatusManagerByName();
            var data = Convert.FromBase64String(base64);
            return MemoryPackSerializer.Deserialize<List<MyStatus>>(data) ?? new List<MyStatus>();
        }

        public void RunUpdate()
        {
            lightPockets.Description = lightPockets.Description.Replace("%amount%", _config.GilLimitFormatted.ToString());
            heavyPockets.Description = heavyPockets.Description.Replace("%name%", _config.Name);
            heavyPockets.Description = heavyPockets.Description.Replace("%amount%", _config.GilLimitFormatted.ToString());
        }
    }
}

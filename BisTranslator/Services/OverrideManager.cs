using BisTranslator.Glamourer;
using BisTranslator.Moodles;
using BisTranslator.Permissions;
using BisTranslator.Translator;
using BisTranslator.Windows;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Moodles.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace BisTranslator.Services
{
    public class OverrideManager
    {
        private readonly IClientState _clientState;
        private readonly IPluginLog _log;
        private Configuration _config;
        IDalamudPluginInterface _pluginInterface;
        private Widget _widget;
        private string fullnameWorld;
        private MoodleManager _moodles;
        private GlamourerManager _glamourerManager;
        public OverrideManager(IClientState clientState,
                               IPluginLog log,
                               Configuration config,
                               IDalamudPluginInterface pluginInterface,
                               Widget widget,
                               MoodleManager moodles,
                               GlamourerManager glamourerManager)
        {
            _clientState = clientState;
            _log = log;
            _config = config;
            _pluginInterface = pluginInterface;
            _widget = widget;
            _moodles = moodles;

            //Test
            _glamourerManager = glamourerManager;
        }

        public void Dispose()
        {
            _glamourerManager.Dispose();
        }

        public void Login()
        {
            if (_clientState != null && _clientState.LocalPlayer != null)
            {
                fullnameWorld = $"{_clientState.LocalPlayer.Name}@{_clientState.LocalPlayer.HomeWorld.Value.InternalName}";
                var configOverride = PermissionConst.PlayerOverrides.FirstOrDefault(x => x.ToString() == $"{fullnameWorld}")?.Configuration;
                //_log.Debug($"configOverride: {configOverride == null}");
                if (configOverride != null)
                {
                    configOverride.OriginalName = _clientState.LocalPlayer.Name.TextValue;
                    _config.Override(configOverride, _pluginInterface);
                }
                else
                {
                    var def = PermissionConst.Default;
                    def.OriginalName = _clientState.LocalPlayer.Name.TextValue;
                    def.Name = _clientState.LocalPlayer.Name.TextValue;
                    _config.Override(def, _pluginInterface);
                }

                    /*if (!_widget.IsOpen)
                    {
                        _widget.Toggle();
                    }*/

                    //Run moodle updates
                    ClearMoodle();
                _moodles.RunUpdate();
                _moodles.SetMoodle(_config.CurseStacks);

                //Test
                //_log.Debug($"[TEST] {_glamourerManager.SetItem()}");


            }
        }

        public void ClearMoodle()
        {
            if (_moodles != null)
            {
                try
                {
                    //_moodles.ClearMoodle(MoodleLib.Curse);
                    _moodles.ClearMoodles(new List<MyStatus>() { MoodleLib.Curse, MoodleLib.Burdened, MoodleLib.LightPockets });
                }
                catch(Exception e) 
                {
                    _log.Error(e.Message);
                }
            }
        }
    }
}

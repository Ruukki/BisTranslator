using BisTranslator.Permissions;
using BisTranslator.Translator;
using BisTranslator.Windows;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
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
        public OverrideManager(IClientState clientState, IPluginLog log, Configuration config, IDalamudPluginInterface pluginInterface, Widget widget)
        {
            _clientState = clientState;
            _log = log;
            _config = config;
            _pluginInterface = pluginInterface;
            _widget = widget;

            _clientState.Login += Login;
        }

        public void Dispose()
        {
            _clientState.Login -= Login;
        }

        public void Login()
        {
            if (_clientState != null && _clientState.LocalPlayer != null &&  _clientState.LocalPlayer.HomeWorld.GameData != null)
            {
                var configOverride = PermissionConst.PlayerOverrides.FirstOrDefault(x => x.ToString() == $"{_clientState.LocalPlayer.Name.TextValue}@{_clientState.LocalPlayer.HomeWorld.GameData.InternalName}")?.Configuration;
                //_log.Debug($"configOverride: {configOverride == null} _clientState.LocalPlayer.Name.TextValue: {_clientState.LocalPlayer.Name.TextValue} _clientState.LocalPlayer.HomeWorld.GameData.InternalName: {_clientState.LocalPlayer.HomeWorld.GameData.InternalName}");
                if ( configOverride != null )
                {
                    _config.Override(configOverride, _pluginInterface);
                }
            }
            if (!_widget.IsOpen)
            {
                _widget.Toggle();
            }
        }
    }
}

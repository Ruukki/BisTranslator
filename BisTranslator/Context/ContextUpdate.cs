using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BisTranslator.Context
{
    internal class ContextUpdate : IDisposable
    {
        private IPluginLog _log;
        public ContextUpdate(IPluginLog log)
        {
            _log = log;

            new ECommons.EzEventManager.EzTerritoryChanged((x) => UpdateTerritoryChanged());
        }

        public void UpdateTerritoryChanged()
        {
            PlayerContext.TerritoryType = ECommons.GameHelpers.Content.TerritoryTypeRow;
            _log.Debug($"[ContextUpdate] Territory RowId {PlayerContext.TerritoryType.Value.RowId}");
        }

        public void Dispose() { }
    }
}

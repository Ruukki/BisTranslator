using ECommons.ExcelServices;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BisTranslator.Context
{
    internal static class PlayerContext
    {
        internal static int[] TerritoryWhitelist = { 60};//Gathering Moon
        internal static TerritoryType? TerritoryType { get; set; }

        internal static bool isInWhitelistedTerritory()
        {
            if (TerritoryType.HasValue)
            {
                if (TerritoryWhitelist.Any(x=> x == TerritoryType.Value.TerritoryIntendedUse.RowId))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

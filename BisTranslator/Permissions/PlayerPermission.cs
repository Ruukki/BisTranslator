using Dalamud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BisTranslator.Permissions
{
    public static class PlayerPermission
    {
        public static uint PlayerObjectId { get; set; }
        public static string? PlayerName { get; set;}
        public static string? PlayerWorld { get; set; }

        public static PlayerOverride playerOverride { get; private set; }

        public static void LoadPlayer()
        {
            if (PlayerName != null && PlayerWorld != null)
            {
                //playerOverride = PermissionConst.PlayerOverrides.FirstOrDefault(x => x.ToString() == $"{PlayerName}@{PlayerWorld}") ?? new PlayerOverride(PlayerName, PlayerWorld);
            }
        }
    }
}

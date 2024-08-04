using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BisTranslator.Permissions
{
    public static class PermissionConst
    {
        public static List<PlayerOverride> PlayerOverrides = new()
        {
            /*new PlayerOverride("Kayda Hagarin", "Spriggan", new Configuration()
            {

            }),*/
            new PlayerOverride("Miki Kiki", "Spriggan", new Configuration()
            {
                //Name = "",
                Name = "slut",
                BannedActionRoles = new() {},
                AbilityRestrictionLevel = AbilityRestrictionLevel.Minimal,
                GilCheck = true,
                GilLimit = 110_000,
                ForcedWalk = true,
                canSelfCast = false,
                BigPussy = false,
                lockedUiOverride = true,
                tester = true,
            }),
            new PlayerOverride("Vie Crevan", "Spriggan", new Configuration()
            {
                Name = "kitty",
                BannedActionRoles = new() {},
                AbilityRestrictionLevel = AbilityRestrictionLevel.Minimal,
                GilCheck = false,
                ForcedWalk = true,
                canSelfCast = false,
                BigPussy = false,
                lockedUiOverride = true,
                tester = true
            }),
            new PlayerOverride("Mia Hime", "Spriggan", new Configuration()
            {
                Name = "maid",
                BannedActionRoles = new() {ActionRoles.Tank, ActionRoles.MeleeDps, ActionRoles.RangedDps},
                AbilityRestrictionLevel = AbilityRestrictionLevel.Advanced,
                GilCheck = true,
                GilLimit = 110_000,
                ForcedWalk = true,
                canSelfCast = false,
                BigPussy = false,
                lockedUiOverride = true,
                tester = true,
            }),
        };
    }

    public class PlayerOverride
    {
        public string Name { get; set; }
        public string World { get; set; }
        public Configuration Configuration { get; set; }

        public PlayerOverride(string name, string world, Configuration configuration)
        {
            Name = name;
            World = world;
            Configuration = configuration;
        }

        public override string ToString()
        {
            return $"{Name}@{World}";
        }
    }
}

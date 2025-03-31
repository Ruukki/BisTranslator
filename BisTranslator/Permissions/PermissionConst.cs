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
                Name = "Miki",
                CommandName = "slut",
                BannedActionRoles = new() {},
                AbilityRestrictionLevel = AbilityRestrictionLevel.MovementBan,
                GilCheck = false,
                GilLimit = 110_000,
                ForcedWalk = true,
                canSelfCast = true,
                BigPussy = true,
                lockedUiOverride = true,
                tester = false,
                ForcedChat = true,
                OwnerName = "Miki Kiki",
                CurseStacks = 13,
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
            new PlayerOverride("Kayda Hagarin", "Spriggan", new Configuration()
            {
                Name = "",
                CommandName = "drone",
                BannedActionRoles = new() {ActionRoles.Tank, ActionRoles.MeleeDps, ActionRoles.RangedDps},
                AbilityRestrictionLevel = AbilityRestrictionLevel.MovementBan,
                GilCheck = false,
                GilLimit = long.MaxValue,
                ForcedWalk = true,
                canSelfCast = false,
                BigPussy = false,
                lockedUiOverride = true,
                tester = false,
            }),
            new PlayerOverride("Kaori Kawashima", "Phantom", new Configuration()
            {
                Name = "whore",
                CommandName = "whore",
                BannedActionRoles = new() {ActionRoles.Tank, ActionRoles.MeleeDps, ActionRoles.RangedDps},
                AbilityRestrictionLevel = AbilityRestrictionLevel.MovementBan,
                GilCheck = false,
                GilLimit = long.MaxValue,
                ForcedWalk = true,
                canSelfCast = false,
                BigPussy = false,
                lockedUiOverride = true,
                tester = false,
            }),
            new PlayerOverride("Depressing Mistake", "Raiden", new Configuration()
            {
                Name = "slut",
                CommandName = "slut",
                BannedActionRoles = new() {ActionRoles.Tank, ActionRoles.MeleeDps, ActionRoles.Healer},
                AbilityRestrictionLevel = AbilityRestrictionLevel.MovementBan,
                GilCheck = false,
                GilLimit = long.MaxValue,
                ForcedWalk = true,
                canSelfCast = true,
                BigPussy = false,
                lockedUiOverride = true,
                tester = false,
            }),
            new PlayerOverride("Lady Marie", "Phantom", new Configuration()
            {
                Name = "slut",
                CommandName = "slut",
                BannedActionRoles = new() {ActionRoles.Tank, ActionRoles.MeleeDps, ActionRoles.RangedDps},
                AbilityRestrictionLevel = AbilityRestrictionLevel.MovementBan,
                GilCheck = true,
                GilLimit = 420,
                ForcedWalk = true,
                canSelfCast = false,
                BigPussy = false,
                lockedUiOverride = true,
                tester = false,
            }),
            new PlayerOverride("Redrix Valentia", "Raiden", new Configuration()
            {
                Name = "doll",
                CommandName = "doll",
                BannedActionRoles = new() {ActionRoles.Tank, ActionRoles.MeleeDps, ActionRoles.RangedDps},
                AbilityRestrictionLevel = AbilityRestrictionLevel.MovementBan,
                GilCheck = true,
                GilLimit = 100000,
                ForcedWalk = true,
                canSelfCast = false,
                BigPussy = false,
                lockedUiOverride = true,
                tester = false,
            }),
            new PlayerOverride("Sister Frieda", "Phantom", new Configuration()
            {
                Name = "whore",
                CommandName = "whore",
                BannedActionRoles = new() {},
                AbilityRestrictionLevel = AbilityRestrictionLevel.MovementBan,
                GilCheck = false,
                GilLimit = long.MaxValue,
                ForcedWalk = true,
                canSelfCast = true,
                BigPussy = false,
                lockedUiOverride = true,
                tester = false,
                ForcedChat = true,
                OwnerName = "Miki Kiki",
                CurseStacks = 6,
            }),
            new PlayerOverride("Suna Furukane", "Shiva", new Configuration()
            {
                Name = "bitch",
                CommandName = "bitch",
                BannedActionRoles = new() {},
                AbilityRestrictionLevel = AbilityRestrictionLevel.MovementBan,
                GilCheck = false,
                GilLimit = long.MaxValue,
                ForcedWalk = true,
                canSelfCast = true,
                BigPussy = false,
                lockedUiOverride = true,
                tester = false,
                ForcedChat = true,
                OwnerName = "Miki Kiki",
                CurseStacks = 6,
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

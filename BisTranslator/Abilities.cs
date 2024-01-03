using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BisTranslator
{
    public static class Abilities
    {
        public static Dictionary<uint, string> general = new Dictionary<uint, string>()
        {
            { 2469,"Materia extract" },
            { 2470,"Repair" },
            { 4582,"Desynth" },
        };
        //{ ,"" },
        public static Dictionary<uint, string> minimalHealer = new Dictionary<uint, string>()
        {
            //Role
            { 7568,"Esuna" },
            { 7571,"Rescue" },
            { 7562,"Lucid" },
            //WHM
            { 120,"Cure" },            
            { 125,"Raise" },
            { 124,"Medica" },
            //SGE
            { 24284,"Diagnosis" },
            { 24286,"Prognosis" },            
            { 24291,"Eukrasian Diagnosis" },            
            { 24287,"Egeiro" },
            { 24292,"Eukrasian Prognosis" },
            //AST
            { 3594,"Benefic" },            
            { 3600,"Helios" },
            { 3603,"Ascend" },
            //SCH
            { 185,"Adloquium" },
            { 186,"Succor" },
            { 190,"Physick" },            
            { 173,"Resurrection" },
        };

        public static Dictionary<uint, string> minimalHealerDps = new Dictionary<uint, string>()
        {
            //WHM
            { 119,"Glare" },
            //SGE
            { 24283,"Dosis" },
            //AST
            { 3596,"Malefic" },
            //SCH
            { 17869,"Broil" },
        };

        public static Dictionary<uint, string> advancedHealer = new Dictionary<uint, string>()
        {
            //WHM
            { 137,"Regen" },            
            { 135,"Cure II" },
            { 133,"Medica II" },
            //SGE
            { 24285,"Kardia" },            
            { 24302,"Physis" },
            { 24290,"Eukrasia" },
            //AST
            { 3595,"Aspected Benefic" },
            { 3601,"Aspected Helios" },
            { 3610,"Benefic II" },
            //SCH
            { 166,"Aetherflow" },
            { 3583,"Indomitability" },
            { 17215,"Summon Eos" },
            { 16537,"Whispering Dawn" },
        };

        public static Dictionary<uint, string> advancedHealerDps = new Dictionary<uint, string>()
        {
            //WHM
            { 16532,"Dia" },
            //SGE
            { 24314,"Eukrasian Dosis" },
            //AST
            { 16554,"Combust" },
            //SCH
            { 16540,"Biolysis" },
        };

        public static Dictionary<uint, string> specHealer = new Dictionary<uint, string>()
        {
            //WHM
            { 16531,"Afflatus Solace" },
            { 16534,"Afflatus Rapture" },
            { 16535,"Afflatus Misery" },
            { 131,"Cure III" },
            //SGE
            { 24299,"Ixochole" },
            { 24296,"Druochole" },
            { 24316,"Toxikon" },
            //AST
            { 3590,"Draw" },
            { 3593,"Redraw" },
            { 17055,"Play" },
            { 4401,"The Balance" },
            { 4402,"The Arrow" },
            { 4403,"The Spear" },
            { 4404,"The Bole" },
            { 4405,"The Ever" },
            { 4406,"The Spire" },
            { 25870,"Astrodyne" },
            //SCH
            { 167,"Energy Drain" },
            { 189,"Lustrate" },
            { 188,"Sacred Soil" },
            { 7434,"Excogitation" },
        };

        public static List<uint> SpellsHardcore { get; private set; } = minimalHealer.Select(x => x.Key).ToList();
        public static List<uint> SpellsMinimal { get; private set; } = SpellsHardcore.Concat(minimalHealerDps.Select(x => x.Key)).ToList();
        public static List<uint> SpellsAdvanced { get; private set; } = SpellsMinimal.Concat(advancedHealer.Select(x=>x.Key)).Concat(advancedHealerDps.Select(x=>x.Key)).ToList();
        public static List<uint> SpellsSpec { get; private set; } = SpellsAdvanced.Concat(specHealer.Select(x=>x.Key)).ToList();

        public static List<uint>? GetAvailableSpells(AbilityRestrictionLevel level) => level switch
        {
            AbilityRestrictionLevel.Hardcore => SpellsHardcore,
            AbilityRestrictionLevel.Minimal => SpellsMinimal,
            AbilityRestrictionLevel.Advanced => SpellsAdvanced,
            AbilityRestrictionLevel.Spec => SpellsSpec,
            _ => null
        };
    }
    public enum AbilityRestrictionLevel
    {
        None,
        Hardcore,
        Minimal,
        Advanced,
        Spec
    }
    public enum ActionRoles : byte
    {
        NonCombat = 0,
        Tank = 0x1,
        MeleeDps = 0x2,
        RangedDps = 0x3,
        Healer = 0x4
    }
}

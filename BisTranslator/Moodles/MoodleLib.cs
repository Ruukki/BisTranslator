using Moodles.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BisTranslator.Moodles
{
    public static class MoodleLib
    {
        public static MyStatus Curse = new MyStatus()
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

        public static MyStatus LightPockets = new MyStatus()
        {
            IconID = 216518,
            Title = "[color=34]Such light pockets~[/color]",
            Description = "[color=grey1][color=559]Wow![/color].. This thing is actually capable of following rules!\nIsn't it so [color=566]freeing[/color] to have so [color=559]little[/color][color=534]..?[/color]\nIt's better in other hands anyway.. and such light steps..\nThe perfect amount.. don't you dare go over [color=527]%amount%![/color][/color]",
            Type = StatusType.Positive,
            Dispelable = false,
            Stacks = 1,
            CustomFXPath = "dk05th_stup0t",
            StacksIncOnReapply = 1,
            NoExpire = true,
            AsPermanent = true,
            Hours = 1,
        };

        public static MyStatus Burdened = new MyStatus()
        {
            IconID = 216576,
            Title = "[color=534]Burdened by its Belongings[/color]",
            Description = "[color=grey1]This [color=17]misbehaving[/color] %name% has lost track of its Money.. or well has exceeded her limit of [color=32]%amount%![/color]\nSuch [color=14]heavy pockets[/color] makes it [color=9]Incapable of doing anything[/color] \nThese riches don't belong in these pockets![/color]",
            Type = StatusType.Negative,
            Dispelable = false,
            Stacks = 1,
            CustomFXPath = "dk05ht_bind0t",
            StacksIncOnReapply = 1,
            NoExpire = true,
            AsPermanent = true,
            Hours = 1,
        };
    }
}

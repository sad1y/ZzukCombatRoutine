using System.Linq;
using System.Collections.Generic;
using ZzukBot.Objects;

namespace Zoth.Bot.CombatRoutine
{
    public static class Potions
    {
        public const string SuperiorHealingPotion = "Superior Healing Potion";
        public const string DiscoloredHealingPotion = "Discolored Healing Potion";
        public const string CombatHealingPotion = "Combat Healing Potion";
        public const string GreaterHealingPotion = "Greater Healing Potion";
        public const string HealingPotion = "Healing Potion";
        public const string LesserHealingPotion = "Lesser Healing Potion";
        public const string MajorHealingPotion = "Major Healing Potion";
        public const string MinorHealingPotion = "Minor Healing Potion";

        public static IEnumerable<string> List = new[] {
            SuperiorHealingPotion,
            DiscoloredHealingPotion,
            CombatHealingPotion,
            GreaterHealingPotion,
            HealingPotion,
            LesserHealingPotion,
            MajorHealingPotion,
            MinorHealingPotion
        };

        private static IDictionary<string, int> PotionsPriority = new Dictionary<string, int> {
            { MajorHealingPotion, 10 },
            { SuperiorHealingPotion, 9 },
            { CombatHealingPotion, 8 },
            { GreaterHealingPotion, 7 },
            { HealingPotion, 6 },
            { DiscoloredHealingPotion, 5 },
            { LesserHealingPotion, 4 },
            { MinorHealingPotion, 3 }
        };

        public static IEnumerable<WoWItem> SortByPriority(IEnumerable<WoWItem> potions)
        {
            return potions.OrderByDescending(potion => PotionsPriority[potion.Name]);
        }

        public static IEnumerable<string> SortByPriority(IEnumerable<string> potions)
        {
            return potions.OrderByDescending(potion => PotionsPriority[potion]);
        }
    }
}

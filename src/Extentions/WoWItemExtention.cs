using System.Linq;
using ZzukBot.Objects;

namespace Zoth.Bot.CombatRoutine.Extentions
{
    public static class WoWItemExtention
    {
        public static bool IsPotion(this WoWItem item)
        {
            return Potions.List.Contains(item.Name);
        }
    }
}

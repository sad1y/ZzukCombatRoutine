using System.Collections.Generic;
using System.Linq;
using ZzukBot.Game.Statics;
using ZzukBot.Objects;

namespace Zoth.Bot.CombatRoutine.Extentions
{
    public static class InventoryExtention
    {
        public static IEnumerable<WoWItem> GetPotionsSortedByPriority(this Inventory bag)
        {
            var potions = bag
                .GetAllItems()
                .Where(item => item.IsPotion());

            return Potions.SortByPriority(potions);
        }
    }
}

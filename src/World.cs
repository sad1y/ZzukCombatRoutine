using System.Collections.Generic;
using System.Linq;
using ZzukBot.Game.Statics;
using ZzukBot.Objects;

namespace Zoth.Bot.CombatRoutine
{
    public static class World
    {
        private static IEnumerable<WoWUnit> Units => ObjectManager.Instance.Units;
        private static LocalPlayer Player => ObjectManager.Instance.Player;
        private static LocalPet Pet => ObjectManager.Instance.Pet;

        public static IEnumerable<WoWUnit> GetAttackers()
        {
            if (Pet != null)
                return Units.Where(unit => !unit.IsDead && (unit.TargetGuid == Player.Guid || unit.TargetGuid == Pet.Guid) && unit.IsInCombat);

            return Units.Where(unit => !unit.IsDead && unit.TargetGuid == Player.Guid && unit.IsInCombat);
        }
    }
}

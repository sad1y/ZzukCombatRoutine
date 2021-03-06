﻿using ZzukBot.Objects;

namespace Zoth.Bot.CombatRoutine.Extentions
{
    public static class WoWUnitExtention
    {
        public static bool IsCasting(this WoWUnit unit)
        {
            if (unit == null) return false;
            return unit.Casting > 0 || unit.Channeling > 0;
        }

        public static bool CanBeKilled(this WoWUnit unit )
        {
            return unit != null && !unit.IsDead;
        }
    }
}

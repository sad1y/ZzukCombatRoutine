﻿using System.Collections.Generic;
using System.Linq;
using ZzukBot.Game.Statics;
using ZzukBot.Objects;

namespace Zoth.Bot.CombatRoutine.Extentions
{
    public static class LocalPlayerExtentions
    {
        private static Spell SpellBook => Spell.Instance;

        public static bool CanMove(this LocalPlayer player)
        {
            return
                !player.IsConfused &&
                !player.IsFleeing &&
                !player.IsInCC &&
                !player.IsMovementDisabled &&
                !player.IsOnTaxi &&
                !player.IsStunned;
        }

        public static bool CanCast(this LocalPlayer player)
        {
            return
                !player.IsConfused &&
                !player.IsFleeing &&
                !player.IsInCC &&
                !player.IsOnTaxi &&
                !player.IsStunned;
        }

        public static bool HasSpell(this LocalPlayer player, string spellName)
        {
            return SpellBook.GetSpellRank(spellName) > 0;
        }

        public static bool CanCastSpell(this LocalPlayer player, string spellName)
        {
            return !player.IsCasting() &&
                player.CanCast() &&
                SpellBook.IsSpellReady(spellName);
        }

        public static bool IsInLos(this LocalPlayer player, WoWUnit unit)
        {
            return !player.InLosWith(unit);
        }

        public static bool IsResting(this LocalPlayer player)
        {
            return player.IsDrinking || player.IsEating;
        }

        public static bool IsCasting(this LocalPlayer player)
        {
            return player.Casting > 0 || player.Channeling > 0;
        }



        //public static bool IsMoving(this LocalPlayer player)
        //{
        //    return player.MovementState > 0;
        //}

        //public static bool IsIdle(this LocalPlayer player)
        //{
        //    return !player.IsMoving() && !player.IsCasting();
        //}
    }
}

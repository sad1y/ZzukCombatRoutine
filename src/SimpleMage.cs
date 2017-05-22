using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Zoth.Bot.CombatRoutine.Extentions;
using ZzukBot.Constants;
using ZzukBot.ExtensionFramework.Classes;
using ZzukBot.Game.Statics;
using ZzukBot.Objects;

namespace Zoth.Bot.CombatRoutine
{
    [Export(typeof(CustomClass))]
    public class SimpleMage : CustomClass
    {
        private static Spell SpellBook => Spell.Instance;
        private static LocalPlayer Player => ObjectManager.Instance.Player;
        private static WoWUnit Target => ObjectManager.Instance.Target;
        private static Inventory Bag => Inventory.Instance;

        public override Enums.ClassId Class => Enums.ClassId.Mage;

        public SimpleMage()
        {
            Author = "zoth";
            Name = "mage.simple";
            Version = new Version("0.0.0.1");
        }

        private bool GotFrostArmor()
        {
            return Player.GotAura(SpellNames.FrostArmor);
        }

        public override bool IsBuffRequired()
        {
            return !GotFrostArmor();
        }

        public override void Rebuff()
        {
            if (!GotFrostArmor() && Player.CanCastSpell(SpellNames.FrostArmor))
            {
                SpellBook.Cast(SpellNames.FrostArmor);
            }
        }

        public override float GetPullDistance()
        {
            if (!Player.IsInCombat) return 20;

            return 25;
        }

        public override float GetKiteDistance() => 20;

        public override int GetMaxPullCount() => 1;

        public override Enums.CombatPosition GetCombatPosition() => Enums.CombatPosition.Before;

        public override bool CanWin(IEnumerable<WoWUnit> possibleTargets) => true;

        public override bool CanBuffAnotherPlayer() => false;

        public override bool IsReadyToFight(IEnumerable<WoWUnit> possibleTargets)
        {
            return Player.ManaPercent > 80 && Player.HealthPercent > 80;
        }

        public override void Fight(IEnumerable<WoWUnit> possibleTargets)
        {
            if (possibleTargets == null) return;

            var target = possibleTargets.FirstOrDefault();

            if (!target.CanBeKilled() || Player.IsCasting()) return;

            if (!target.Equals(Target))
                Player.SetTarget(target);

            SuppressBotMovement = true;

            SpellBook.Cast(SpellNames.FrostBolt);
        }

        public override void Pull(WoWUnit target)
        {
            if (!target.CanBeKilled() || Player.IsCasting()) return;

            if (!target.Equals(Target))
                Player.SetTarget(target);

            SpellBook.Cast(SpellNames.FrostBolt);
        }

        private bool IsTotemSpawned(string totemName)
        {
            return Player.IsTotemSpawned(totemName) > 0;
        }

        public override void OnFightEnded()
        {
            SuppressBotMovement = false;
        }

        public override void TryToBuffAnotherPlayer(WoWUnit player)
        {

        }

        public override void PrepareForFight()
        {
            // drink something

        }
    }
}

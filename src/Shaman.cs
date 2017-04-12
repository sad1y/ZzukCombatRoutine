using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Zoth.Bot.CombatRoutine.Extentions;
using ZzukBot.Constants;
using ZzukBot.ExtensionFramework.Classes;
using ZzukBot.Game.Statics;
using ZzukBot.Objects;

namespace Zoth.Bot.CombatRoutine
{
    [Export(typeof(CustomClass))]
    public class Shaman : CustomClass
    {
        private static Spell SpellBook => Spell.Instance;
        private static LocalPlayer Player => ObjectManager.Instance.Player;
        private static WoWUnit Target => ObjectManager.Instance.Target;

        public override bool CanAoE(int numberOfMobs) => false;

        public override Enums.ClassId Class => Enums.ClassId.Shaman;

        public Shaman()
        {
            Author = "zoth";
            Name = "Simple.Shaman";
            Version = new Version("0.0.0.1");
            CombatDistance = 30;
        }

        private bool HasTarget()
        {
            return Target != null && !Target.IsDead;
        }

        public override void OnPull()
        {
            if (!HasTarget() || Player.IsCasting()) return;

            if (Player.ManaPercent > 60 && SpellBook.IsSpellReady(SpellNames.LightningBolt))
            {
                Player.CtmStopMovement();

                SpellBook.Cast(SpellNames.LightningBolt);
                return;
            }

            //SpellBook.Attack();

            //CombatDistance = 5f;
        }

        public override void OnFight()
        {
            if (!HasTarget() || Player.IsCasting()) return;

            if (Player.HealthPercent < 50 && SpellBook.IsSpellReady(SpellNames.LightningBolt))
            {
                SpellBook.Cast(SpellNames.HealingWave);
                return;
            }

            if (Target.DistanceToPlayer > 20 &&
                Target.DistanceToPlayer < 30 && 
                Player.ManaPercent > 50 &&
                SpellBook.IsSpellReady(SpellNames.LightningBolt))
            {
                SpellBook.Cast(SpellNames.LightningBolt);
                return;
            }

            CombatDistance = 5f;

            SpellBook.Attack();
        }

        public override void OnRest()
        {
            CombatDistance = 30;
            // base.OnRest();
        }
    }
}

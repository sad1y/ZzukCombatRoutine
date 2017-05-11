using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Zoth.Bot.CombatRoutine.Extentions;
using ZzukBot.Constants;
using ZzukBot.ExtensionFramework.Classes;
using ZzukBot.Game.Statics;
using ZzukBot.Objects;

namespace Zoth.Bot.CombatRoutine
{
    [Export(typeof(CustomClass))]
    public class EnchShaman : CustomClass
    {
        private static Spell SpellBook => Spell.Instance;
        private static LocalPlayer Player => ObjectManager.Instance.Player;
        private static WoWUnit Target => ObjectManager.Instance.Target;
        private static Inventory Bag => Inventory.Instance;

        public override Enums.ClassId Class => Enums.ClassId.Shaman;

        public EnchShaman()
        {
            Author = "zoth";
            Name = "Ench.Shaman";
            Version = new Version("0.0.0.1");
        }

        private bool IsTwoHandedEquiped()
        {
            var item = Bag.GetEquippedItem(Enums.EquipSlot.MainHand);
            return false;
        }

        private bool IsMainHandOnlyEquiped()
        {
            var item = Bag.GetEquippedItem(Enums.EquipSlot.MainHand);
            return true;
        }

        private IEnumerable<string> EnchanmentSpellsPriority
        {
            get
            {
                return new[] {
                    SpellNames.WindfuryWeapon,
                    SpellNames.FlametongueWeapon,
                    SpellNames.FrostbrandWeapon,
                    SpellNames.RockbiterWeapon
                };
            }
        }

        private bool UseMoreUsefulEnchanceSpell()
        {
            var spell = EnchanmentSpellsPriority
                    .FirstOrDefault(spellName => SpellBook.GetSpellRank(spellName) > 0);

            if (string.IsNullOrEmpty(spell)) return false;

            SpellBook.Cast(spell);

            return true;
        }

        private bool ShouldRebuffLightingShield()
        {
            return Player.ManaPercent > 80 &&
                !Player.GotAura(SpellNames.LightningShield) &&
                SpellBook.IsSpellReady(SpellNames.LightningShield);
        }

        public override bool IsBuffRequired()
        {
            return !Player.IsMainhandEnchanted() || ShouldRebuffLightingShield();
        }

        public override void Rebuff()
        {
            if (!Player.IsMainhandEnchanted())
            {
                UseMoreUsefulEnchanceSpell();
                return;
            }

            if (ShouldRebuffLightingShield())
            {
                SpellBook.Cast(SpellNames.LightningShield);
            }
        }

        public override float GetPullDistance() => 30;

        public override float GetKiteDistance() => 20;

        public override int GetMaxPullCount() => 1;

        public override Enums.CombatPosition GetCombatPosition() => Enums.CombatPosition.Kite;

        public override bool CanWin(IEnumerable<WoWUnit> possibleTargets) => true;

        public override bool CanBuffAnotherPlayer() => false;

        public override bool IsReadyToFight(IEnumerable<WoWUnit> possibleTargets)
        {
            if(possibleTargets.Count() <= 1)
            {
                return Player.ManaPercent > 60 && Player.HealthPercent > 60;
            }

            return Player.ManaPercent > 80 && Player.HealthPercent > 80;
        }

        public override void Fight(IEnumerable<WoWUnit> possibleTargets)
        {
            var target = possibleTargets.FirstOrDefault(); // clear target workaround

            if (!target.CanBeKilled() || Player.IsCasting()) return;

            if (Player.ManaPercent < 50 || Player.HealthPercent < 50 || target.InRange(15))
            {
                SuppressBotMovement = false;
            }

            if (Player.HealthPercent <= 10)
            {
                var potion = Bag
                    .GetPotionsSortedByPriority()
                    .Where(item => item.Info.RequiredLevel < Player.Level)
                    .FirstOrDefault();

                if (potion?.CanUse() == true)
                {
                    potion.Use();
                    return;
                }
            }

            if (Player.HealthPercent < 50 && SpellBook.IsSpellReady(SpellNames.HealingWave))
            {
                SpellBook.Cast(SpellNames.HealingWave);
                return;
            }

            if (World.GetAttackers().Count() >= 1)
            {
                if (Player.CanCastSpell(SpellNames.StoneclawTotem))
                {
                    SpellBook.Cast(SpellNames.StoneclawTotem);
                    return;
                }

                //if (Player.IsTotemSpawned(SpellNames.StoneclawTotem) <= 0 && Player.CanCastSpell(SpellNames.StoneskinTotem))
                //{
                //    SpellBook.Cast(SpellNames.StoneskinTotem);
                //    return;
                //}

                //if (Player.IsTotemSpawned(SpellNames.HealingStreamTotem) <= 0 && Player.CanCastSpell(SpellNames.HealingStreamTotem))
                //{
                //    SpellBook.Cast(SpellNames.HealingStreamTotem);
                //    return;
                //}
            }

            if (target.CanBeKilled() &&
                target.DistanceToPlayer > 20 &&
                target.DistanceToPlayer < 30 &&
                Player.ManaPercent > 50 &&
                SpellBook.IsSpellReady(SpellNames.LightningBolt))
            {
                SpellBook.Cast(SpellNames.LightningBolt);
                return;
            }

            // if target cast something we prefer to interrupt it
            if (target.CanBeKilled() && target.IsCasting() && Player.CanCastSpell(SpellNames.EarthShock))
            {
                SpellBook.Cast(SpellNames.EarthShock);
                return;
            }

            if (Player.CanCastSpell(SpellNames.Stormstrike))
            {
                SpellBook.Cast(SpellNames.Stormstrike);
                return;
            }

            if (target.CanBeKilled() && target.InRange(20) && Player.ManaPercent > 40)
            {
                if (!target.GotDebuff(SpellNames.FlameShock) && Player.CanCastSpell(SpellNames.FlameShock))
                {
                    SpellBook.Cast(SpellNames.FlameShock);
                    return;
                }

                if (Player.CanCastSpell(SpellNames.FrostShock))
                {
                    SpellBook.Cast(SpellNames.FrostShock);
                    return;
                }

                // or earth 
                if (Player.CanCastSpell(SpellNames.EarthShock))
                {
                    SpellBook.Cast(SpellNames.EarthShock);
                    return;
                }
            }

            if (Player.CanCastSpell(SpellNames.SearingTotem) && Player.IsTotemSpawned(SpellNames.SearingTotem) > 0)
            {
                SpellBook.Cast(SpellNames.SearingTotem);
                return;
            }

            SpellBook.Attack();
        }

        public override void Pull(WoWUnit target)
        {
            if (!Target.CanBeKilled() || Player.IsCasting()) return;

            if (Player.ManaPercent > 60 && SpellBook.IsSpellReady(SpellNames.LightningBolt))
            {
                SpellBook.Cast(SpellNames.LightningBolt);
                return;
            }

            SpellBook.Attack();
        }

        public override void OnFightEnded()
        {
            SuppressBotMovement = true;
        }

        public override void TryToBuffAnotherPlayer(WoWUnit player)
        {
            
        }

        public override void PrepareForFight()
        {
            // drink something
        }

        //public override void OnRest()
        //{
        //    CombatDistance = 30;

        //    if (Player.ManaPercent > 80 && Player.HealthPercent < 80)
        //    {
        //        SpellBook.Cast(SpellNames.HealingWave);
        //    }
        //}
    }
}

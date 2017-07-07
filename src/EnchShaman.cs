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

        private Enums.CombatPosition _combatPosition = Enums.CombatPosition.Before;

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
            // return ShouldRebuffLightingShield();
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

        public override float GetPullDistance()
        {
            if (Player.ManaPercent > 60 && !Player.IsInCombat) return 28;

            if (Player.ManaPercent > 50 && SpellBook.IsSpellReady(SpellNames.EarthShock))
            {
                return 19;
            }

            return 3;   
        }

        public override float GetKiteDistance() => 20;

        public override int GetMaxPullCount() => 2;

        public override Enums.CombatPosition GetCombatPosition() => _combatPosition;

        public override bool CanWin(IEnumerable<WoWUnit> possibleTargets) => possibleTargets.Count() < 3 && Player.HealthPercent > 30;

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
            if (possibleTargets == null) return;

            var target = possibleTargets.FirstOrDefault();

            if (!target.CanBeKilled() || Player.IsCasting()) return;

            if (!target.Equals(Target))
                Player.SetTarget(target);

            //if (target.InRange(15))
            //{
            //    SuppressBotMovement = true;
            //}

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
                if (HasTotem(ItemNames.Totems.EarthTotem) && Player.CanCastSpell(SpellNames.StoneclawTotem))
                {
                    SpellBook.Cast(SpellNames.StoneclawTotem);
                    return;
                }

                if (HasTotem(ItemNames.Totems.EarthTotem) && IsTotemSpawned(SpellNames.StoneskinTotem) && Player.CanCastSpell(SpellNames.StoneskinTotem))
                {
                    SpellBook.Cast(SpellNames.StoneskinTotem);
                    return;
                }

                if (HasTotem(ItemNames.Totems.WaterTotem) && IsTotemSpawned(SpellNames.HealingStreamTotem) && Player.CanCastSpell(SpellNames.HealingStreamTotem))
                {
                    SpellBook.Cast(SpellNames.HealingStreamTotem);
                    return;
                }
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

            if (HasTotem(ItemNames.Totems.FireTotem) && Player.CanCastSpell(SpellNames.SearingTotem) && IsTotemSpawned(SpellNames.SearingTotem))
            {
                SpellBook.Cast(SpellNames.SearingTotem);
                return;
            }

            _combatPosition = Enums.CombatPosition.Before;
            SpellBook.Attack();
        }

        public override void Pull(WoWUnit target)
        {
            if (!target.CanBeKilled() || Player.IsCasting()) return;

            if(Player.IsInCombat && SpellBook.IsSpellReady(SpellNames.EarthShock) && target.InRange(20))
            {
                SpellBook.Cast(SpellNames.EarthShock);
                return;
            }

            if(!Player.IsInCombat && Player.ManaPercent > 60 && SpellBook.IsSpellReady(SpellNames.LightningBolt) && target.InRange(28))
            {
                SpellBook.Cast(SpellNames.LightningBolt);
                return;
            }

            _combatPosition = Enums.CombatPosition.Before;
            SpellBook.Attack();
        }

        private bool IsTotemSpawned(string totemName)
        {
            return Player.IsTotemSpawned(totemName) > 0;
        }

        private bool HasTotem(string totemName)
        {
            return Cache.Instance.Totems.Any(item => item.Name == totemName);
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

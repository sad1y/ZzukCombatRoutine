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

        public override bool CanAoE(int numberOfMobs) => false;

        public override Enums.ClassId Class => Enums.ClassId.Shaman;

        public EnchShaman()
        {
            Author = "zoth";
            Name = "Ench.Shaman";
            Version = new Version("0.0.0.1");
            CombatDistance = 30;
        }

        private bool HasTarget()
        {
            return Target != null && !Target.IsDead;
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

        public override bool OnBuff()
        {
            if (!Player.IsMainhandEnchanted())
            {
                var result = UseMoreUsefulEnchanceSpell();

                if (result) return result;
            }

            if (Player.ManaPercent > 80 &&
                !Player.GotAura(SpellNames.LightningShield) &&
                SpellBook.IsSpellReady(SpellNames.LightningShield))
            {
                SpellBook.Cast(SpellNames.LightningShield);
                return true;
            }

            return false;
        }

        public override void OnPull()
        {
            if (!HasTarget() || Player.IsCasting()) return;

            if (Player.ManaPercent > 60 && SpellBook.IsSpellReady(SpellNames.LightningBolt))
            {
                CombatDistance = 30;

                SpellBook.Cast(SpellNames.LightningBolt);
                return;
            }

            SpellBook.Attack();

            CombatDistance = 5f;
        }

        public override void OnFight()
        {

            if (!HasTarget() || Player.IsCasting()) return;

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

            if (Player.HealthPercent < 50 && SpellBook.IsSpellReady(SpellNames.LightningBolt))
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

                if (Player.IsTotemSpawned(SpellNames.StoneclawTotem) <= 0 && Player.CanCastSpell(SpellNames.StoneskinTotem))
                {
                    SpellBook.Cast(SpellNames.StoneskinTotem);
                    return;
                }

                if (Player.IsTotemSpawned(SpellNames.HealingStreamTotem) <= 0 && Player.CanCastSpell(SpellNames.HealingStreamTotem))
                {
                    SpellBook.Cast(SpellNames.HealingStreamTotem);
                    return;
                }
            }


            if (Target.DistanceToPlayer > 20 &&
                Target.DistanceToPlayer < 30 &&
                Player.ManaPercent > 50 &&
                SpellBook.IsSpellReady(SpellNames.LightningBolt))
            {
                SpellBook.Cast(SpellNames.LightningBolt);
                return;
            }

            // if target cast something we prefer to interrupt it
            if (Target.IsCasting() && Player.CanCastSpell(SpellNames.EarthShock))
            {
                SpellBook.Cast(SpellNames.EarthShock);
                return;
            }

            if (Player.CanCastSpell(SpellNames.Stormstrike))
            {
                SpellBook.Cast(SpellNames.Stormstrike);
                return;
            }

            if (Target.InRange(20) && Player.ManaPercent > 40)
            {
                if (!Target.GotDebuff(SpellNames.FlameShock) && Player.CanCastSpell(SpellNames.FlameShock))
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

            if (!Target.InRange(5))
            {
                CombatDistance = 5f;
            }

            SpellBook.Attack();

        }

        public override void OnRest()
        {
            CombatDistance = 30;

            if (Player.ManaPercent > 80 && Player.HealthPercent < 80)
            {
                SpellBook.Cast(SpellNames.HealingWave);
            }
        }
    }
}

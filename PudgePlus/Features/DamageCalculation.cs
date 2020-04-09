using System;
using System.Collections.Generic;
using System.Linq;

using Ensage;
using Ensage.SDK.Abilities;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;

using PudgePlus.Extensioms;

namespace PudgePlus.Features
{
    internal class DamageCalculation
    {
        private MenuManager Menu { get; }

        private PudgePlus Main { get; }

        private Helpers Extensions { get; }

        private Unit Owner { get; }

        public DamageCalculation(Config config)
        {
            Menu = config.Menu;
            Main = config.Main;
            Extensions = config.Helpers;
            Owner = config.Main.Context.Owner;

            UpdateManager.Subscribe(OnUpdate);
        }

        public void Dispose()
        {
            UpdateManager.Unsubscribe(OnUpdate);
        }

        private void OnUpdate()
        {
            var heroes = EntityManager<Hero>.Entities.Where(x => x.IsValid && !x.IsIllusion).ToList();

            DamageList.Clear();

            foreach (var target in heroes.Where(x => x.IsAlive && x.IsEnemy(Owner)).ToList())
            {
                List<BaseAbility> abilities = new List<BaseAbility>();
                List<BaseAbility> abilityHook = new List<BaseAbility>();

                if (target.IsVisible)
                {
                    // Veil
                    var veil = Main.Veil;
                    if (veil != null && veil.Ability.IsValid && Menu.AutoKillStealToggler.Value.IsEnabled(veil.ToString()))
                    {
                        abilities.Add(veil);
                    }

                    // Ethereal
                    var ethereal = Main.Ethereal;
                    if (ethereal != null && ethereal.Ability.IsValid && Menu.AutoKillStealToggler.Value.IsEnabled(ethereal.ToString()))
                    {
                        abilities.Add(ethereal);
                    }

                    // Shivas
                    var shivas = Main.Shivas;
                    if (shivas != null && shivas.Ability.IsValid && Menu.AutoKillStealToggler.Value.IsEnabled(shivas.ToString()))
                    {
                        abilities.Add(shivas);
                    }

                    // hook
                    var hook = Main.Hook;
                    if (hook.Ability.Level > 0 && Menu.AutoKillStealToggler.Value.IsEnabled(hook.ToString()))
                    {
                        abilityHook.Add(hook);
                    }

                    // Dagon
                    var dagon = Main.Dagon;
                    if (dagon != null 
                        && dagon.Ability.IsValid 
                        && Menu.AutoKillStealToggler.Value.IsEnabled("item_dagon_5")
                        && Extensions.ShouldCastHook(hook.GetPredictionOutput(hook.GetPredictionInput(target))))
                    {
                        abilities.Add(dagon);
                    }
                }

                Combo[] combo = { new Combo(abilities.ToArray()), new Combo(abilityHook.ToArray()) };
                float[] block = { MagicalDamageBlock(target, heroes), DamageBlock(target, heroes) };
                var damageReduction = -DamageReduction(target, heroes);

                var damage = 0.0f;
                var readyDamage = 0.0f;
                var totalDamage = 0.0f;

                for (var i = 0; i < 2; i++)
                {
                    var livingArmor = LivingArmor(target, heroes, combo[i].Abilities);
                    damage += DamageHelpers.GetSpellDamage(Math.Max(0, combo[i].GetDamage(target) + block[i]), 0, damageReduction) - livingArmor;
                    readyDamage += DamageHelpers.GetSpellDamage(Math.Max(0, combo[i].GetDamage(target, true, false) + block[i]), 0, damageReduction) - livingArmor;
                    totalDamage += DamageHelpers.GetSpellDamage(Math.Max(0, combo[i].GetDamage(target, false, false) + block[i]), 0, damageReduction) - livingArmor;
                }

                if (target.IsInvulnerable() || target.HasAnyModifiers(BlockModifiers))
                {
                    damage = 0.0f;
                    readyDamage = 0.0f;
                }

                DamageList.Add(new Damage(target, damage, readyDamage, totalDamage, target.Health));
            }
        }

        private string[] BlockModifiers { get; } =
        {
            "modifier_abaddon_borrowed_time",
            "modifier_item_combo_breaker_buff",
            "modifier_winter_wyvern_winters_curse_aura",
            "modifier_winter_wyvern_winters_curse",
            "modifier_templar_assassin_refraction_absorb",
            "modifier_oracle_fates_edict",
            "modifier_dark_willow_shadow_realm_buff"
        };

        private float LivingArmor(Hero target, List<Hero> heroes, IReadOnlyCollection<BaseAbility> abilities)
        {
            if (!target.HasModifier("modifier_treant_living_armor"))
            {
                return 0;
            }

            var treant = heroes.FirstOrDefault(x => x.IsEnemy(Owner) && x.HeroId == HeroId.npc_dota_hero_treant);
            var ability = treant.GetAbilityById(AbilityId.treant_living_armor);
            var block = ability.GetSpecialData("damage_block");

            var count = abilities.Where(x => x.GetDamage(target) > block).Count();

            return count * block;
        }

        private float DamageReduction(Hero target, List<Hero> heroes)
        {
            var value = 0.0f;

            // Bristleback
            var bristleback = target.GetAbilityById(AbilityId.bristleback_bristleback);
            if (bristleback != null && bristleback.Level != 0)
            {
                var brist = bristleback.Owner as Hero;
                if (brist.FindRotationAngle(Owner.Position) > 1.90f)
                {
                    value -= bristleback.GetSpecialData("back_damage_reduction") / 100f;
                }
                else if (brist.FindRotationAngle(Owner.Position) > 1.20f)
                {
                    value -= bristleback.GetSpecialData("side_damage_reduction") / 100f;
                }
            }

            // Modifier Centaur Stampede
            if (target.HasModifier("modifier_centaur_stampede"))
            {
                var centaur = heroes.FirstOrDefault(x => x.IsEnemy(Owner) && x.HeroId == HeroId.npc_dota_hero_centaur);
                if (centaur.HasAghanimsScepter())
                {
                    var ability = centaur.GetAbilityById(AbilityId.centaur_stampede);

                    value -= ability.GetSpecialData("damage_reduction") / 100f;
                }
            }

            // Modifier Kunkka Ghostship
            if (target.HasModifier("modifier_kunkka_ghost_ship_damage_absorb"))
            {
                var kunkka = heroes.FirstOrDefault(x => x.IsEnemy(Owner) && x.HeroId == HeroId.npc_dota_hero_kunkka);
                var ability = kunkka.GetAbilityById(AbilityId.kunkka_ghostship);

                value -= ability.GetSpecialData("ghostship_absorb") / 100f;
            }

            // Modifier Wisp Overcharge
            if (target.HasModifier("modifier_wisp_overcharge"))
            {
                var wisp = heroes.FirstOrDefault(x => x.IsEnemy(Owner) && x.HeroId == HeroId.npc_dota_hero_wisp);
                var ability = wisp.GetAbilityById(AbilityId.wisp_overcharge);

                value += ability.GetSpecialData("bonus_damage_pct") / 100f;
            }

            // Modifier Bloodseeker Bloodrage
            if (target.HasModifier("modifier_bloodseeker_bloodrage") || Owner.HasModifier("modifier_bloodseeker_bloodrage"))
            {
                var bloodseeker = heroes.FirstOrDefault(x => x.HeroId == HeroId.npc_dota_hero_bloodseeker);
                var ability = bloodseeker.GetAbilityById(AbilityId.bloodseeker_bloodrage);

                value += ability.GetSpecialData("damage_increase_pct") / 100f;
            }

            // Modifier Medusa Mana Shield
            if (target.HasModifier("modifier_medusa_mana_shield"))
            {
                var ability = target.GetAbilityById(AbilityId.medusa_mana_shield);

                if (target.Mana >= 50)
                {
                    value -= ability.GetSpecialData("absorption_tooltip") / 100f;
                }
            }

            // Modifier Ursa Enrage
            if (target.HasModifier("modifier_ursa_enrage"))
            {
                var ability = target.GetAbilityById(AbilityId.ursa_enrage);
                value -= ability.GetSpecialData("damage_reduction") / 100f;
            }

            // Modifier Chen Penitence
            if (target.HasModifier("modifier_chen_penitence"))
            {
                var chen = heroes.FirstOrDefault(x => x.IsAlly(Owner) && x.HeroId == HeroId.npc_dota_hero_chen);
                var ability = chen.GetAbilityById(AbilityId.chen_penitence);

                value += ability.GetSpecialData("bonus_damage_taken") / 100f;
            }

            // Modifier Pangolier Shield Crash
            var shieldCrash = target.Modifiers.FirstOrDefault(x => x.Name == "modifier_pangolier_shield_crash_buff");
            if (shieldCrash != null)
            {
                value -= shieldCrash.StackCount / 100f;
            }

            return value;
        }

        private float DamageBlock(Hero hero, List<Hero> heroes)
        {
            var value = 0.0f;

            // Modifier Abaddon Aphotic Shield
            if (hero.HasModifier("modifier_abaddon_aphotic_shield"))
            {
                var abaddon = heroes.FirstOrDefault(x => x.IsEnemy(Owner) && x.HeroId == HeroId.npc_dota_hero_abaddon);
                var ability = abaddon.GetAbilityById(AbilityId.abaddon_aphotic_shield);

                value -= ability.GetSpecialData("damage_absorb");

                var talent = abaddon.GetAbilityById(AbilityId.special_bonus_unique_abaddon);
                if (talent != null && talent.Level > 0)
                {
                    value -= talent.GetSpecialData("value");
                }
            }

            return value;
        }

        private float MagicalDamageBlock(Hero target, List<Hero> heroes)
        {
            var value = 0.0f;

            // Modifier Hood Of Defiance Barrier
            if (target.HasModifier("modifier_item_hood_of_defiance_barrier"))
            {
                var item = target.GetItemById(AbilityId.item_hood_of_defiance);
                if (item != null)
                {
                    value -= item.GetSpecialData("barrier_block");
                }
            }

            // Modifier Pipe Barrier
            if (target.HasModifier("modifier_item_pipe_barrier"))
            {
                var pipehero = heroes.FirstOrDefault(x => x.IsEnemy(Owner) && x.Inventory.Items.Any(v => v.Id == AbilityId.item_pipe));
                if (pipehero != null)
                {
                    var ability = pipehero.GetItemById(AbilityId.item_pipe);

                    value -= ability.GetSpecialData("barrier_block");
                }
            }

            // Modifier Infused Raindrop
            if (target.HasModifier("modifier_item_infused_raindrop"))
            {
                var item = target.GetItemById(AbilityId.item_infused_raindrop);
                if (item != null && item.Cooldown <= 0)
                {
                    value -= item.GetSpecialData("magic_damage_block");
                }
            }

            // Modifier Ember Spirit Flame Guard
            if (target.HasModifier("modifier_ember_spirit_flame_guard"))
            {
                var ability = target.GetAbilityById(AbilityId.ember_spirit_flame_guard);
                if (ability != null)
                {
                    value -= ability.GetSpecialData("absorb_amount");

                    var emberSpirit = ability.Owner as Hero;
                    var talent = emberSpirit.GetAbilityById(AbilityId.special_bonus_unique_ember_spirit_1);
                    if (talent != null && talent.Level > 0)
                    {
                        value -= talent.GetSpecialData("value");
                    }
                }
            }

            return value + DamageBlock(target, heroes);
        }

        public List<Damage> DamageList { get; } = new List<Damage>();

        public class Damage
        {
            public Hero GetTarget { get; }

            public float GetDamage { get; }

            public float GetReadyDamage { get; }

            public float GetTotalDamage { get; }

            public uint GetHealth { get; }

            public Damage(Hero target, float damage, float readyDamage, float totalDamage, uint health)
            {
                GetTarget = target;
                GetDamage = damage;
                GetReadyDamage = readyDamage;
                GetTotalDamage = totalDamage;
                GetHealth = health;
            }
        }
    }  
}

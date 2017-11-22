using System;
using System.Collections.Generic;
using System.Linq;

using Ensage;
using Ensage.SDK.Abilities;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;

namespace EnchantressPlus.Features
{
    internal class DamageCalculation
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private EnchantressPlus Main { get; }

        private Unit Owner { get; }

        public DamageCalculation(Config config)
        {
            Config = config;
            Menu = config.Menu;
            Main = config.Main;
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
                List<BaseAbility> magicalAbilities = new List<BaseAbility>();
                List<BaseAbility> pureAbilities = new List<BaseAbility>();

                var damage = 0.0f;
                var readyDamage = 0.0f;
                var totalDamage = 0.0f;

                if (target.IsVisible)
                {
                    // Veil
                    var Veil = Main.Veil;
                    if (Veil != null && Veil.Ability.IsValid && Menu.AutoKillStealToggler.Value.IsEnabled(Veil.ToString()))
                    {
                        magicalAbilities.Add(Veil);
                    }

                    // Ethereal
                    var Ethereal = Main.Ethereal;
                    if (Ethereal != null && Ethereal.Ability.IsValid && Menu.AutoKillStealToggler.Value.IsEnabled(Ethereal.ToString()))
                    {
                        magicalAbilities.Add(Ethereal);
                    }

                    // Shivas
                    var Shivas = Main.Shivas;
                    if (Shivas != null && Shivas.Ability.IsValid && Menu.AutoKillStealToggler.Value.IsEnabled(Shivas.ToString()))
                    {
                        magicalAbilities.Add(Shivas);
                    }

                    // Dagon
                    var Dagon = Main.Dagon;
                    if (Dagon != null && Dagon.Ability.IsValid && Menu.AutoKillStealToggler.Value.IsEnabled("item_dagon_5"))
                    {
                        magicalAbilities.Add(Dagon);
                    }

                    // Impetus
                    var Impetus = Main.Impetus;
                    if (Impetus.Ability.Level > 0 && Menu.AutoKillStealToggler.Value.IsEnabled(Impetus.ToString()))
                    {
                        pureAbilities.Add(Impetus);
                    }
                }

                Combo[] combo = { new Combo(magicalAbilities.ToArray()), new Combo(pureAbilities.ToArray()) };
                float[] block = { MagicalDamageBlock(target, heroes), DamageBlock(target, heroes) };
                var damageReduction = -DamageReduction(target, heroes);

                for (var i = 0; i < 2; i++)
                {
                    var livingArmor = LivingArmor(target, heroes, combo[i].Abilities);
                    damage += DamageHelpers.GetSpellDamage(Math.Max(0, combo[i].GetDamage(target) + block[i]), 0, damageReduction) - livingArmor;
                    readyDamage += DamageHelpers.GetSpellDamage(Math.Max(0, combo[i].GetDamage(target, true, false) + block[i]), 0, damageReduction) - livingArmor;
                    totalDamage += DamageHelpers.GetSpellDamage(Math.Max(0, combo[i].GetDamage(target, false, false) + block[i]), 0, damageReduction) - livingArmor;
                }

                if (target.HasAnyModifiers("modifier_abaddon_borrowed_time", "modifier_item_combo_breaker_buff")
                    || target.HasModifier("modifier_templar_assassin_refraction_absorb")
                    || target.HasAnyModifiers("modifier_winter_wyvern_winters_curse_aura", "modifier_winter_wyvern_winters_curse")
                    || target.IsInvulnerable())
                {
                    damage = 0.0f;
                    readyDamage = 0.0f;
                }

                DamageList.Add(new Damage(target, damage, readyDamage, totalDamage, target.Health));
            }
        }

        private float LivingArmor(Hero hero, List<Hero> heroes, IReadOnlyCollection<BaseAbility> abilities)
        {
            if (!hero.HasModifier("modifier_treant_living_armor"))
            {
                return 0;
            }

            var treant = heroes.FirstOrDefault(x => x.IsEnemy(Owner) && x.HeroId == HeroId.npc_dota_hero_treant);
            var ability = treant.GetAbilityById(AbilityId.treant_living_armor);
            var block = ability.GetAbilitySpecialData("damage_block");

            var count = abilities.Where(x => x.GetDamage(hero) > block).Count();

            if (abilities.Any(x => x.Ability.Id == AbilityId.enchantress_impetus))
            {
                count = 2;
            }

            return count * block;
        }

        private float DamageReduction(Hero hero, List<Hero> heroes)
        {
            var value = 0.0f;

            // Bristleback
            var bristleback = hero.GetAbilityById(AbilityId.bristleback_bristleback);
            if (bristleback != null && bristleback.Level != 0)
            {
                var brist = bristleback.Owner as Hero;
                if (brist.FindRotationAngle(Owner.Position) > 1.90f)
                {
                    value -= bristleback.GetAbilitySpecialData("back_damage_reduction") / 100f;
                }
                else if (brist.FindRotationAngle(Owner.Position) > 1.20f)
                {
                    value -= bristleback.GetAbilitySpecialData("side_damage_reduction") / 100f;
                }
            }

            // Modifier Centaur Stampede
            if (hero.HasModifier("modifier_centaur_stampede"))
            {
                var centaur = heroes.FirstOrDefault(x => x.IsEnemy(Owner) && x.HeroId == HeroId.npc_dota_hero_centaur);
                if (centaur.HasAghanimsScepter())
                {
                    var ability = centaur.GetAbilityById(AbilityId.centaur_stampede);

                    value -= ability.GetAbilitySpecialData("damage_reduction") / 100f;
                }
            }

            // Modifier Kunkka Ghostship
            if (hero.HasModifier("modifier_kunkka_ghost_ship_damage_absorb"))
            {
                var kunkka = heroes.FirstOrDefault(x => x.IsEnemy(Owner) && x.HeroId == HeroId.npc_dota_hero_kunkka);
                var ability = kunkka.GetAbilityById(AbilityId.kunkka_ghostship);

                value -= ability.GetAbilitySpecialData("ghostship_absorb") / 100f;
            }

            // Modifier Wisp Overcharge
            if (hero.HasModifier("modifier_wisp_overcharge"))
            {
                var wisp = heroes.FirstOrDefault(x => x.IsEnemy(Owner) && x.HeroId == HeroId.npc_dota_hero_wisp);
                var ability = wisp.GetAbilityById(AbilityId.wisp_overcharge);

                value += ability.GetAbilitySpecialData("bonus_damage_pct") / 100f;
            }

            // Modifier Bloodseeker Bloodrage
            if (hero.HasModifier("modifier_bloodseeker_bloodrage") || Owner.HasModifier("modifier_bloodseeker_bloodrage"))
            {
                var bloodseeker = heroes.FirstOrDefault(x => x.HeroId == HeroId.npc_dota_hero_bloodseeker);
                var ability = bloodseeker.GetAbilityById(AbilityId.bloodseeker_bloodrage);

                value += ability.GetAbilitySpecialData("damage_increase_pct") / 100f;
            }

            // Modifier Medusa Mana Shield
            if (hero.HasModifier("modifier_medusa_mana_shield"))
            {
                var ability = hero.GetAbilityById(AbilityId.medusa_mana_shield);

                if (hero.Mana >= 50)
                {
                    value -= ability.GetAbilitySpecialData("absorption_tooltip") / 100f;
                }
            }

            // Modifier Ursa Enrage
            if (hero.HasModifier("modifier_ursa_enrage"))
            {
                var ability = hero.GetAbilityById(AbilityId.ursa_enrage);
                value -= ability.GetAbilitySpecialData("damage_reduction") / 100f;
            }

            // Modifier Chen Penitence
            if (hero.HasModifier("modifier_chen_penitence"))
            {
                var chen = heroes.FirstOrDefault(x => x.IsAlly(Owner) && x.HeroId == HeroId.npc_dota_hero_chen);
                var ability = chen.GetAbilityById(AbilityId.chen_penitence);

                value += ability.GetAbilitySpecialData("bonus_damage_taken") / 100f;
            }

            // Modifier Shadow Demon Soul Catcher
            if (hero.HasModifier("modifier_shadow_demon_soul_catcher"))
            {
                var shadowDemon = heroes.FirstOrDefault(x => x.IsAlly(Owner) && x.HeroId == HeroId.npc_dota_hero_shadow_demon);
                var ability = shadowDemon.GetAbilityById(AbilityId.shadow_demon_soul_catcher);

                value += ability.GetAbilitySpecialData("bonus_damage_taken") / 100f;
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

                value -= ability.GetAbilitySpecialData("damage_absorb");

                var talent = abaddon.GetAbilityById(AbilityId.special_bonus_unique_abaddon);
                if (talent != null && talent.Level > 0)
                {
                    value -= talent.GetAbilitySpecialData("value");
                }
            }

            return value;
        }

        private float MagicalDamageBlock(Hero hero, List<Hero> heroes)
        {
            var value = 0.0f;

            // Modifier Hood Of Defiance Barrier
            if (hero.HasModifier("modifier_item_hood_of_defiance_barrier"))
            {
                var item = hero.GetItemById(AbilityId.item_hood_of_defiance);
                if (item != null)
                {
                    value -= item.GetAbilitySpecialData("barrier_block");
                }
            }

            // Modifier Pipe Barrier
            if (hero.HasModifier("modifier_item_pipe_barrier"))
            {
                var pipehero = heroes.FirstOrDefault(x => x.IsEnemy(Owner) && x.Inventory.Items.Any(v => v.Id == AbilityId.item_pipe));
                if (pipehero != null)
                {
                    var ability = pipehero.GetItemById(AbilityId.item_pipe);

                    value -= ability.GetAbilitySpecialData("barrier_block");
                }
            }

            // Modifier Infused Raindrop
            if (hero.HasModifier("modifier_item_infused_raindrop"))
            {
                var item = hero.GetItemById(AbilityId.item_infused_raindrop);
                if (item != null && item.Cooldown <= 0)
                {
                    value -= item.GetAbilitySpecialData("magic_damage_block");
                }
            }

            // Modifier Ember Spirit Flame Guard
            if (hero.HasModifier("modifier_ember_spirit_flame_guard"))
            {
                var ability = hero.GetAbilityById(AbilityId.ember_spirit_flame_guard);
                if (ability != null)
                {
                    value -= ability.GetAbilitySpecialData("absorb_amount");

                    var emberSpirit = ability.Owner as Hero;
                    var talent = emberSpirit.GetAbilityById(AbilityId.special_bonus_unique_ember_spirit_1);
                    if (talent != null && talent.Level > 0)
                    {
                        value -= talent.GetAbilitySpecialData("value");
                    }
                }
            }

            return value + DamageBlock(hero, heroes);
        }

        public List<Damage> DamageList { get; } = new List<Damage>();

        public class Damage
        {
            public Hero GetHero { get; }

            public float GetDamage { get; }

            public float GetReadyDamage { get; }

            public float GetTotalDamage { get; }

            public uint GetHealth { get; }

            public Damage(Hero hero, float damage, float readyDamage, float totalDamage, uint health)
            {
                GetHero = hero;
                GetDamage = damage;
                GetReadyDamage = readyDamage;
                GetTotalDamage = totalDamage;
                GetHealth = health;
            }
        }
    }  
}

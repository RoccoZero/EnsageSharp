using System.Linq;

using Ensage;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;

namespace SkywrathMagePlus
{
    internal class Extensions
    {
        private MenuManager Menu { get; }

        public Extensions(Config config)
        {
            Menu = config.Menu;
        }

        public bool Active(Hero target)
        {
            var borrowedTime = target.GetAbilityById(AbilityId.abaddon_borrowed_time);
            if (borrowedTime != null && borrowedTime.Owner.Health <= 2000 && borrowedTime.Cooldown <= 0 && borrowedTime.Level > 0)
            {
                return false;
            }

            if (target.HasAnyModifiers("modifier_dazzle_shallow_grave", "modifier_spirit_breaker_charge_of_darkness", "modifier_pugna_nether_ward_aura"))
            {
                return false;
            }

            var ability = target.Spellbook.Spells.Any(x => ActiveAbilities.Contains(x.Id) && x.IsInAbilityPhase);
            if (ability)
            {
                return true;
            }

            var stunDebuff = target.Modifiers.Any(x => x.IsStunDebuff && x.Duration > 1);
            return target.MovementSpeed < 240 || stunDebuff || target.HasAnyModifiers(ActiveModifiers);
        }

        private AbilityId[] ActiveAbilities { get; } =
        {
            AbilityId.rattletrap_power_cogs,
            AbilityId.enigma_black_hole,
            AbilityId.bane_fiends_grip,
            AbilityId.witch_doctor_death_ward
        };

        private string[] ActiveModifiers { get; } =
        {
            "modifier_skywrath_mystic_flare_aura_effect",
            "modifier_rod_of_atos_debuff",
            "modifier_crystal_maiden_frostbite",
            "modifier_crystal_maiden_freezing_field",
            "modifier_naga_siren_ensnare",
            "modifier_meepo_earthbind",
            "modifier_lone_druid_spirit_bear_entangle_effect",
            "modifier_legion_commander_duel",
            "modifier_kunkka_torrent",
            "modifier_enigma_black_hole_pull",
            "modifier_ember_spirit_searing_chains",
            "modifier_dark_troll_warlord_ensnare",
            "modifier_rattletrap_cog_marker",
            "modifier_axe_berserkers_call",
            "modifier_faceless_void_chronosphere_freeze",
            "modifier_winter_wyvern_cold_embrace"
        };

        public bool Cancel(Hero target)
        {
            return !target.IsMagicImmune() 
                && !target.IsInvulnerable()
                && !DuelAghanimsScepter(target)
                && !target.HasAnyModifiers(CancelModifiers);
        }

        private string[] CancelModifiers { get; } =
        {
            "modifier_abaddon_borrowed_time",
            "modifier_item_combo_breaker_buff",
            "modifier_winter_wyvern_winters_curse_aura",
            "modifier_winter_wyvern_winters_curse",
            "modifier_oracle_fates_edict"
        };

        public bool DuelAghanimsScepter(Hero target)
        {
            var duelAghanimsScepter = false;
            if (target.HasModifier("modifier_legion_commander_duel"))
            {
                duelAghanimsScepter = EntityManager<Hero>.Entities.Any(x =>
                                                                       x.HeroId == HeroId.npc_dota_hero_legion_commander &&
                                                                       x.IsValid &&
                                                                       x.IsVisible &&
                                                                       x.IsAlive &&
                                                                       x.HasAghanimsScepter());
            }

            return duelAghanimsScepter;
        }

        public bool ConcussiveShotTarget(Hero target, Hero targetHit)
        {
            if (!Menu.ConcussiveShotTargetItem)
            {
                return true;
            }

            if (targetHit == null)
            {
                return false;
            }

            if (target == targetHit)
            {
                return true;
            }

            if (target.Distance2D(targetHit) < 200)
            {
                return true;
            }

            return false;
        }

        public bool ComboBreaker(Hero target, bool menu = true)
        {
            if (!Menu.ComboBreakerItem && menu)
            {
                return false;
            }

            var comboBreaker = target.GetItemById(AbilityId.item_aeon_disk);
            if (comboBreaker != null && comboBreaker.Cooldown <= 0)
            {
                return true;
            }

            return false;
        }
    }
}

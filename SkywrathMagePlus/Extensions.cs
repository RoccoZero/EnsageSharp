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
            var stunDebuff = target.Modifiers.FirstOrDefault(x => x.IsStunDebuff);

            var borrowedTime = target.GetAbilityById(AbilityId.abaddon_borrowed_time);
            var powerCogs = target.GetAbilityById(AbilityId.rattletrap_power_cogs);
            var blackHole = target.GetAbilityById(AbilityId.enigma_black_hole);
            var fiendsGrip = target.GetAbilityById(AbilityId.bane_fiends_grip);
            var deathWard = target.GetAbilityById(AbilityId.witch_doctor_death_ward);

            return (target.MovementSpeed < 240
                || (stunDebuff != null && stunDebuff.Duration >= 1)
                || target.HasModifier("modifier_skywrath_mystic_flare_aura_effect")
                || target.HasModifier("modifier_rod_of_atos_debuff")
                || target.HasModifier("modifier_crystal_maiden_frostbite")
                || target.HasModifier("modifier_crystal_maiden_freezing_field")
                || target.HasModifier("modifier_naga_siren_ensnare")
                || target.HasModifier("modifier_meepo_earthbind")
                || target.HasModifier("modifier_lone_druid_spirit_bear_entangle_effect")
                || target.HasModifier("modifier_legion_commander_duel")
                || target.HasModifier("modifier_kunkka_torrent")
                || target.HasModifier("modifier_enigma_black_hole_pull")
                || (blackHole != null && blackHole.IsInAbilityPhase)
                || target.HasModifier("modifier_ember_spirit_searing_chains")
                || target.HasModifier("modifier_dark_troll_warlord_ensnare")
                || target.HasModifier("modifier_rattletrap_cog_marker")
                || (powerCogs != null && powerCogs.IsInAbilityPhase)
                || target.HasModifier("modifier_axe_berserkers_call")
                || target.HasModifier("modifier_faceless_void_chronosphere_freeze")
                || (fiendsGrip != null && fiendsGrip.IsInAbilityPhase)
                || (deathWard != null && deathWard.IsInAbilityPhase)
                || target.HasModifier("modifier_winter_wyvern_cold_embrace"))
                && (borrowedTime == null || borrowedTime.Owner.Health > 2000 || borrowedTime.Cooldown > 0)
                && !target.HasModifier("modifier_dazzle_shallow_grave")
                && !target.HasModifier("modifier_spirit_breaker_charge_of_darkness")
                && !target.HasModifier("modifier_pugna_nether_ward_aura");
        }

        public bool Cancel(Hero target)
        {
            return !target.IsMagicImmune() && !target.IsInvulnerable()
                && !target.HasAnyModifiers("modifier_abaddon_borrowed_time", "modifier_item_combo_breaker_buff")
                && !target.HasAnyModifiers("modifier_winter_wyvern_winters_curse_aura", "modifier_winter_wyvern_winters_curse")
                && !DuelAghanimsScepter(target);
        }

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
    }
}

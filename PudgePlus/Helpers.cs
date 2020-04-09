using System.Linq;

using Ensage;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Prediction;

namespace PudgePlus
{
    internal class Helpers
    {
        private MenuManager Menu { get; }

        private Unit Owner { get; }

        public Helpers(Config config)
        {
            Menu = config.Menu;
            Owner = config.Main.Context.Owner;
        }

        public bool ShouldCastHook(PredictionOutput output)
        {
            if (output.HitChance == HitChance.OutOfRange || output.HitChance == HitChance.Impossible)
            {
                return false;
            }

            if (output.HitChance == HitChance.Collision)
            {
                return false;
            }

            if (output.HitChance < HitChance.Medium)
            {
                return false;
            }

            return true;
        }

        public bool CancelMagicImmune(Hero target)
        {
            return !target.IsMagicImmune() && !DuelAghanimsScepter(target);
        }

        public bool Cancel(Hero target)
        {
            return !target.IsInvulnerable() && !target.HasAnyModifiers(CancelModifiers);
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
                                                                       x.IsValid &&
                                                                       x.IsVisible &&
                                                                       x.IsAlive &&
                                                                       x.HeroId == HeroId.npc_dota_hero_legion_commander &&
                                                                       x.HasAghanimsScepter());
            }

            return duelAghanimsScepter;
        }

        public bool ComboBreaker(Hero target, bool menu = true)
        {
            if (!Menu.ComboBreakerItem && menu)
            {
                return false;
            }

            var comboBreaker = target.GetItemById(AbilityId.item_combo_breaker);
            if (comboBreaker != null && comboBreaker.Cooldown <= 0)
            {
                return true;
            }

            return false;
        }
    }
}

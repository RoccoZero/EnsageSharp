using Ensage;
using Ensage.SDK.Extensions;

namespace LegionCommanderPlus
{
    internal class Extensions
    {
        private MenuManager Menu { get; }

        public Extensions(Config config)
        {
            Menu = config.Menu;
        }

        public bool Cancel(Hero target)
        {
            return !target.HasAnyModifiers(CancelModifiers);
        }

        private string[] CancelModifiers { get; } =
        {
            "modifier_winter_wyvern_winters_curse_aura",
            "modifier_winter_wyvern_winters_curse",
            "modifier_oracle_fates_edict"
        };

        public bool CancelAdditionally(Hero target)
        {
            return !target.IsInvulnerable() && !target.HasAnyModifiers("modifier_abaddon_borrowed_time", "modifier_item_combo_breaker_buff");
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

using System.Linq;

using Ensage;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;

using SharpDX;

namespace VisagePlus
{
    internal class Extensions
    {
        private MultiSleeper MultiSleeper { get; }

        public Extensions(Config config)
        {
            MultiSleeper = config.MultiSleeper;
        }

        public int GetDelay { get; } = 100 + (int)Game.Ping;

        private float LastCastAttempt { get; set; }

        public bool CanBeCasted(Ability ability, Unit unit)
        {
            if (ability.IsHidden || ability.Cooldown > 0)
            {
                return false;
            }

            if (unit.IsStunned() || unit.IsMuted() || unit.IsSilenced())
            {
                return false;
            }

            if (Game.RawGameTime - LastCastAttempt < 0.8f)
            {
                return false;
            }

            return true;
        }

        public bool UseAbility(Ability ability, Unit unit)
        {
            if (!CanBeCasted(ability, unit))
            {
                return false;
            }

            var result = ability.UseAbility();
            if (result)
            {
                LastCastAttempt = Game.RawGameTime;
            }

            return result;
        }

        public bool Attack(Unit unit, Unit target)
        {
            if (!MultiSleeper.Sleeping($"Attack{ unit.Handle }"))
            {
                MultiSleeper.Sleep(200, $"Attack{ unit.Handle }");
                return unit.Attack(target);
            }

            return false;
        }

        public bool Move(Unit unit, Vector3 position)
        {
            if (unit.IsRooted())
            {
                return false;
            }

            if (!MultiSleeper.Sleeping($"Move{ unit.Handle }"))
            {
                MultiSleeper.Sleep(200, $"Move{ unit.Handle }");
                return unit.Move(position);
            }

            return false;
        }

        public bool Follow(Unit unit, Unit target)
        {
            if (unit.IsRooted())
            {
                return false;
            }

            if (!MultiSleeper.Sleeping($"Follow{ unit.Handle }"))
            {
                MultiSleeper.Sleep(200, $"Follow{ unit.Handle }");
                return unit.Follow(target);
            }

            return false;
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
    }
}

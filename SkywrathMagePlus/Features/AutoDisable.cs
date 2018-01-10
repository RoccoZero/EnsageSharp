using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Ensage;
using Ensage.Common.Threading;
using Ensage.SDK.Extensions;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;

namespace SkywrathMagePlus.Features
{
    internal class AutoDisable
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private Unit Owner { get; }

        private Abilities Abilities { get; }

        private TaskHandler Handler { get; }

        public AutoDisable(Config config)
        {
            Config = config;
            Menu = config.Menu;
            Abilities = config.Abilities;
            Owner = config.Main.Context.Owner;

            Handler = UpdateManager.Run(ExecuteAsync, true, false);

            if (config.Menu.AutoDisableItem)
            {
                Handler.RunAsync();
            }

            config.Menu.AutoDisableItem.PropertyChanged += AutoDisableChanged;
        }

        public void Dispose()
        {
            Menu.AutoDisableItem.PropertyChanged -= AutoDisableChanged;

            if (Menu.AutoDisableItem)
            {
                Handler?.Cancel();
            }
        }

        private void AutoDisableChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Menu.AutoDisableItem)
            {
                Handler.RunAsync();
            }
            else
            {
                Handler?.Cancel();
            }
        }

        private async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                if (Game.IsPaused || !Owner.IsValid || !Owner.IsAlive || Owner.IsStunned() || Owner.IsInvisible())
                {
                    return;
                }

                var target = EntityManager<Hero>.Entities.FirstOrDefault(x =>
                                                                         x.IsValid &&
                                                                         x.IsVisible &&
                                                                         x.IsAlive &&
                                                                         !x.IsIllusion &&
                                                                         x.IsEnemy(Owner) &&
                                                                         Disable(x));

                if (target == null)
                {
                    return;
                }

                // Hex
                var hex = Abilities.Hex;
                if (hex != null
                    && Menu.AutoDisableToggler.Value.IsEnabled(hex.ToString())
                    && hex.CanBeCasted
                    && hex.CanHit(target))
                {
                    hex.UseAbility(target);
                    await Await.Delay(hex.GetCastDelay(target), token);
                    return;
                }

                // Orchid
                var orchid = Abilities.Orchid;
                if (orchid != null
                    && Menu.AutoDisableToggler.Value.IsEnabled(orchid.ToString())
                    && orchid.CanBeCasted
                    && orchid.CanHit(target))
                {
                    orchid.UseAbility(target);
                    await Await.Delay(orchid.GetCastDelay(target), token);
                    return;
                }

                // Bloodthorn
                var bloodthorn = Abilities.Bloodthorn;
                if (bloodthorn != null
                    && Menu.AutoDisableToggler.Value.IsEnabled(bloodthorn.ToString())
                    && bloodthorn.CanBeCasted
                    && bloodthorn.CanHit(target))
                {
                    bloodthorn.UseAbility(target);
                    await Await.Delay(bloodthorn.GetCastDelay(target), token);
                    return;
                }

                // AncientSeal
                var ancientSeal = Abilities.AncientSeal;
                if (Menu.AutoDisableToggler.Value.IsEnabled(ancientSeal.ToString())
                    && ancientSeal.CanBeCasted
                    && ancientSeal.CanHit(target))
                {
                    ancientSeal.UseAbility(target);
                    await Await.Delay(ancientSeal.GetCastDelay(target), token);
                    return;
                }
            }
            catch (TaskCanceledException)
            {
                // canceled
            }
            catch (Exception e)
            {
                Config.Main.Log.Error(e);
            }
        }

        private bool Disable(Hero target)
        {
            var ability = target.Spellbook.Spells.Any(x => AbilityId.Contains(x.Id) && x.IsInAbilityPhase);
            if (ability)
            {
                return true;
            }

            return false;
        }

        private AbilityId[] AbilityId { get; } =
        {
            Ensage.AbilityId.queenofpain_blink,
            Ensage.AbilityId.antimage_blink,
            Ensage.AbilityId.antimage_mana_void,
            Ensage.AbilityId.legion_commander_duel,
            Ensage.AbilityId.doom_bringer_doom,
            Ensage.AbilityId.faceless_void_time_walk,
            Ensage.AbilityId.faceless_void_chronosphere,
            Ensage.AbilityId.witch_doctor_death_ward,
            Ensage.AbilityId.rattletrap_power_cogs,
            Ensage.AbilityId.tidehunter_ravage,
            Ensage.AbilityId.axe_berserkers_call,
            Ensage.AbilityId.brewmaster_primal_split,
            Ensage.AbilityId.omniknight_guardian_angel,
            Ensage.AbilityId.queenofpain_sonic_wave,
            Ensage.AbilityId.slardar_slithereen_crush,
            Ensage.AbilityId.lion_finger_of_death,
            Ensage.AbilityId.lina_laguna_blade
        };
    }
}

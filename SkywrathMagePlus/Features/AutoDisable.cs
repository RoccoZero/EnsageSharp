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

        private SkywrathMagePlus Main { get; }

        private TaskHandler Handler { get; }

        public AutoDisable(Config config)
        {
            Config = config;
            Menu = config.Menu;
            Main = config.Main;
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
                var Hex = Main.Hex;
                if (Hex != null
                    && Menu.AutoDisableToggler.Value.IsEnabled(Hex.ToString())
                    && Hex.CanBeCasted
                    && Hex.CanHit(target))
                {
                    Hex.UseAbility(target);
                    await Await.Delay(Hex.GetCastDelay(target), token);
                    return;
                }

                // Orchid
                var Orchid = Main.Orchid;
                if (Orchid != null
                    && Menu.AutoDisableToggler.Value.IsEnabled(Orchid.ToString())
                    && Orchid.CanBeCasted
                    && Orchid.CanHit(target))
                {
                    Orchid.UseAbility(target);
                    await Await.Delay(Orchid.GetCastDelay(target), token);
                    return;
                }

                // Bloodthorn
                var Bloodthorn = Main.Bloodthorn;
                if (Bloodthorn != null
                    && Menu.AutoDisableToggler.Value.IsEnabled(Bloodthorn.ToString())
                    && Bloodthorn.CanBeCasted
                    && Bloodthorn.CanHit(target))
                {
                    Bloodthorn.UseAbility(target);
                    await Await.Delay(Bloodthorn.GetCastDelay(target), token);
                    return;
                }

                // AncientSeal
                var AncientSeal = Main.AncientSeal;
                if (Menu.AutoDisableToggler.Value.IsEnabled(AncientSeal.ToString())
                    && AncientSeal.CanBeCasted
                    && AncientSeal.CanHit(target))
                {
                    AncientSeal.UseAbility(target);
                    await Await.Delay(AncientSeal.GetCastDelay(target), token);
                    return;
                }
            }
            catch (TaskCanceledException)
            {
                // canceled
            }
            catch (Exception e)
            {
                Main.Log.Error(e);
            }
        }

        private bool Disable(Hero target)
        {
            var queenofPainBlink = target.GetAbilityById(AbilityId.queenofpain_blink);
            var antiMageBlink = target.GetAbilityById(AbilityId.antimage_blink);
            var manaVoid = target.GetAbilityById(AbilityId.antimage_mana_void);
            var duel = target.GetAbilityById(AbilityId.legion_commander_duel);
            var doom = target.GetAbilityById(AbilityId.doom_bringer_doom);
            var timeWalk = target.GetAbilityById(AbilityId.faceless_void_time_walk);
            var chronoSphere = target.GetAbilityById(AbilityId.faceless_void_chronosphere);
            var deathWard = target.GetAbilityById(AbilityId.witch_doctor_death_ward);
            var powerCogs = target.GetAbilityById(AbilityId.rattletrap_power_cogs);
            var ravage = target.GetAbilityById(AbilityId.tidehunter_ravage);
            var berserkersCall = target.GetAbilityById(AbilityId.axe_berserkers_call);
            var primalSplit = target.GetAbilityById(AbilityId.brewmaster_primal_split);
            var guardianAngel = target.GetAbilityById(AbilityId.omniknight_guardian_angel);
            var sonicWave = target.GetAbilityById(AbilityId.queenofpain_sonic_wave);
            var slithereenCrush = target.GetAbilityById(AbilityId.slardar_slithereen_crush);
            var fingerofDeath = target.GetAbilityById(AbilityId.lion_finger_of_death);
            var lagunaBlade = target.GetAbilityById(AbilityId.lina_laguna_blade);

            return (queenofPainBlink != null && queenofPainBlink.IsInAbilityPhase)
                || (antiMageBlink != null && antiMageBlink.IsInAbilityPhase)
                || (manaVoid != null && manaVoid.IsInAbilityPhase)
                || (duel != null && duel.IsInAbilityPhase)
                || (doom != null && doom.IsInAbilityPhase)
                || (timeWalk != null && timeWalk.IsInAbilityPhase)
                || (chronoSphere != null && chronoSphere.IsInAbilityPhase)
                || (deathWard != null && deathWard.IsInAbilityPhase)
                || (powerCogs != null && powerCogs.IsInAbilityPhase)
                || (ravage != null && ravage.IsInAbilityPhase)
                || (berserkersCall != null && berserkersCall.IsInAbilityPhase)
                || (primalSplit != null && primalSplit.IsInAbilityPhase)
                || (guardianAngel != null && guardianAngel.IsInAbilityPhase)
                || (sonicWave != null && sonicWave.IsInAbilityPhase)
                || (slithereenCrush != null && slithereenCrush.IsInAbilityPhase)
                || (fingerofDeath != null && fingerofDeath.IsInAbilityPhase)
                || (lagunaBlade != null && lagunaBlade.IsInAbilityPhase);
        }
    }
}

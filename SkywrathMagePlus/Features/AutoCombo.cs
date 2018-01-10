using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Ensage;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.Common.Threading;
using Ensage.SDK.Extensions;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;
using Ensage.SDK.Prediction;

namespace SkywrathMagePlus.Features
{
    internal class AutoCombo
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private Abilities Abilities { get; }

        private Extensions Extensions { get; }

        private MultiSleeper MultiSleeper { get; }

        private Unit Owner { get; }

        private IPredictionManager Prediction { get; }

        private TaskHandler Handler { get; }

        public AutoCombo(Config config)
        {
            Config = config;
            Menu = config.Menu;
            Abilities = config.Abilities;
            Extensions = config.Extensions;
            MultiSleeper = config.MultiSleeper;
            Owner = config.Main.Context.Owner;
            Prediction = config.Main.Context.Prediction;

            Handler = UpdateManager.Run(ExecuteAsync, true, false);

            if (config.Menu.AutoComboItem)
            {
                Handler.RunAsync();
            }

            config.Menu.AutoComboItem.PropertyChanged += AutoComboChanged;
        }

        public void Dispose()
        {
            Menu.AutoComboItem.PropertyChanged -= AutoComboChanged;

            if (Menu.AutoComboItem)
            {
                Handler?.Cancel();
            }
        }

        private void AutoComboChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Menu.AutoComboItem)
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

                if (Menu.AutoComboWhenComboItem && Menu.ComboKeyItem)
                {
                    return;
                }

                if (Menu.AutoOwnerMinHealthItem > ((float)Owner.Health / Owner.MaximumHealth) * 100)
                {
                    return;
                }

                var target = EntityManager<Hero>.Entities.Where(x =>
                                                                x.IsValid &&
                                                                x.IsVisible &&
                                                                x.IsAlive &&
                                                                !x.IsIllusion &&
                                                                x.IsEnemy(Owner) &&
                                                                Extensions.Active(x)).OrderBy(x => x.Distance2D(Owner)).FirstOrDefault();

                if (target == null)
                {
                    return;
                }

                if (!Extensions.Cancel(target) || Extensions.ComboBreaker(target, false))
                {
                    return;
                }

                if (Menu.BladeMailItem && target.HasModifier("modifier_item_blade_mail_reflect"))
                {
                    return;
                }

                if (target.IsBlockingAbilities())
                {
                    Config.LinkenBreaker.Handler.RunAsync();
                    return;
                }

                var stunDebuff = target.Modifiers.FirstOrDefault(x => x.IsStunDebuff);
                var hexDebuff = target.Modifiers.FirstOrDefault(x => x.Name == "modifier_sheepstick_debuff");

                // Hex
                var hex = Abilities.Hex;
                if (hex != null
                    && Menu.AutoItemToggler.Value.IsEnabled(hex.ToString())
                    && hex.CanBeCasted
                    && hex.CanHit(target)
                    && (stunDebuff == null || !stunDebuff.IsValid || stunDebuff.RemainingTime <= 0.3f)
                    && (hexDebuff == null || !hexDebuff.IsValid || hexDebuff.RemainingTime <= 0.3f))
                {
                    hex.UseAbility(target);
                    await Await.Delay(hex.GetCastDelay(target), token);
                }

                // Orchid
                var orchid = Abilities.Orchid;
                if (orchid != null
                    && Menu.AutoItemToggler.Value.IsEnabled(orchid.ToString())
                    && orchid.CanBeCasted
                    && orchid.CanHit(target))
                {
                    orchid.UseAbility(target);
                    await Await.Delay(orchid.GetCastDelay(target), token);
                }

                // Bloodthorn
                var bloodthorn = Abilities.Bloodthorn;
                if (bloodthorn != null
                    && Menu.AutoItemToggler.Value.IsEnabled(bloodthorn.ToString())
                    && bloodthorn.CanBeCasted
                    && bloodthorn.CanHit(target))
                {
                    bloodthorn.UseAbility(target);
                    await Await.Delay(bloodthorn.GetCastDelay(target), token);
                }

                // Mystic Flare
                var mysticFlare = Abilities.MysticFlare;
                if (Menu.AutoAbilityToggler.Value.IsEnabled(mysticFlare.ToString())
                    && Menu.AutoMinHealthToUltItem <= ((float)target.Health / target.MaximumHealth) * 100
                    && mysticFlare.CanBeCasted
                    && mysticFlare.CanHit(target))
                {
                    var enemies = EntityManager<Hero>.Entities.Where(x =>
                                                                     x.IsVisible &&
                                                                     x.IsAlive &&
                                                                     x.IsValid &&
                                                                     !x.IsIllusion &&
                                                                     x.IsEnemy(Owner) &&
                                                                     x.Distance2D(Owner) <= mysticFlare.CastRange).ToList();

                    var dubleMysticFlare = Owner.HasAghanimsScepter() && enemies.Count() == 1;
                    var input = new PredictionInput
                    {
                        Owner = Owner,
                        Range = mysticFlare.CastRange,
                        Radius = dubleMysticFlare ? -250 : -100
                    };

                    var output = Prediction.GetPrediction(input.WithTarget(target));

                    mysticFlare.UseAbility(output.CastPosition);
                    await Await.Delay(mysticFlare.GetCastDelay(output.CastPosition), token);
                }

                // Nullifier
                var nullifier = Abilities.Nullifier;
                if (nullifier != null
                    && Menu.ItemToggler.Value.IsEnabled(nullifier.ToString())
                    && nullifier.CanBeCasted
                    && nullifier.CanHit(target)
                    && (stunDebuff == null || !stunDebuff.IsValid || stunDebuff.RemainingTime <= 0.5f)
                    && (hexDebuff == null || !hexDebuff.IsValid || hexDebuff.RemainingTime <= 0.5f))
                {
                    nullifier.UseAbility(target);
                    await Await.Delay(nullifier.GetCastDelay(target), token);
                }

                // RodofAtos
                var rodofAtos = Abilities.RodofAtos;
                if (rodofAtos != null
                    && Menu.AutoItemToggler.Value.IsEnabled(rodofAtos.ToString())
                    && rodofAtos.CanBeCasted
                    && rodofAtos.CanHit(target)
                    && !target.Modifiers.Any(x => x.IsValid && x.Name == "modifier_rod_of_atos_debuff" && x.RemainingTime > 0.5f)
                    && (stunDebuff == null || !stunDebuff.IsValid || stunDebuff.RemainingTime <= 0.5f))
                {
                    rodofAtos.UseAbility(target);
                    await Await.Delay(rodofAtos.GetCastDelay(target), token);
                }

                // AncientSeal
                var ancientSeal = Abilities.AncientSeal;
                if (Menu.AutoAbilityToggler.Value.IsEnabled(ancientSeal.ToString())
                    && ancientSeal.CanBeCasted
                    && ancientSeal.CanHit(target))
                {
                    ancientSeal.UseAbility(target);
                    await Await.Delay(ancientSeal.GetCastDelay(target), token);
                    return;
                }

                // Veil
                var veil = Abilities.Veil;
                if (veil != null
                    && Menu.AutoItemToggler.Value.IsEnabled(veil.ToString())
                    && veil.CanBeCasted
                    && veil.CanHit(target))
                {
                    veil.UseAbility(target.Position);
                    await Await.Delay(veil.GetCastDelay(target.Position), token);
                }

                // Ethereal
                var ethereal = Abilities.Ethereal;
                if (ethereal != null
                    && Menu.AutoItemToggler.Value.IsEnabled(ethereal.ToString())
                    && ethereal.CanBeCasted
                    && ethereal.CanHit(target))
                {
                    ethereal.UseAbility(target);
                    MultiSleeper.Sleep(ethereal.GetHitTime(target), "ethereal");
                    await Await.Delay(ethereal.GetCastDelay(target), token);
                }

                // Shivas
                var shivas = Abilities.Shivas;
                if (shivas != null
                    && Menu.AutoItemToggler.Value.IsEnabled(shivas.ToString())
                    && shivas.CanBeCasted
                    && shivas.CanHit(target))
                {
                    shivas.UseAbility();
                    await Await.Delay(shivas.GetCastDelay(), token);
                }

                if (!MultiSleeper.Sleeping("ethereal") || target.IsEthereal())
                {
                    // ConcussiveShot
                    var concussiveShot = Abilities.ConcussiveShot;
                    if (Menu.AutoAbilityToggler.Value.IsEnabled(concussiveShot.ToString())
                        && Extensions.ConcussiveShotTarget(target, concussiveShot.TargetHit)
                        && concussiveShot.CanBeCasted
                        && Owner.Distance2D(target) < Menu.ConcussiveShotUseRadiusItem - Owner.HullRadius)
                    {
                        concussiveShot.UseAbility();
                        await Await.Delay(concussiveShot.GetCastDelay(), token);
                    }

                    // ArcaneBolt
                    var arcaneBolt = Abilities.ArcaneBolt;
                    if (Menu.AutoAbilityToggler.Value.IsEnabled(arcaneBolt.ToString())
                        && arcaneBolt.CanBeCasted
                        && arcaneBolt.CanHit(target))
                    {
                        arcaneBolt.UseAbility(target);

                        UpdateManager.BeginInvoke(() =>
                        {
                            MultiSleeper.Sleep(arcaneBolt.GetHitTime(target) - (arcaneBolt.GetCastDelay(target) + 350), $"arcanebolt_{ target.Name }");
                        },
                        arcaneBolt.GetCastDelay(target) + 50);

                        await Await.Delay(arcaneBolt.GetCastDelay(target), token);
                        return;
                    }

                    // Dagon
                    var dagon = Abilities.Dagon;
                    if (dagon != null
                        && Menu.AutoItemToggler.Value.IsEnabled("item_dagon_5")
                        && dagon.CanBeCasted
                        && dagon.CanHit(target))
                    {
                        dagon.UseAbility(target);
                        await Await.Delay(dagon.GetCastDelay(target), token);
                        return;
                    }
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
    }
}

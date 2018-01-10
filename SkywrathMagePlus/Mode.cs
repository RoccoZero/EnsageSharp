using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Ensage;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.Common.Threading;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Orbwalker.Modes;
using Ensage.SDK.Prediction;

using PlaySharp.Toolkit.Helper.Annotations;

using SharpDX;

namespace SkywrathMagePlus
{
    [PublicAPI]
    internal class Mode : KeyPressOrbwalkingModeAsync
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private Abilities Abilities { get; }

        private Extensions Extensions { get; }

        private MultiSleeper MultiSleeper { get; }

        private IPredictionManager Prediction { get; }

        public Mode(Key key, Config config) 
            : base(config.Main.Context, key)
        {
            Config = config;
            Menu = config.Menu;
            Abilities = config.Abilities;
            Extensions = config.Extensions;
            MultiSleeper = config.MultiSleeper;
            Prediction = config.Main.Context.Prediction;
        }

        public override async Task ExecuteAsync(CancellationToken token)
        {
            var target = Config.UpdateMode.Target;
            if (target == null || Menu.BladeMailItem && target.HasModifier("modifier_item_blade_mail_reflect"))
            {
                Orbwalker.Move(Game.MousePosition);
                return;
            }

            // Blink
            var blink = Abilities.Blink;
            if (blink != null
                && Menu.ItemToggler.Value.IsEnabled(blink.ToString())
                && Owner.Distance2D(Game.MousePosition) > Menu.BlinkActivationItem
                && Owner.Distance2D(target) > 600
                && blink.CanBeCasted)
            {
                var blinkPos = target.Position.Extend(Game.MousePosition, Menu.BlinkDistanceEnemyItem);
                if (Owner.Distance2D(blinkPos) < blink.CastRange)
                {
                    blink.UseAbility(blinkPos);
                    await Await.Delay(blink.GetCastDelay(blinkPos), token);
                }
            }

            if (Extensions.Cancel(target) && StartCombo(target))
            {
                if (!target.IsBlockingAbilities())
                {
                    var comboBreaker = Extensions.ComboBreaker(target);
                    var stunDebuff = target.Modifiers.FirstOrDefault(x => x.IsStunDebuff);
                    var hexDebuff = target.Modifiers.FirstOrDefault(x => x.Name == "modifier_sheepstick_debuff");

                    // Hex
                    var hex = Abilities.Hex;
                    if (hex != null
                        && Menu.ItemToggler.Value.IsEnabled(hex.ToString())
                        && hex.CanBeCasted
                        && hex.CanHit(target)
                        && !comboBreaker
                        && (stunDebuff == null || !stunDebuff.IsValid || stunDebuff.RemainingTime <= 0.3f)
                        && (hexDebuff == null || !hexDebuff.IsValid || hexDebuff.RemainingTime <= 0.3f))
                    {
                        hex.UseAbility(target);
                        await Await.Delay(hex.GetCastDelay(target), token);
                    }

                    // Orchid
                    var orchid = Abilities.Orchid;
                    if (orchid != null
                        && Menu.ItemToggler.Value.IsEnabled(orchid.ToString())
                        && orchid.CanBeCasted
                        && orchid.CanHit(target)
                        && !comboBreaker)
                    {
                        orchid.UseAbility(target);
                        await Await.Delay(orchid.GetCastDelay(target), token);
                    }

                    // Bloodthorn
                    var bloodthorn = Abilities.Bloodthorn;
                    if (bloodthorn != null
                        && Menu.ItemToggler.Value.IsEnabled(bloodthorn.ToString())
                        && bloodthorn.CanBeCasted
                        && bloodthorn.CanHit(target)
                        && !comboBreaker)
                    {
                        bloodthorn.UseAbility(target);
                        await Await.Delay(bloodthorn.GetCastDelay(target), token);
                    }

                    // MysticFlare
                    var mysticFlare = Abilities.MysticFlare;
                    if (Menu.AbilityToggler.Value.IsEnabled(mysticFlare.ToString())
                        && Menu.MinHealthToUltItem <= ((float)target.Health / target.MaximumHealth) * 100
                        && mysticFlare.CanBeCasted
                        && mysticFlare.CanHit(target)
                        && !comboBreaker
                        && (BadUlt(target) || Extensions.Active(target)))
                    {
                        var enemies = EntityManager<Hero>.Entities.Where(x =>
                                                                         x.IsValid &&
                                                                         x.IsVisible &&
                                                                         x.IsAlive &&
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
                        && !comboBreaker
                        && (stunDebuff == null || !stunDebuff.IsValid || stunDebuff.RemainingTime <= 0.5f)
                        && (hexDebuff == null || !hexDebuff.IsValid || hexDebuff.RemainingTime <= 0.5f))
                    {
                        nullifier.UseAbility(target);
                        await Await.Delay(nullifier.GetCastDelay(target), token);
                    }

                    // RodofAtos
                    var atosDebuff = target.Modifiers.Any(x => x.IsValid && x.Name == "modifier_rod_of_atos_debuff" && x.RemainingTime > 0.5f);
                    var rodofAtos = Abilities.RodofAtos;
                    if (rodofAtos != null
                        && Menu.ItemToggler.Value.IsEnabled(rodofAtos.ToString())
                        && rodofAtos.CanBeCasted
                        && rodofAtos.CanHit(target)
                        && !atosDebuff
                        && (stunDebuff == null || !stunDebuff.IsValid || stunDebuff.RemainingTime <= 0.5f))
                    {
                        rodofAtos.UseAbility(target);
                        await Await.Delay(rodofAtos.GetCastDelay(target), token);
                    }

                    // AncientSeal
                    var ancientSeal = Abilities.AncientSeal;
                    if (Menu.AbilityToggler.Value.IsEnabled(ancientSeal.ToString())
                        && ancientSeal.CanBeCasted
                        && ancientSeal.CanHit(target)
                        && !comboBreaker)
                    {
                        ancientSeal.UseAbility(target);
                        await Await.Delay(ancientSeal.GetCastDelay(target), token);
                        return;
                    }

                    // Veil
                    var veil = Abilities.Veil;
                    if (veil != null
                        && Menu.ItemToggler.Value.IsEnabled(veil.ToString())
                        && veil.CanBeCasted
                        && veil.CanHit(target))
                    {
                        veil.UseAbility(target.Position);
                        await Await.Delay(veil.GetCastDelay(target.Position), token);
                    }

                    // Ethereal
                    var ethereal = Abilities.Ethereal;
                    if (ethereal != null
                        && Menu.ItemToggler.Value.IsEnabled(ethereal.ToString())
                        && ethereal.CanBeCasted
                        && ethereal.CanHit(target)
                        && !comboBreaker)
                    {
                        ethereal.UseAbility(target);
                        MultiSleeper.Sleep(ethereal.GetHitTime(target), "ethereal");
                        await Await.Delay(ethereal.GetCastDelay(target), token);
                    }

                    // Shivas
                    var shivas = Abilities.Shivas;
                    if (shivas != null
                        && Menu.ItemToggler.Value.IsEnabled(shivas.ToString())
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
                        if (Menu.AbilityToggler.Value.IsEnabled(concussiveShot.ToString())
                            && Extensions.ConcussiveShotTarget(target, concussiveShot.TargetHit)
                            && concussiveShot.CanBeCasted
                            && concussiveShot.CanHit(target))
                        {
                            concussiveShot.UseAbility();
                            await Await.Delay(concussiveShot.GetCastDelay(), token);
                        }

                        // ArcaneBolt
                        var arcaneBolt = Abilities.ArcaneBolt;
                        if (Menu.AbilityToggler.Value.IsEnabled(arcaneBolt.ToString())
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
                            && Menu.ItemToggler.Value.IsEnabled("item_dagon_5")
                            && dagon.CanBeCasted
                            && dagon.CanHit(target)
                            && !comboBreaker)
                        {
                            dagon.UseAbility(target);
                            await Await.Delay(dagon.GetCastDelay(target), token);
                            return;
                        }
                    }

                    // UrnOfShadows
                    var urnOfShadows = Abilities.UrnOfShadows;
                    if (urnOfShadows != null
                        && Menu.ItemToggler.Value.IsEnabled(urnOfShadows.ToString())
                        && urnOfShadows.CanBeCasted
                        && urnOfShadows.CanHit(target)
                        && !comboBreaker)
                    {
                        urnOfShadows.UseAbility(target);
                        await Await.Delay(urnOfShadows.GetCastDelay(target), token);
                    }

                    // SpiritVessel
                    var spiritVessel = Abilities.SpiritVessel;
                    if (spiritVessel != null
                        && Menu.ItemToggler.Value.IsEnabled(spiritVessel.ToString())
                        && spiritVessel.CanBeCasted
                        && spiritVessel.CanHit(target)
                        && !comboBreaker)
                    {
                        spiritVessel.UseAbility(target);
                        await Await.Delay(spiritVessel.GetCastDelay(target), token);
                    }
                }
                else
                {
                    Config.LinkenBreaker.Handler.RunAsync();
                }
            }

            if (target.IsInvulnerable() || target.IsAttackImmune())
            {
                Orbwalker.Move(Game.MousePosition);
            }
            else
            {
                if (Menu.OrbwalkerItem.Value.SelectedValue.Contains("Default"))
                {
                    Orbwalker.OrbwalkTo(target);
                }
                else if (Menu.OrbwalkerItem.Value.SelectedValue.Contains("Distance"))
                {
                    var ownerDis = Math.Min(Owner.Distance2D(Game.MousePosition), 230);
                    var ownerPos = Owner.Position.Extend(Game.MousePosition, ownerDis);
                    var pos = target.Position.Extend(ownerPos, Menu.MinDisInOrbwalkItem);

                    Orbwalker.OrbwalkingPoint = pos;
                    Orbwalker.OrbwalkTo(target);
                    Orbwalker.OrbwalkingPoint = Vector3.Zero;
                }
                else if (Menu.OrbwalkerItem.Value.SelectedValue.Contains("Free"))
                {
                    if (Owner.Distance2D(target) < Owner.AttackRange(target) && target.Distance2D(Game.MousePosition) < Owner.AttackRange(target))
                    {
                        Orbwalker.OrbwalkTo(target);
                    }
                    else
                    {
                        Orbwalker.Move(Game.MousePosition);
                    }
                }
                else if (Menu.OrbwalkerItem.Value.SelectedValue.Contains("Only Attack"))
                {
                    Orbwalker.Attack(target);
                }
                else if (Menu.OrbwalkerItem.Value.SelectedValue.Contains("No Move"))
                {
                    if (Owner.Distance2D(target) < Owner.AttackRange(target))
                    {
                        Orbwalker.Attack(target);
                    }
                }
            }
        }

        private bool StartCombo(Hero target)
        {
            if (!Menu.StartComboKeyItem)
            {
                return true;
            }

            var hex = Abilities.Hex;
            var ancientSeal = Abilities.AncientSeal;

            if (hex != null
                && Menu.ItemToggler.Value.IsEnabled(hex.ToString())
                && hex.CanBeCasted
                && hex.CanHit(target))
            {
                return true;
            }
            else

            if (Menu.AbilityToggler.Value.IsEnabled(ancientSeal.ToString())
                && ancientSeal.CanBeCasted
                && !ancientSeal.CanHit(target))
            {
                return false;
            }

            return true;
        }

        private bool BadUlt(Hero target)
        {
            if (!Menu.BadUltItem)
            {
                return false;
            }

            if (Abilities.RodofAtos != null || Abilities.Hex != null || Abilities.Ethereal != null)
            {
                return false;
            }

            if (target.MovementSpeed < Menu.BadUltMovementSpeedItem)
            {
                return true;
            }

            return false;
        }
    }
}

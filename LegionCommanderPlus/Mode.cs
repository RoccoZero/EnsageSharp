using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Ensage;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Extensions;
using Ensage.SDK.Geometry;
using Ensage.SDK.Helpers;
using Ensage.SDK.Orbwalker.Modes;
using Ensage.SDK.Prediction;

using LegionCommanderPlus.Features;

namespace LegionCommanderPlus
{
    internal class Mode : KeyPressOrbwalkingModeAsync
    {
        private MenuManager Menu { get; }

        private LegionCommanderPlus Main { get; }

        private UpdateMode UpdateMode { get; }

        private LinkenBreaker LinkenBreaker { get; }

        private Extensions Extensions { get; }

        private MultiSleeper MultiSleeper { get; }

        public Mode(Key key, Config config)
            : base(config.Main.Context, key)
        {
            Menu = config.Menu;
            Main = config.Main;
            UpdateMode = config.UpdateMode;
            LinkenBreaker = config.LinkenBreaker;
            Extensions = config.Extensions;
            MultiSleeper = config.MultiSleeper;
        }

        public override async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                var target = UpdateMode.Target;
                if (target == null || Menu.BladeMailItem && target.HasModifier("modifier_item_blade_mail_reflect"))
                {
                    Orbwalker.Move(Game.MousePosition);
                    return;
                }

                if (!Owner.IsInvisible())
                {
                    var blockingAbilities = target.IsBlockingAbilities();
                    var comboBreaker = Extensions.ComboBreaker(target);

                    var cancelAdditionally = Extensions.CancelAdditionally(target);
                    if (Extensions.Cancel(target) && cancelAdditionally)
                    {
                        var modifiers = target.Modifiers.ToList();

                        if (!target.IsMagicImmune())
                        {
                            if (!blockingAbilities)
                            {
                                var stunDebuff = modifiers.FirstOrDefault(x => x.IsStunDebuff);
                                var hexDebuff = modifiers.FirstOrDefault(x => x.Name == "modifier_sheepstick_debuff");

                                // Abyssal Blade
                                var abyssalBlade = Main.AbyssalBlade;
                                if (abyssalBlade != null
                                    && Menu.ItemToggler.Value.IsEnabled(abyssalBlade.ToString())
                                    && abyssalBlade.CanBeCasted
                                    && Owner.Distance2D(target) < abyssalBlade.CastRange + 60
                                    && !comboBreaker
                                    && (stunDebuff == null || !stunDebuff.IsValid || stunDebuff.RemainingTime <= 0.3f)
                                    && (hexDebuff == null || !hexDebuff.IsValid || hexDebuff.RemainingTime <= 0.3f))
                                {
                                    abyssalBlade.UseAbility(target);
                                    await Task.Delay(abyssalBlade.GetCastDelay(target), token);
                                    return;
                                }

                                // Hex
                                var hex = Main.Hex;
                                if (hex != null
                                    && Menu.ItemToggler.Value.IsEnabled(hex.ToString())
                                    && hex.CanBeCasted
                                    && hex.CanHit(target)
                                    && !comboBreaker
                                    && (stunDebuff == null || !stunDebuff.IsValid || stunDebuff.RemainingTime <= 0.3f)
                                    && (hexDebuff == null || !hexDebuff.IsValid || hexDebuff.RemainingTime <= 0.3f))
                                {
                                    hex.UseAbility(target);
                                    await Task.Delay(hex.GetCastDelay(target), token);
                                }

                                // Orchid
                                var orchid = Main.Orchid;
                                if (orchid != null
                                    && Menu.ItemToggler.Value.IsEnabled(orchid.ToString())
                                    && orchid.CanBeCasted
                                    && orchid.CanHit(target)
                                    && !comboBreaker)
                                {
                                    orchid.UseAbility(target);
                                    await Task.Delay(orchid.GetCastDelay(target), token);
                                }

                                // Bloodthorn
                                var bloodthorn = Main.Bloodthorn;
                                if (bloodthorn != null
                                    && Menu.ItemToggler.Value.IsEnabled(bloodthorn.ToString())
                                    && bloodthorn.CanBeCasted
                                    && bloodthorn.CanHit(target)
                                    && !comboBreaker)
                                {
                                    bloodthorn.UseAbility(target);
                                    await Task.Delay(bloodthorn.GetCastDelay(target), token);
                                }

                                // Nullifier
                                var nullifier = Main.Nullifier;
                                if (nullifier != null
                                    && Menu.ItemToggler.Value.IsEnabled(nullifier.ToString())
                                    && nullifier.CanBeCasted
                                    && nullifier.CanHit(target)
                                    && !comboBreaker
                                    && (stunDebuff == null || !stunDebuff.IsValid || stunDebuff.RemainingTime <= 0.5f)
                                    && (hexDebuff == null || !hexDebuff.IsValid || hexDebuff.RemainingTime <= 0.5f))
                                {
                                    nullifier.UseAbility(target);
                                    await Task.Delay(nullifier.GetCastDelay(target), token);
                                }

                                // Atos
                                var atosDebuff = modifiers.Any(x => x.Name == "modifier_rod_of_atos_debuff" && x.RemainingTime > 0.5f);
                                var atos = Main.Atos;
                                if (atos != null
                                    && Menu.ItemToggler.Value.IsEnabled(atos.ToString())
                                    && atos.CanBeCasted
                                    && atos.CanHit(target)
                                    && !atosDebuff
                                    && (stunDebuff == null || !stunDebuff.IsValid || stunDebuff.RemainingTime <= 0.5f))
                                {
                                    atos.UseAbility(target);
                                    MultiSleeper.Sleep(atos.GetHitTime(target), "atos");
                                    await Task.Delay(atos.GetCastDelay(target), token);
                                }

                                // Veil
                                var veil = Main.Veil;
                                if (veil != null
                                    && Menu.ItemToggler.Value.IsEnabled(veil.ToString())
                                    && veil.CanBeCasted
                                    && veil.CanHit(target))
                                {
                                    veil.UseAbility(target.Position);
                                    await Task.Delay(veil.GetCastDelay(target.Position), token);
                                }

                                // Ethereal
                                var ethereal = Main.Ethereal;
                                if (ethereal != null
                                    && Menu.ItemToggler.Value.IsEnabled(ethereal.ToString())
                                    && ethereal.CanBeCasted
                                    && ethereal.CanHit(target)
                                    && !comboBreaker)
                                {
                                    ethereal.UseAbility(target);
                                    MultiSleeper.Sleep(ethereal.GetHitTime(target), "ethereal");
                                    await Task.Delay(ethereal.GetCastDelay(target), token);
                                }

                                // Shivas
                                var shivas = Main.Shivas;
                                if (shivas != null
                                    && Menu.ItemToggler.Value.IsEnabled(shivas.ToString())
                                    && shivas.CanBeCasted
                                    && shivas.CanHit(target))
                                {
                                    shivas.UseAbility();
                                    await Task.Delay(shivas.GetCastDelay(), token);
                                }

                                if (!MultiSleeper.Sleeping("ethereal") || target.IsEthereal())
                                {
                                    // Dagon
                                    var dagon = Main.Dagon;
                                    if (dagon != null
                                        && Menu.ItemToggler.Value.IsEnabled("item_dagon_5")
                                        && dagon.CanBeCasted
                                        && dagon.CanHit(target)
                                        && !comboBreaker)
                                    {
                                        dagon.UseAbility(target);
                                        await Task.Delay(dagon.GetCastDelay(target), token);
                                    }
                                }

                                // Urn
                                var urn = Main.Urn;
                                if (urn != null
                                    && Menu.ItemToggler.Value.IsEnabled(urn.ToString())
                                    && urn.CanBeCasted
                                    && urn.CanHit(target)
                                    && !comboBreaker
                                    && !modifiers.Any(x => x.Name == urn.TargetModifierName))
                                {
                                    urn.UseAbility(target);
                                    await Task.Delay(urn.GetCastDelay(target), token);
                                }

                                // Vessel
                                var vessel = Main.Vessel;
                                if (vessel != null
                                    && Menu.ItemToggler.Value.IsEnabled(vessel.ToString())
                                    && vessel.CanBeCasted
                                    && vessel.CanHit(target)
                                    && !comboBreaker
                                    && !modifiers.Any(x => x.Name == vessel.TargetModifierName))
                                {
                                    vessel.UseAbility(target);
                                    await Task.Delay(vessel.GetCastDelay(target), token);
                                }
                            }
                            else
                            {
                                LinkenBreaker.Handler.RunAsync();
                            }
                        }

                        if (!MultiSleeper.Sleeping("ethereal") || target.IsEthereal())
                        {
                            // Overwhelming Odds
                            var overwhelmingOdds = Main.OverwhelmingOdds;
                            if (Menu.AbilityToggler.Value.IsEnabled(overwhelmingOdds.ToString())
                                && overwhelmingOdds.CanBeCasted
                                && !comboBreaker)
                            {
                                var heroes = EntityManager<Hero>.Entities.Where(x => x.IsValid && x.IsVisible && x.IsAlive && x.IsEnemy(Owner) && !x.IsInvulnerable()).ToArray();
                                if (heroes.All(x => x.Distance2D(Owner) < overwhelmingOdds.CastRange + 300))
                                {
                                    var input = new PredictionInput
                                    {
                                        Owner = Owner,
                                        AreaOfEffect = overwhelmingOdds.HasAreaOfEffect,
                                        AreaOfEffectTargets = heroes,
                                        CollisionTypes = overwhelmingOdds.CollisionTypes,
                                        Delay = overwhelmingOdds.CastPoint + overwhelmingOdds.ActivationDelay,
                                        Speed = overwhelmingOdds.Speed,
                                        Range = overwhelmingOdds.CastRange,
                                        Radius = overwhelmingOdds.Radius,
                                        PredictionSkillshotType = overwhelmingOdds.PredictionSkillshotType
                                    };

                                    var castPosition = overwhelmingOdds.GetPredictionOutput(input.WithTarget(target)).CastPosition;
                                    if (Owner.Distance2D(castPosition) <= overwhelmingOdds.CastRange)
                                    {
                                        overwhelmingOdds.UseAbility(castPosition);
                                        await Task.Delay(overwhelmingOdds.GetCastDelay(castPosition), token);
                                    }
                                }
                            }
                        }
                    }

                    var blink = Main.Blink;
                    var distance = Owner.Distance2D(target, true);
                    var blinkReady = blink != null && Menu.ItemToggler.Value.IsEnabled(blink.ToString()) && blink.CanBeCasted;

                    if (cancelAdditionally)
                    {
                        if (distance <= (blinkReady ? blink.CastRange : 250))
                        {
                            // Press The Attack
                            var pressTheAttack = Main.PressTheAttack;
                            var pressTheAttackReady = Menu.AbilityToggler.Value.IsEnabled(pressTheAttack.ToString()) && pressTheAttack.CanBeCasted;
                            if (pressTheAttackReady)
                            {
                                pressTheAttack.UseAbility(Owner);
                                await Task.Delay(pressTheAttack.GetCastDelay(), token);
                            }

                            // Mjollnir
                            var mjollnir = Main.Mjollnir;
                            var mjollnirReady = mjollnir != null && Menu.ItemToggler.Value.IsEnabled(mjollnir.ToString()) && mjollnir.CanBeCasted;
                            if (mjollnirReady)
                            {
                                mjollnir.UseAbility(Owner);
                                await Task.Delay(mjollnir.GetCastDelay(), token);
                            }

                            // Armlet
                            var armlet = Main.Armlet;
                            var armletReady = armlet != null && Menu.ItemToggler.Value.IsEnabled(armlet.ToString()) && !armlet.Enabled;
                            if (armletReady)
                            {
                                armlet.Enabled = true;
                                await Task.Delay(armlet.GetCastDelay(), token);
                            }

                            // Blade Mail
                            var bladeMail = Main.BladeMail;
                            var bladeMailReady = bladeMail != null && Menu.ItemToggler.Value.IsEnabled(bladeMail.ToString()) && bladeMail.CanBeCasted && !comboBreaker;
                            if (bladeMailReady)
                            {
                                bladeMail.UseAbility();
                                await Task.Delay(bladeMail.GetCastDelay(), token);
                            }

                            // Satanic
                            var satanic = Main.Satanic;
                            var satanicReady = satanic != null && Menu.ItemToggler.Value.IsEnabled(satanic.ToString()) && satanic.CanBeCasted && !comboBreaker;
                            if (satanicReady)
                            {
                                satanic.UseAbility();
                                await Task.Delay(satanic.GetCastDelay(), token);
                            }

                            // Black King Bar
                            var blackKingBar = Main.BlackKingBar;
                            var blackKingBarReady = blackKingBar != null && Menu.ItemToggler.Value.IsEnabled(blackKingBar.ToString()) && blackKingBar.CanBeCasted && !comboBreaker;
                            if (blackKingBarReady)
                            {
                                blackKingBar.UseAbility();
                                await Task.Delay(blackKingBar.GetCastDelay(), token);
                            }

                            if (pressTheAttackReady || mjollnirReady || armletReady || bladeMailReady || satanicReady || blackKingBarReady)
                            {
                                await Task.Delay(125, token);
                                return;
                            }
                        }

                        if (!blockingAbilities)
                        {
                            // Duel
                            var duel = Main.Duel;
                            if (Menu.AbilityToggler.Value.IsEnabled(duel.ToString())
                                && duel.CanBeCasted
                                && Owner.Distance2D(target) < duel.CastRange + 50
                                && !comboBreaker)
                            {
                                duel.UseAbility(target);
                                await Task.Delay(duel.GetCastDelay(target), token);
                            }
                        }
                        else
                        {
                            LinkenBreaker.Handler.RunAsync();
                        }
                    }

                    // Blink
                    if (blinkReady && distance <= blink.CastRange && distance > 150)
                    {
                        blink.UseAbility(target.Position);
                        await Task.Delay(blink.GetCastDelay(target.Position), token);
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
                    else if(Menu.OrbwalkerItem.Value.SelectedValue.Contains("Free"))
                    {
                        var attackRange = Owner.AttackRange(target);
                        if (Owner.Distance2D(target) <= attackRange && !Menu.FullFreeModeItem || target.Distance2D(Game.MousePosition) <= attackRange)
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
                        if (Owner.Distance2D(target) <= Owner.AttackRange(target))
                        {
                            Orbwalker.Attack(target);
                        }
                    }
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
    }
}

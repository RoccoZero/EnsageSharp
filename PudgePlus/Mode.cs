using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Ensage;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Extensions;
using Ensage.SDK.Geometry;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;
using Ensage.SDK.Orbwalker.Modes;
using Ensage.SDK.Prediction;

using PlaySharp.Toolkit.Helper.Annotations;

using SharpDX;

namespace PudgePlus
{
    [PublicAPI]
    internal class Mode : KeyPressOrbwalkingModeAsync
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private PudgePlus Main { get; }

        private UpdateMode UpdateMode { get; }

        private Helpers Extensions { get; }

        private MultiSleeper MultiSleeper { get; }

        private IUpdateHandler HookUpdateHandler { get; }

        private Vector3 HookCastPosition { get; set; }

        private float HookStartCastTime { get; set; }

        private bool HookModifierDetected { get; set; }

        public Mode(Key key, Config config)
            : base(config.Main.Context, key)
        {
            Config = config;
            Menu = config.Menu;
            Main = config.Main;
            UpdateMode = config.UpdateMode;
            Extensions = config.Helpers;
            MultiSleeper = config.MultiSleeper;

            Entity.OnBoolPropertyChange += OnHookCast;
            Unit.OnModifierAdded += OnHookAdded;
            Unit.OnModifierRemoved += OnHookRemoved;
            HookUpdateHandler = UpdateManager.Subscribe(HookHitCheck, 0, false);
        }

        protected override void OnDeactivate()
        {
            UpdateManager.Unsubscribe(HookHitCheck);
            Unit.OnModifierRemoved -= OnHookRemoved;
            Unit.OnModifierAdded -= OnHookAdded;
            Entity.OnBoolPropertyChange -= OnHookCast;

            base.OnDeactivate();
        }

        public override async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                if (Owner.IsChanneling())
                {
                    return;
                }

                var target = UpdateMode.Target;
                if (target == null || Menu.BladeMailItem && target.HasModifier("modifier_item_blade_mail_reflect"))
                {
                    Orbwalker.Move(Game.MousePosition);
                    return;
                }

                // ForceStaff
                var hook = Main.Hook;
                var forceStaff = Main.ForceStaff;
                var blink = Main.Blink;
                if (forceStaff != null
                    && Menu.ItemToggler.Value.IsEnabled(forceStaff.ToString())
                    && !HookModifierDetected
                    && Owner.Distance2D(target) > 500
                    && forceStaff.CanBeCasted)
                {
                    var forceStaffHook = hook.CanBeCasted && Owner.Distance2D(target) < hook.CastRange + forceStaff.PushLength;

                    var blinkReady = blink != null && blink.CanBeCasted;
                    var forceStaffBlink = blinkReady && Owner.Distance2D(target) < blink.CastRange + forceStaff.PushLength;
                    if (forceStaffBlink && !blink.CanHit(target) || !blinkReady && forceStaffHook)
                    {
                        if (Owner.FindRotationAngle(target.Position) <= 0.3f)
                        {
                            forceStaff.UseAbility(Owner);
                            await Task.Delay((int)((forceStaff.PushLength / forceStaff.PushSpeed) * 1000), token);
                            return;
                        }

                        Owner.MoveToDirection(target.Position);
                        await Task.Delay(100);
                        return;
                    }
                }

                // Blink
                if (blink != null
                    && Menu.ItemToggler.Value.IsEnabled(blink.ToString())
                    && !HookModifierDetected
                    && blink.CanBeCasted
                    && Owner.Distance2D(target, true) <= blink.CastRange
                    && Owner.Distance2D(target) > 500)
                {
                    blink.UseAbility(target.Position);
                    await Task.Delay(blink.GetCastDelay(target.Position), token);
                    return;
                }

                var dismember = Main.Dismember;
                var comboBreaker = Extensions.ComboBreaker(target);
                var isBlockingAbilities = target.IsBlockingAbilities();

                var cancel = Extensions.Cancel(target);
                var cancelMagicImmune = Extensions.CancelMagicImmune(target);
                if (cancel || cancelMagicImmune)
                {
                    if (!isBlockingAbilities)
                    {
                        var modifiers = target.Modifiers.ToList();
                        var stunDebuff = modifiers.FirstOrDefault(x => x.IsStunDebuff);
                        var hexDebuff = modifiers.FirstOrDefault(x => x.Name == "modifier_sheepstick_debuff");

                        // Hex
                        var dismemberReady = !Menu.AbilityToggler.Value.IsEnabled(dismember.ToString()) || !dismember.CanBeCasted || !dismember.CanHit(target);
                        var hex = Main.Hex;
                        if (hex != null
                            && Menu.ItemToggler.Value.IsEnabled(hex.ToString())
                            && hex.CanBeCasted
                            && hex.CanHit(target)
                            && !HookModifierDetected
                            && dismemberReady
                            && !comboBreaker
                            && (stunDebuff == null || !stunDebuff.IsValid || stunDebuff.RemainingTime <= 0.3f)
                            && (hexDebuff == null || !hexDebuff.IsValid || hexDebuff.RemainingTime <= 0.3f))
                        {
                            hex.UseAbility(target);
                            await Task.Delay(hex.GetCastDelay(target), token);
                            return;
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
                        var hookReady = !Menu.AbilityToggler.Value.IsEnabled(hook.ToString()) || !hook.CanBeCasted;
                        var nullifier = Main.Nullifier;
                        if (nullifier != null
                            && Menu.ItemToggler.Value.IsEnabled(nullifier.ToString())
                            && nullifier.CanBeCasted
                            && nullifier.CanHit(target)
                            && !HookModifierDetected
                            && dismemberReady
                            && hookReady
                            && !comboBreaker
                            && (stunDebuff == null || !stunDebuff.IsValid || stunDebuff.RemainingTime <= 0.5f)
                            && (hexDebuff == null || !hexDebuff.IsValid || hexDebuff.RemainingTime <= 0.5f))
                        {
                            nullifier.UseAbility(target);
                            await Task.Delay(nullifier.GetCastDelay(target), token);
                        }

                        var atosDebuff = modifiers.Any(x => x.Name == "modifier_rod_of_atos_debuff" && x.RemainingTime > 0.5f);
                        var atos = Main.Atos;
                        if (atos != null
                            && Menu.ItemToggler.Value.IsEnabled(atos.ToString())
                            && atos.CanBeCasted
                            && atos.CanHit(target)
                            && !HookModifierDetected
                            && dismemberReady
                            && hookReady
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
                            // Dismember
                            if (Menu.AbilityToggler.Value.IsEnabled(dismember.ToString())
                                && dismember.CanBeCasted
                                && dismember.CanHit(target)
                                && !comboBreaker)
                            {
                                dismember.UseAbility(target);
                                await Task.Delay(dismember.GetCastDelay(target) + 50, token);
                                return;
                            }

                            // Hook
                            if (Menu.AbilityToggler.Value.IsEnabled(hook.ToString())
                                && hook.CanBeCasted
                                && hook.CanHit(target))
                            {
                                // Atos
                                var hookOutput = hook.GetPredictionOutput(hook.GetPredictionInput(target));
                                if (atos != null
                                    && Menu.ItemToggler.Value.IsEnabled(atos.ToString())
                                    && !HookModifierDetected
                                    && atos.CanBeCasted
                                    && atos.CanHit(target)
                                    && !atosDebuff
                                    && (stunDebuff == null || !stunDebuff.IsValid || stunDebuff.RemainingTime <= 0.5f))
                                {
                                    if (hookOutput.HitChance != HitChance.OutOfRange && hookOutput.HitChance != HitChance.Collision)
                                    {
                                        atos.UseAbility(target);
                                        MultiSleeper.Sleep(atos.GetHitTime(target), "atos");
                                        await Task.Delay(atos.GetCastDelay(target), token);
                                    }
                                }

                                if (Extensions.ShouldCastHook(hookOutput) && !MultiSleeper.Sleeping("atos") || target.HasModifier("modifier_rod_of_atos_debuff"))
                                {
                                    HookCastPosition = hookOutput.UnitPosition;
                                    hook.UseAbility(HookCastPosition);
                                    await Task.Delay(hook.GetCastDelay(HookCastPosition), token);
                                    return;
                                }
                            }

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
                                return;
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
                        Config.LinkenBreaker.Handler.RunAsync();
                    }
                }

                if (Menu.DismemberIsMagicImmune && cancel && !cancelMagicImmune && !isBlockingAbilities)
                {
                    // Dismember
                    if (Menu.AbilityToggler.Value.IsEnabled(dismember.ToString())
                        && dismember.CanBeCasted
                        && dismember.CanHit(target)
                        && !comboBreaker)
                    {
                        dismember.UseAbility(target);
                        await Task.Delay(dismember.GetCastDelay(target) + 50, token);
                        return;
                    }
                }

                if (isBlockingAbilities)
                {
                    // Hook
                    if (Menu.AbilityToggler.Value.IsEnabled(hook.ToString())
                        && hook.CanBeCasted
                        && hook.CanHit(target))
                    {
                        var hookOutput = hook.GetPredictionOutput(hook.GetPredictionInput(target));
                        if (Extensions.ShouldCastHook(hookOutput))
                        {
                            HookCastPosition = hookOutput.UnitPosition;
                            hook.UseAbility(HookCastPosition);
                            await Task.Delay(hook.GetCastDelay(HookCastPosition), token);
                        }
                    }
                }

                if (HookModifierDetected)
                {
                    return;
                }

                if (target.IsInvulnerable() || target.IsAttackImmune())
                {
                    Orbwalker.Move(Game.MousePosition);
                }
                else
                {
                    if (Menu.OrbwalkerItem.Value.SelectedValue.Contains("Free"))
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
                    else if (Menu.OrbwalkerItem.Value.SelectedValue.Contains("Default"))
                    {
                        Orbwalker.OrbwalkTo(target);
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

        private void OnHookCast(Entity sender, BoolPropertyChangeEventArgs args)
        {
            if (!Menu.ComboKeyItem)
            {
                return;
            }

            if (args.NewValue == args.OldValue || sender != Main.Hook || args.PropertyName != "m_bInAbilityPhase")
            {
                return;
            }

            if (args.NewValue)
            {
                HookStartCastTime = Game.RawGameTime;
                HookUpdateHandler.IsEnabled = true;
            }
            else
            {
                HookUpdateHandler.IsEnabled = false;
            }
        }

        private void HookHitCheck()
        {
            var target = UpdateMode.Target;
            if (target == null || !target.IsVisible)
            {
                return;
            }

            var hook = Main.Hook;
            var input = hook.GetPredictionInput(target);
            input.Delay = Math.Max((HookStartCastTime - Game.RawGameTime) + hook.CastPoint, 0);
            var output = hook.GetPredictionOutput(input);

            if (HookCastPosition.Distance2D(output.UnitPosition) > hook.Radius || !Extensions.ShouldCastHook(output))
            {
                Owner.Stop();
                Cancel();
                HookUpdateHandler.IsEnabled = false;
            }
        }

        private void OnHookAdded(Unit sender, ModifierChangedEventArgs args)
        {
            if (sender is Hero && Owner.IsEnemy(sender) && args.Modifier.Name == Main.Hook.TargetModifierName)
            {
                HookModifierDetected = true;
                Owner.Stop();
            }
        }

        private void OnHookRemoved(Unit sender, ModifierChangedEventArgs args)
        {
            if (Owner.IsEnemy(sender) && args.Modifier.Name == Main.Hook.TargetModifierName)
            {
                HookModifierDetected = false;
            }
        }
    }
}

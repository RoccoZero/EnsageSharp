using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Ensage;
using Ensage.Common.Threading;
using Ensage.SDK.Extensions;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;
using Ensage.SDK.Orbwalker;

using SharpDX;

namespace NyxAssassinPlus.Features
{
    internal class MaxStunMode
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private NyxAssassinPlus Main { get; set; }

        private UpdateMode UpdateMode { get; }

        private IOrbwalkerManager Orbwalker { get; }

        private Unit Owner { get; }

        private TaskHandler Handler { get; }

        public MaxStunMode(Config config)
        {
            Config = config;
            Menu = config.Menu;
            Main = config.Main;
            UpdateMode = config.UpdateMode;
            Orbwalker = config.Main.Context.Orbwalker;
            Owner = config.Main.Context.Owner;

            Handler = UpdateManager.Run(ExecuteAsync, true, false);

            config.Menu.MaxStunKeyItem.PropertyChanged += MaxStunKeyChanged;
        }

        private void MaxStunKeyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (Menu.MaxStunKeyItem)
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
                if (Game.IsPaused || !Owner.IsValid || !Owner.IsAlive || Owner.IsStunned() || !Owner.HasAghanimsScepter())
                {
                    return;
                }

                var target = Config.UpdateMode.Target;

                if (target != null && (!Menu.BladeMailItem || !target.HasModifier("modifier_item_blade_mail_reflect")))
                {
                    var StunDebuff = target.Modifiers.FirstOrDefault(x => x.IsStunDebuff);
                    var HexDebuff = target.Modifiers.FirstOrDefault(x => x.Name == "modifier_sheepstick_debuff");
                    var MultiSleeper = Config.MultiSleeper;
                    var SpikedCarapace = Main.SpikedCarapace;

                    if (!target.IsMagicImmune() && !target.IsInvulnerable() && (!Owner.IsInvisible() || !Main.Burrow.CanBeCasted)
                        && !target.HasAnyModifiers("modifier_abaddon_borrowed_time", "modifier_item_combo_breaker_buff")
                        && !target.HasAnyModifiers("modifier_winter_wyvern_winters_curse_aura", "modifier_winter_wyvern_winters_curse")
                        && SpikedCarapace.CanBeCasted)
                    {
                        var Impale = Main.Impale;

                        if (!target.IsBlockingAbilities())
                        {
                            // Impale
                            if (Impale.CanBeCasted
                                && Impale.CanHit(target)
                                && (StunDebuff == null || !StunDebuff.IsValid || StunDebuff.RemainingTime <= 0.5f)
                                && (HexDebuff == null || !HexDebuff.IsValid || HexDebuff.RemainingTime <= 0.5f))
                            {
                                var impalePos = Impale.GetPredictionOutput(Impale.GetPredictionInput(target)).CastPosition;
                                if (Owner.Distance2D(UpdateMode.BlinkPos) <= 200 && !UpdateMode.ImpalePos.IsZero)
                                {
                                    impalePos = UpdateMode.ImpalePos;
                                }

                                Impale.UseAbility(impalePos);
                                await Await.Delay(Impale.GetCastDelay(impalePos), token);
                            }

                            // ManaBurn
                            var ManaBurn = Main.ManaBurn;
                            if (ManaBurn.CanBeCasted
                                && ManaBurn.CanHit(target)
                                && target.Mana > 80)
                            {
                                ManaBurn.UseAbility(target);
                                await Await.Delay(ManaBurn.GetCastDelay(target), token);
                            }

                            if (Owner.Distance2D(target) < 250)
                            {
                                // Burrow
                                var Burrow = Main.Burrow;
                                if (!Config.MultiSleeper.Sleeping("Burrow")
                                    && Burrow.CanBeCasted
                                    && StunDebuff != null)
                                {
                                    Burrow.UseAbility();
                                    MultiSleeper.Sleep(Burrow.GetCastDelay() + 500, "Burrow");
                                    return;
                                }

                                // SpikedCarapace
                                if (!Burrow.CanBeCasted
                                    && (StunDebuff == null || !StunDebuff.IsValid || StunDebuff.RemainingTime <= 0.5f)
                                    && (HexDebuff == null || !HexDebuff.IsValid || HexDebuff.RemainingTime <= 0.5f))
                                {
                                    SpikedCarapace.UseAbility();
                                    await Await.Delay(SpikedCarapace.GetCastDelay(), token);
                                }
                            }
                        }
                        else
                        {
                            // Impale
                            if (Menu.AbilitiesToggler.Value.IsEnabled(Impale.ToString())
                                && Impale.CanBeCasted
                                && Impale.CanHit(target)
                                && (StunDebuff == null || !StunDebuff.IsValid || StunDebuff.RemainingTime <= 0.5f)
                                && (HexDebuff == null || !HexDebuff.IsValid || HexDebuff.RemainingTime <= 0.5f))
                            {
                                var impalePos = Impale.GetPredictionOutput(Impale.GetPredictionInput(target)).CastPosition;
                                if (Owner.Distance2D(UpdateMode.BlinkPos) <= 200 && !UpdateMode.ImpalePos.IsZero)
                                {
                                    impalePos = UpdateMode.ImpalePos;
                                }

                                Impale.UseAbility(impalePos);
                                await Await.Delay(Impale.GetCastDelay(impalePos), token);
                            }

                            Config.LinkenBreaker.Handler.RunAsync();
                        }
                    }

                    // UnBurrow
                    var UnBurrow = Main.UnBurrow;
                    if (UnBurrow.CanBeCasted && !SpikedCarapace.CanBeCasted)
                    {
                        UnBurrow.UseAbility();
                        await Await.Delay(UnBurrow.GetCastDelay(), token);
                    }

                    if (target.IsInvulnerable() || target.IsAttackImmune())
                    {
                        Orbwalker.Move(Game.MousePosition);
                    }
                    else
                    {
                        if (Menu.OrbwalkerItem.Value.SelectedValue.Contains("Default"))
                        {
                            Orbwalker.OrbwalkingPoint = Vector3.Zero;
                            Orbwalker.OrbwalkTo(target);
                        }
                        else if (Menu.OrbwalkerItem.Value.SelectedValue.Contains("Free"))
                        {
                            if (Owner.Distance2D(target) < Owner.AttackRange(target) && target.Distance2D(Game.MousePosition) < Owner.AttackRange(target))
                            {
                                Orbwalker.OrbwalkingPoint = Vector3.Zero;
                                Orbwalker.OrbwalkTo(target);
                            }
                            else
                            {
                                Orbwalker.Move(Game.MousePosition);
                            }
                        }
                    }
                }
                else
                {
                    Orbwalker.Move(Game.MousePosition);
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

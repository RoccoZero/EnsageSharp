using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Ensage;
using Ensage.Common.Threading;
using Ensage.SDK.Extensions;
using Ensage.SDK.Geometry;
using Ensage.SDK.Orbwalker.Modes;
using Ensage.SDK.Service;

using SharpDX;

using PlaySharp.Toolkit.Helper.Annotations;

namespace NyxAssassinPlus
{
    [PublicAPI]
    internal class Mode : KeyPressOrbwalkingModeAsync
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private NyxAssassinPlus Main { get; }

        private UpdateMode UpdateMode { get; }

        public Mode(IServiceContext context, Key key, Config config) : base(context, key)
        {
            Config = config;
            Menu = config.Menu;
            Main = config.Main;
            UpdateMode = config.UpdateMode;
        }

        public override async Task ExecuteAsync(CancellationToken token)
        {
            var target = UpdateMode.Target;

            if (target != null && (!Menu.BladeMailItem || !target.HasModifier("modifier_item_blade_mail_reflect")))
            {
                var StunDebuff = target.Modifiers.FirstOrDefault(x => x.IsStunDebuff);
                var HexDebuff = target.Modifiers.FirstOrDefault(x => x.Name =="modifier_sheepstick_debuff");
                var AtosDebuff = target.Modifiers.FirstOrDefault(x => x.Name == "modifier_rod_of_atos_debuff");
                var MultiSleeper = Config.MultiSleeper;

                if (!target.IsMagicImmune() && !target.IsInvulnerable() && (!Owner.IsInvisible() || !Main.Burrow.CanBeCasted)
                    && !target.HasAnyModifiers("modifier_abaddon_borrowed_time", "modifier_item_combo_breaker_buff")
                    && !target.HasAnyModifiers("modifier_winter_wyvern_winters_curse_aura", "modifier_winter_wyvern_winters_curse"))
                {
                    // Blink
                    var Impale = Main.Impale;
                    var Blink = Main.Blink;
                    if (Blink != null
                        && Menu.ItemsToggler.Value.IsEnabled(Blink.ToString())
                        && Blink.CanBeCasted
                        && Blink.CanHit(target)
                        && Impale.CanBeCasted)
                    {
                        var blinkPos = !UpdateMode.BlinkPos.IsZero ? UpdateMode.BlinkPos : target.Position;
                        if (blinkPos.Distance2D(Owner.Position) >= 200)
                        {
                            Blink.UseAbility(blinkPos);
                            await Await.Delay(Blink.GetCastDelay(blinkPos), token);
                        }
                    }

                    if (!target.IsBlockingAbilities())
                    {
                        // Hex
                        var Hex = Main.Hex;
                        if (Hex != null
                            && Menu.ItemsToggler.Value.IsEnabled(Hex.ToString())
                            && Hex.CanBeCasted
                            && Hex.CanHit(target)
                            && (StunDebuff == null || !StunDebuff.IsValid || StunDebuff.RemainingTime <= 0.3f)
                            && (HexDebuff == null || !HexDebuff.IsValid || HexDebuff.RemainingTime <= 0.3f))
                        {
                            Hex.UseAbility(target);
                            await Await.Delay(Hex.GetCastDelay(target), token);
                            return;
                        }

                        // Orchid
                        var Orchid = Main.Orchid;
                        if (Orchid != null
                            && Menu.ItemsToggler.Value.IsEnabled(Orchid.ToString())
                            && Orchid.CanBeCasted
                            && Orchid.CanHit(target))
                        {
                            Main.Orchid.UseAbility(target);
                            await Await.Delay(Main.Orchid.GetCastDelay(target), token);
                        }

                        // Bloodthorn
                        var Bloodthorn = Main.Bloodthorn;
                        if (Bloodthorn != null
                            && Menu.ItemsToggler.Value.IsEnabled(Bloodthorn.ToString())
                            && Bloodthorn.CanBeCasted
                            && Bloodthorn.CanHit(target))
                        {
                            Bloodthorn.UseAbility(target);
                            await Await.Delay(Bloodthorn.GetCastDelay(target), token);
                        }

                        // Nullifier
                        var Nullifier = Main.Nullifier;
                        if (Nullifier != null
                            && Menu.ItemsToggler.Value.IsEnabled(Nullifier.ToString())
                            && Nullifier.CanBeCasted
                            && Nullifier.CanHit(target)
                            && (StunDebuff == null || !StunDebuff.IsValid || StunDebuff.RemainingTime <= 0.5f)       
                            && (HexDebuff == null || !HexDebuff.IsValid || HexDebuff.RemainingTime <= 0.5f))
                        {
                            Nullifier.UseAbility(target);
                            await Await.Delay(Nullifier.GetCastDelay(target), token);
                        }

                        // RodofAtos
                        var RodofAtos = Main.RodofAtos;
                        if (RodofAtos != null
                            && Menu.ItemsToggler.Value.IsEnabled(RodofAtos.ToString())
                            && RodofAtos.CanBeCasted
                            && RodofAtos.CanHit(target)
                            && (StunDebuff == null || !StunDebuff.IsValid || StunDebuff.RemainingTime <= 0.5f)
                            && (AtosDebuff == null || !AtosDebuff.IsValid || AtosDebuff.RemainingTime <= 0.5f))
                        {
                            RodofAtos.UseAbility(target);
                            await Await.Delay(RodofAtos.GetCastDelay(target), token);
                        }

                        // Veil
                        var Veil = Main.Veil;
                        if (Veil != null
                            && Menu.ItemsToggler.Value.IsEnabled(Veil.ToString())
                            && Veil.CanBeCasted
                            && Veil.CanHit(target))
                        {
                            Veil.UseAbility(target.Position);
                            await Await.Delay(Veil.GetCastDelay(target.Position), token);
                        }

                        // Ethereal
                        var Ethereal = Main.Ethereal;
                        if (Ethereal != null
                            && Menu.ItemsToggler.Value.IsEnabled(Ethereal.ToString())
                            && Ethereal.CanBeCasted
                            && Ethereal.CanHit(target))
                        {
                            Ethereal.UseAbility(target);
                            MultiSleeper.Sleep(Ethereal.GetHitTime(target), "Ethereal");
                            await Await.Delay(Ethereal.GetCastDelay(target), token);
                        }

                        // Shivas
                        var Shivas = Main.Shivas;
                        if (Shivas != null
                            && Menu.ItemsToggler.Value.IsEnabled(Shivas.ToString())
                            && Shivas.CanBeCasted
                            && Shivas.CanHit(target))
                        {
                            Shivas.UseAbility();
                            await Await.Delay(Shivas.GetCastDelay(), token);
                        }

                        if (!MultiSleeper.Sleeping("Ethereal") || target.IsEthereal())
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

                            // ManaBurn
                            var ManaBurn = Main.ManaBurn;
                            if (Menu.AbilitiesToggler.Value.IsEnabled(ManaBurn.ToString())
                                && ManaBurn.CanBeCasted
                                && ManaBurn.CanHit(target)
                                && target.Mana > 80)
                            {
                                ManaBurn.UseAbility(target);
                                await Await.Delay(ManaBurn.GetCastDelay(target), token);
                            }

                            // Dagon
                            var Dagon = Main.Dagon;
                            if (Dagon != null
                                && Menu.ItemsToggler.Value.IsEnabled("item_dagon_5")
                                && Dagon.CanBeCasted
                                && Dagon.CanHit(target))
                            {
                                Dagon.UseAbility(target);
                                await Await.Delay(Dagon.GetCastDelay(target), token);
                            }
                        }

                        // UrnOfShadows
                        var UrnOfShadows = Main.UrnOfShadows;
                        if (UrnOfShadows != null
                            && Menu.ItemsToggler.Value.IsEnabled(UrnOfShadows.ToString())
                            && UrnOfShadows.CanBeCasted
                            && UrnOfShadows.CanHit(target))
                        {
                            UrnOfShadows.UseAbility(target);
                            await Await.Delay(UrnOfShadows.GetCastDelay(target), token);
                        }

                        // SpiritVessel
                        var SpiritVessel = Main.SpiritVessel;
                        if (SpiritVessel != null
                            && Menu.ItemsToggler.Value.IsEnabled(SpiritVessel.ToString())
                            && SpiritVessel.CanBeCasted
                            && SpiritVessel.CanHit(target))
                        {
                            SpiritVessel.UseAbility(target);
                            await Await.Delay(SpiritVessel.GetCastDelay(target), token);
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
    }
}

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Ensage;
using Ensage.Common.Threading;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Orbwalker.Modes;
using Ensage.SDK.Prediction;
using Ensage.SDK.Prediction.Collision;
using Ensage.SDK.Service;
using Ensage.SDK.TargetSelector;

using SharpDX;

namespace SkywrathMagePlus
{
    internal class Mode : KeyPressOrbwalkingModeAsync
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private SkywrathMagePlus Main { get; }

        private ITargetSelectorManager TargetSelector { get; }

        private IPredictionManager Prediction { get; }

        public Sleeper Sleeper { get; }

        public Mode(IServiceContext context, Key key, Config config) : base(context, key)
        {
            Config = config;
            Menu = config.Menu;
            Main = config.Main;
            TargetSelector = context.TargetSelector;
            Prediction = context.Prediction;

            Sleeper = new Sleeper();
        }

        public override async Task ExecuteAsync(CancellationToken token)
        {
            var target = Config.UpdateMode.Target;

            if (target != null && (!Menu.BladeMailItem || !target.HasModifier("modifier_item_blade_mail_reflect")))
            {
                var StunDebuff = target.Modifiers.FirstOrDefault(x => x.IsStunDebuff);
                var HexDebuff = target.Modifiers.FirstOrDefault(x => x.IsValid && x.Name == "modifier_sheepstick_debuff");
                var AtosDebuff = target.Modifiers.FirstOrDefault(x => x.IsValid && x.Name == "modifier_rod_of_atos_debuff");

                // Blink
                var Blink = Main.Blink;
                if (Blink != null
                    && Menu.ItemsToggler.Value.IsEnabled(Blink.ToString())
                    && Owner.Distance2D(Game.MousePosition) > Menu.BlinkActivationItem
                    && Owner.Distance2D(target) > 600
                    && Blink.CanBeCasted)
                {
                    var blinkPos = target.Position.Extend(Game.MousePosition, Menu.BlinkDistanceEnemyItem);
                    if (Owner.Distance2D(blinkPos) < Blink.CastRange)
                    {
                        Blink.UseAbility(blinkPos);
                        await Await.Delay(Blink.GetCastDelay(blinkPos), token);
                    }
                }

                if (!Config.Extensions.Cancel(target))
                {
                    if (!target.IsLinkensProtected() && !Config.Extensions.AntimageShield(target))
                    {
                        // Hex
                        var Hex = Main.Hex;
                        if (Hex != null
                            && Menu.ItemsToggler.Value.IsEnabled(Hex.ToString())
                            && Hex.CanBeCasted
                            && Hex.CanHit(target)
                            && (StunDebuff == null || StunDebuff.RemainingTime <= 0.3f)
                            && (HexDebuff == null || HexDebuff.RemainingTime <= 0.3f))
                        {
                            Hex.UseAbility(target);
                            await Await.Delay(Hex.GetCastDelay(target), token);
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

                        // MysticFlare
                        var MysticFlare = Main.MysticFlare;
                        if (Menu.AbilitiesToggler.Value.IsEnabled(MysticFlare.ToString())
                            && Main.MysticFlare.CanBeCasted
                            && Main.MysticFlare.CanHit(target)
                            && Config.Extensions.Active(target))
                        {
                            var enemies = EntityManager<Hero>.Entities.Where(x =>
                                                                             x.IsVisible &&
                                                                             x.IsAlive &&
                                                                             x.IsValid &&
                                                                             !x.IsIllusion &&
                                                                             x.IsEnemy(Owner) &&
                                                                             x.Distance2D(Owner) <= Main.MysticFlare.CastRange).ToList();

                            var ultimateScepter = Owner.HasAghanimsScepter();
                            var dubleMysticFlare = ultimateScepter && enemies.Count() == 1;

                            var Input =
                                new PredictionInput(
                                    Owner,
                                    target,
                                    0,
                                    float.MaxValue,
                                    MysticFlare.CastRange,
                                    dubleMysticFlare ? -250 : -100,
                                    PredictionSkillshotType.SkillshotCircle,
                                    true)
                                {
                                    CollisionTypes = CollisionTypes.None
                                };

                            var Output = Prediction.GetPrediction(Input);

                            MysticFlare.UseAbility(Output.CastPosition);
                            await Await.Delay(MysticFlare.GetCastDelay(Output.CastPosition), token);
                        }

                        // RodofAtos
                        var RodofAtos = Main.RodofAtos;
                        if (RodofAtos != null
                            && Menu.ItemsToggler.Value.IsEnabled(RodofAtos.ToString())
                            && RodofAtos.CanBeCasted
                            && RodofAtos.CanHit(target)
                            && (StunDebuff == null || StunDebuff.RemainingTime <= 0.5f)
                            && (AtosDebuff == null || AtosDebuff.RemainingTime <= 0.5f))
                        {
                            RodofAtos.UseAbility(target);
                            await Await.Delay(RodofAtos.GetCastDelay(target), token);
                        }

                        // AncientSeal
                        var AncientSeal = Main.AncientSeal;
                        if (Menu.AbilitiesToggler.Value.IsEnabled(AncientSeal.ToString())
                            && AncientSeal.CanBeCasted
                            && AncientSeal.CanHit(target))
                        {
                            AncientSeal.UseAbility(target);
                            await Await.Delay(AncientSeal.GetCastDelay(target), token);
                            return;
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
                            Sleeper.Sleep(Ethereal.GetHitTime(target));
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

                        if (!Sleeper.Sleeping || target.IsEthereal())
                        {
                            // ConcussiveShot
                            var ConcussiveShot = Main.ConcussiveShot;
                            if (Menu.AbilitiesToggler.Value.IsEnabled(ConcussiveShot.ToString())
                                && (!Menu.WTargetItem
                                || (target == Config.UpdateMode.WShowTarget
                                || (Config.UpdateMode.WShowTarget != null && target.Distance2D(Config.UpdateMode.WShowTarget) < 250)))
                                && ConcussiveShot.CanBeCasted
                                && Owner.Distance2D(target) < Menu.WRadiusItem - Owner.HullRadius)
                            {
                                ConcussiveShot.UseAbility();
                                await Await.Delay(ConcussiveShot.GetCastDelay(), token);
                            }

                            // ArcaneBolt
                            var ArcaneBolt = Main.ArcaneBolt;
                            if (Menu.AbilitiesToggler.Value.IsEnabled(ArcaneBolt.ToString())
                                && ArcaneBolt.CanBeCasted
                                && ArcaneBolt.CanHit(target))
                            {
                                ArcaneBolt.UseAbility(target);
                                await Await.Delay(ArcaneBolt.GetCastDelay(target), token);
                                return;
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
                                return;
                            }
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
                        Orbwalker.OrbwalkingPoint = Vector3.Zero;
                        Orbwalker.OrbwalkTo(target);
                    }
                    else if (Menu.OrbwalkerItem.Value.SelectedValue.Contains("Distance"))
                    {
                        var ownerDis = Math.Min(Owner.Distance2D(Game.MousePosition), 230);
                        var ownerPos = Owner.Position.Extend(Game.MousePosition, ownerDis);
                        var pos = target.Position.Extend(ownerPos, Menu.MinDisInOrbwalkItem);

                        Orbwalker.OrbwalkTo(target);
                        Orbwalker.OrbwalkingPoint = pos;
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

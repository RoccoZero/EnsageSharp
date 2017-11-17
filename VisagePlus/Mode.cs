using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Ensage;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.Common.Threading;
using Ensage.SDK.Extensions;
using Ensage.SDK.Orbwalker.Modes;

using PlaySharp.Toolkit.Helper.Annotations;

using SharpDX;

namespace VisagePlus
{
    [PublicAPI]
    internal class Mode : KeyPressOrbwalkingModeAsync
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private VisagePlus Main { get; }

        private MultiSleeper MultiSleeper { get; }

        private Extensions Extensions { get; }

        public Mode(Key key, Config config) 
            : base(config.Main.Context, key)
        {
            Config = config;
            Menu = config.Menu;
            Main = config.Main;
            MultiSleeper = config.MultiSleeper;
            Extensions = config.Extensions;
        }

        public override async Task ExecuteAsync(CancellationToken token)
        {
            var target = Config.UpdateMode.Target;

            if (target != null && (!Menu.BladeMailItem || !target.HasModifier("modifier_item_blade_mail_reflect")))
            {
                var stunDebuff = target.Modifiers.FirstOrDefault(x => x.IsStunDebuff);
                var hexDebuff = target.Modifiers.FirstOrDefault(x => x.Name == "modifier_sheepstick_debuff");
                var atosDebuff = target.Modifiers.FirstOrDefault(x => x.Name == "modifier_rod_of_atos_debuff");
                var modifierHurricanePike = Owner.HasModifier("modifier_item_hurricane_pike_range");

                // Blink
                var blink = Main.Blink;
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

                if (Extensions.Cancel(target))
                {
                    if (!target.IsBlockingAbilities())
                    {
                        // Hex
                        var hex = Main.Hex;
                        if (hex != null
                            && Menu.ItemToggler.Value.IsEnabled(hex.ToString())
                            && hex.CanBeCasted
                            && hex.CanHit(target)
                            && (stunDebuff == null || !stunDebuff.IsValid || stunDebuff.RemainingTime <= 0.3f)
                            && (hexDebuff == null || !hexDebuff.IsValid || hexDebuff.RemainingTime <= 0.3f))
                        {
                            hex.UseAbility(target);
                            await Await.Delay(hex.GetCastDelay(target), token);
                        }

                        // Orchid
                        var orchid = Main.Orchid;
                        if (orchid != null
                            && Menu.ItemToggler.Value.IsEnabled(orchid.ToString())
                            && orchid.CanBeCasted
                            && orchid.CanHit(target))
                        {
                            Main.Orchid.UseAbility(target);
                            await Await.Delay(Main.Orchid.GetCastDelay(target), token);
                        }

                        // Bloodthorn
                        var bloodthorn = Main.Bloodthorn;
                        if (bloodthorn != null
                            && Menu.ItemToggler.Value.IsEnabled(bloodthorn.ToString())
                            && bloodthorn.CanBeCasted
                            && bloodthorn.CanHit(target))
                        {
                            bloodthorn.UseAbility(target);
                            await Await.Delay(bloodthorn.GetCastDelay(target), token);
                        }

                        // Nullifier
                        var nullifier = Main.Nullifier;
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
                        var rodofAtos = Main.RodofAtos;
                        if (rodofAtos != null
                            && Menu.ItemToggler.Value.IsEnabled(rodofAtos.ToString())
                            && rodofAtos.CanBeCasted
                            && rodofAtos.CanHit(target)
                            && (stunDebuff == null || !stunDebuff.IsValid || stunDebuff.RemainingTime <= 0.5f)
                            && (atosDebuff == null || !atosDebuff.IsValid || atosDebuff.RemainingTime <= 0.5f))
                        {
                            rodofAtos.UseAbility(target);
                            await Await.Delay(rodofAtos.GetCastDelay(target), token);
                        }

                        // GraveChill
                        var graveChill = Main.GraveChill;
                        if (Menu.AbilityToggler.Value.IsEnabled(graveChill.ToString())
                            && graveChill.CanBeCasted
                            && graveChill.CanHit(target))
                        {
                            graveChill.UseAbility(target);
                            await Await.Delay(graveChill.GetCastDelay(target), token);
                        }

                        // HurricanePike
                        var hurricanePike = Main.HurricanePike;
                        if (hurricanePike != null
                            && Menu.ItemToggler.Value.IsEnabled(hurricanePike.ToString())
                            && hurricanePike.CanBeCasted
                            && hurricanePike.CanHit(target)
                            && !MultiSleeper.Sleeping("ethereal") 
                            && !target.IsEthereal())
                        {
                            hurricanePike.UseAbility(target);
                            await Await.Delay(hurricanePike.GetCastDelay(target), token);
                            return;
                        }

                        // HeavensHalberd
                        var heavensHalberd = Main.HeavensHalberd;
                        if (heavensHalberd != null
                            && Menu.ItemToggler.Value.IsEnabled(heavensHalberd.ToString())
                            && heavensHalberd.CanBeCasted
                            && heavensHalberd.CanHit(target))
                        {
                            heavensHalberd.UseAbility(target);
                            await Await.Delay(heavensHalberd.GetCastDelay(target), token);
                        }

                        // Veil
                        var veil = Main.Veil;
                        if (veil != null
                            && Menu.ItemToggler.Value.IsEnabled(veil.ToString())
                            && veil.CanBeCasted
                            && veil.CanHit(target))
                        {
                            veil.UseAbility(target.Position);
                            await Await.Delay(veil.GetCastDelay(target.Position), token);
                        }

                        // Ethereal
                        var ethereal = Main.Ethereal;
                        if (ethereal != null
                            && Menu.ItemToggler.Value.IsEnabled(ethereal.ToString())
                            && ethereal.CanBeCasted
                            && ethereal.CanHit(target)
                            && !modifierHurricanePike)
                        {
                            ethereal.UseAbility(target);
                            MultiSleeper.Sleep(ethereal.GetHitTime(target), "ethereal");
                            await Await.Delay(ethereal.GetCastDelay(target), token);
                        }

                        // Shivas
                        var shivas = Main.Shivas;
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
                            // SoulAssumption
                            var SoulAssumption = Main.SoulAssumption;
                            if (Menu.AbilityToggler.Value.IsEnabled(SoulAssumption.ToString())
                                && SoulAssumption.CanBeCasted
                                && SoulAssumption.CanHit(target)
                                && SoulAssumption.MaxCharges)
                            {
                                SoulAssumption.UseAbility(target);
                                await Await.Delay(SoulAssumption.GetCastDelay(target), token);
                                return;
                            }

                            // Dagon
                            var Dagon = Main.Dagon;
                            if (Dagon != null
                                && Menu.ItemToggler.Value.IsEnabled("item_dagon_5")
                                && Dagon.CanBeCasted
                                && Dagon.CanHit(target))
                            {
                                Dagon.UseAbility(target);
                                await Await.Delay(Dagon.GetCastDelay(target), token);
                                return;
                            }
                        }

                        // Medallion
                        var medallion = Main.Medallion;
                        if (medallion != null
                            && Menu.ItemToggler.Value.IsEnabled(medallion.ToString())
                            && medallion.CanBeCasted
                            && medallion.CanHit(target))
                        {
                            medallion.UseAbility(target);
                            await Await.Delay(medallion.GetCastDelay(target), token);
                        }

                        // SolarCrest
                        var solarCrest = Main.SolarCrest;
                        if (solarCrest != null
                            && Menu.ItemToggler.Value.IsEnabled(solarCrest.ToString())
                            && solarCrest.CanBeCasted
                            && solarCrest.CanHit(target))
                        {
                            solarCrest.UseAbility(target);
                            await Await.Delay(solarCrest.GetCastDelay(target), token);
                        }

                        // UrnOfShadows
                        var urnOfShadows = Main.UrnOfShadows;
                        if (urnOfShadows != null
                            && Menu.ItemToggler.Value.IsEnabled(urnOfShadows.ToString())
                            && urnOfShadows.CanBeCasted
                            && urnOfShadows.CanHit(target))
                        {
                            urnOfShadows.UseAbility(target);
                            await Await.Delay(urnOfShadows.GetCastDelay(target), token);
                        }

                        // SpiritVessel
                        var spiritVessel = Main.SpiritVessel;
                        if (spiritVessel != null
                            && Menu.ItemToggler.Value.IsEnabled(spiritVessel.ToString())
                            && spiritVessel.CanBeCasted
                            && spiritVessel.CanHit(target))
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

                // Necronomicon
                var necronomicon = Main.Necronomicon;
                if (necronomicon != null
                    && Menu.ItemToggler.Value.IsEnabled("item_necronomicon_3")
                    && necronomicon.CanBeCasted
                    && Owner.Distance2D(target) <= Owner.AttackRange)
                {
                    necronomicon.UseAbility();
                    await Await.Delay(necronomicon.GetCastDelay(), token);
                }

                // Armlet
                var armlet = Main.Armlet;
                if (armlet != null
                    && Menu.ItemToggler.Value.IsEnabled(armlet.ToString())
                    && !armlet.Enabled
                    && Owner.Distance2D(target) <= Owner.AttackRange)
                {
                    armlet.UseAbility();
                    await Await.Delay(armlet.GetCastDelay(), token);
                }

                if (target.IsInvulnerable() || target.IsAttackImmune())
                {
                    Orbwalker.Move(Game.MousePosition);
                }
                else
                {
                    if (Menu.OrbwalkerItem.Value.SelectedValue.Contains("Only Attack") || modifierHurricanePike)
                    {
                        Orbwalker.Attack(target);
                    }
                    else if(Menu.OrbwalkerItem.Value.SelectedValue.Contains("Default"))
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
                    else if (Menu.OrbwalkerItem.Value.SelectedValue.Contains("No Move"))
                    {
                        if (Owner.Distance2D(target) < Owner.AttackRange(target))
                        {
                            Orbwalker.Attack(target);
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

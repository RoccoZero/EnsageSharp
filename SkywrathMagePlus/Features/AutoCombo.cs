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
using Ensage.SDK.Prediction;
using Ensage.SDK.Prediction.Collision;

namespace SkywrathMagePlus.Features
{
    internal class AutoCombo
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private SkywrathMagePlus Main { get; }

        private Unit Owner { get; }

        private IPredictionManager Prediction { get; }

        private TaskHandler Handler { get; }

        public AutoCombo(Config config)
        {
            Config = config;
            Menu = config.Menu;
            Main = config.Main;
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
                if (Game.IsPaused || !Owner.IsValid || !Owner.IsAlive || Owner.IsStunned() || Menu.ComboKeyItem)
                {
                    return;
                }

                var target = EntityManager<Hero>.Entities.FirstOrDefault(x =>
                                                                         x.IsVisible &&
                                                                         x.IsAlive &&
                                                                         !x.IsIllusion &&
                                                                         x.IsValid &&
                                                                         x.IsEnemy(Owner) &&
                                                                         Config.Extensions.Active(x));

                if (target == null)
                {
                    return;
                }

                if (!Menu.BladeMailItem || !target.HasModifier("modifier_item_blade_mail_reflect"))
                {
                    var StunDebuff = target.Modifiers.FirstOrDefault(x => x.IsStunDebuff);
                    var HexDebuff = target.Modifiers.FirstOrDefault(x => x.IsValid && x.Name == "modifier_sheepstick_debuff");
                    var AtosDebuff = target.Modifiers.FirstOrDefault(x => x.IsValid && x.Name == "modifier_rod_of_atos_debuff");
                    var Sleeper = Config.Mode.Sleeper;

                    if (Config.Extensions.Cancel(target) || Owner.IsInvisible())
                    {
                        return;
                    }

                    if (!target.IsLinkensProtected() && !Config.Extensions.AntimageShield(target))
                    {
                        // Hex
                        var Hex = Main.Hex;
                        if (Hex != null
                            && Menu.AutoItemsToggler.Value.IsEnabled(Hex.ToString())
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
                            && Menu.AutoItemsToggler.Value.IsEnabled(Orchid.ToString())
                            && Orchid.CanBeCasted
                            && Orchid.CanHit(target))
                        {
                            Main.Orchid.UseAbility(target);
                            await Await.Delay(Main.Orchid.GetCastDelay(target), token);
                        }

                        // Bloodthorn
                        var Bloodthorn = Main.Bloodthorn;
                        if (Bloodthorn != null
                            && Menu.AutoItemsToggler.Value.IsEnabled(Bloodthorn.ToString())
                            && Bloodthorn.CanBeCasted
                            && Bloodthorn.CanHit(target))
                        {
                            Bloodthorn.UseAbility(target);
                            await Await.Delay(Bloodthorn.GetCastDelay(target), token);
                        }

                        // MysticFlare
                        var MysticFlare = Main.MysticFlare;
                        if (Menu.AutoAbilitiesToggler.Value.IsEnabled(MysticFlare.ToString())
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
                            && Menu.AutoItemsToggler.Value.IsEnabled(RodofAtos.ToString())
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
                        if (Menu.AutoAbilitiesToggler.Value.IsEnabled(AncientSeal.ToString())
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
                            && Menu.AutoItemsToggler.Value.IsEnabled(Veil.ToString())
                            && Veil.CanBeCasted
                            && Veil.CanHit(target))
                        {
                            Veil.UseAbility(target.Position);
                            await Await.Delay(Veil.GetCastDelay(target.Position), token);
                        }

                        // Ethereal
                        var Ethereal = Main.Ethereal;
                        if (Ethereal != null
                            && Menu.AutoItemsToggler.Value.IsEnabled(Ethereal.ToString())
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
                            && Menu.AutoItemsToggler.Value.IsEnabled(Shivas.ToString())
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
                            if (Menu.AutoAbilitiesToggler.Value.IsEnabled(ConcussiveShot.ToString())
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
                            if (Menu.AutoAbilitiesToggler.Value.IsEnabled(ArcaneBolt.ToString())
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
                                && Menu.AutoItemsToggler.Value.IsEnabled("item_dagon_5")
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

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
    internal class AutoKillSteal
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private SkywrathMagePlus Main { get; }

        private DamageCalculation DamageCalculation { get; }

        private Unit Owner { get; }

        private DamageCalculation.Damage Damage { get; set; }

        private TaskHandler Handler { get; }

        private IUpdateHandler Update { get; set; }
        
        public AutoKillSteal(Config config)
        {
            Config = config;
            Menu = config.Menu;
            Main = config.Main;
            DamageCalculation = config.DamageCalculation;
            Owner = config.Main.Context.Owner;

            Handler = UpdateManager.Run(ExecuteAsync, true, false);

            if (Menu.AutoKillStealItem)
            {
                Handler.RunAsync();
            }

            config.Menu.AutoKillStealItem.PropertyChanged += AutoKillStealChanged;

            Update = UpdateManager.Subscribe(Stop, 0, false);
        }

        public void Dispose()
        {
            Menu.AutoKillStealItem.PropertyChanged -= AutoKillStealChanged;

            if (Menu.AutoKillStealItem)
            {
                Handler?.Cancel();
            }
        }

        private void AutoKillStealChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Menu.AutoKillStealItem)
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
                if (Game.IsPaused || !Owner.IsValid || !Owner.IsAlive || Owner.IsStunned())
                {
                    return;
                }

                var damageCalculation = DamageCalculation.DamageList.Where(x => (x.GetHealth - x.GetDamage) / x.GetTarget.MaximumHealth <= 0.0f).ToList();
                Damage = damageCalculation.OrderByDescending(x => x.GetHealth).OrderByDescending(x => x.GetTarget.Player.Kills).FirstOrDefault();

                if (Damage == null)
                {
                    return;
                }

                if (!Update.IsEnabled)
                {
                    Update.IsEnabled = true;
                }

                var target = Damage.GetTarget;
                var multiSleeper = Config.MultiSleeper;

                if (!Cancel(target))
                {
                    return;
                }

                if (!target.IsBlockingAbilities())
                {
                    // AncientSeal
                    var AncientSeal = Main.AncientSeal;
                    if (Menu.AutoKillStealToggler.Value.IsEnabled(AncientSeal.ToString())
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
                        && Menu.AutoKillStealToggler.Value.IsEnabled(Veil.ToString())
                        && Veil.CanBeCasted
                        && Veil.CanHit(target))
                    {
                        Veil.UseAbility(target.Position);
                        await Await.Delay(Veil.GetCastDelay(target.Position), token);
                    }

                    // Ethereal
                    var Ethereal = Main.Ethereal;
                    if (Ethereal != null
                        && Menu.AutoKillStealToggler.Value.IsEnabled(Ethereal.ToString())
                        && Ethereal.CanBeCasted
                        && Ethereal.CanHit(target))
                    {
                        Ethereal.UseAbility(target);
                        multiSleeper.Sleep(Ethereal.GetHitTime(target), "ethereal");
                        await Await.Delay(Ethereal.GetCastDelay(target), token);
                    }

                    // Shivas
                    var Shivas = Main.Shivas;
                    if (Shivas != null
                        && Menu.AutoKillStealToggler.Value.IsEnabled(Shivas.ToString())
                        && Shivas.CanBeCasted
                        && Shivas.CanHit(target))
                    {
                        Shivas.UseAbility();
                        await Await.Delay(Shivas.GetCastDelay(), token);
                    }

                    if (!multiSleeper.Sleeping("ethereal") || target.IsEthereal())
                    {
                        // ConcussiveShot
                        var ConcussiveShot = Main.ConcussiveShot;
                        if (Menu.AutoKillStealToggler.Value.IsEnabled(ConcussiveShot.ToString())
                            && target == ConcussiveShot.TargetHit
                            && ConcussiveShot.CanBeCasted
                            && ConcussiveShot.CanHit(target))
                        {
                            ConcussiveShot.UseAbility();
                            await Await.Delay(ConcussiveShot.GetCastDelay(), token);
                        }

                        // ArcaneBolt
                        var ArcaneBolt = Main.ArcaneBolt;
                        if (Menu.AutoKillStealToggler.Value.IsEnabled(ArcaneBolt.ToString())
                            && ArcaneBolt.CanBeCasted
                            && ArcaneBolt.CanHit(target))
                        {
                            ArcaneBolt.UseAbility(target);

                            UpdateManager.BeginInvoke(() =>
                            {
                                multiSleeper.Sleep(ArcaneBolt.GetHitTime(target) - (ArcaneBolt.GetCastDelay(target) + 350), $"arcanebolt_{ target.Name }");
                            },
                            ArcaneBolt.GetCastDelay(target) + 50);

                            await Await.Delay(ArcaneBolt.GetCastDelay(target), token);
                            return;
                        }

                        // Dagon
                        var Dagon = Main.Dagon;
                        if (Dagon != null
                            && Menu.AutoKillStealToggler.Value.IsEnabled("item_dagon_5")
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
            catch (TaskCanceledException)
            {
                // canceled
            }
            catch (Exception e)
            {
                Main.Log.Error(e);
            }
        }

        private bool Cancel(Hero target)
        {
            return !Owner.IsInvisible()
                && !target.IsMagicImmune()
                && !target.IsInvulnerable()
                && !target.HasAnyModifiers("modifier_dazzle_shallow_grave", "modifier_necrolyte_reapers_scythe")
                && !Config.Extensions.DuelAghanimsScepter(target)
                && !Reincarnation(target);
        }

        private bool Reincarnation(Hero target)
        {
            var reincarnation = target.GetAbilityById(AbilityId.skeleton_king_reincarnation);

            return reincarnation != null && reincarnation.Cooldown == 0 && reincarnation.Level > 0;
        }

        private void Stop()
        {
            if (Damage == null)
            {
                Update.IsEnabled = false;
                return;
            }

            var stop = EntityManager<Hero>.Entities.Any(x => !x.IsAlive && x == Damage.GetTarget);
            if (stop && Owner.Animation.Name.Contains("cast"))
            {
                Owner.Stop();
                Update.IsEnabled = false;
            }
        }
    }
}
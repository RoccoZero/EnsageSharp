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

namespace VisagePlus.Features
{
    internal class AutoKillSteal
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private VisagePlus Main { get; }

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

            if (config.Menu.AutoKillStealItem)
            {
                Handler.RunAsync();
            }

            config.Menu.AutoKillStealItem.PropertyChanged += AutoKillStealChanged;

            Update = UpdateManager.Subscribe(Stop, 0, false);
        }

        public void Dispose()
        {
            UpdateManager.Unsubscribe(Stop);

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
                    // Veil
                    var veil = Main.Veil;
                    if (veil != null
                        && Menu.AutoKillStealToggler.Value.IsEnabled(veil.ToString())
                        && veil.CanBeCasted
                        && veil.CanHit(target))
                    {
                        veil.UseAbility(target.Position);
                        await Await.Delay(veil.GetCastDelay(target.Position), token);
                    }

                    // Ethereal
                    var ethereal = Main.Ethereal;
                    if (ethereal != null
                        && Menu.AutoKillStealToggler.Value.IsEnabled(ethereal.ToString())
                        && ethereal.CanBeCasted
                        && ethereal.CanHit(target))
                    {
                        ethereal.UseAbility(target);
                        multiSleeper.Sleep(ethereal.GetHitTime(target), "ethereal");
                        await Await.Delay(ethereal.GetCastDelay(target), token);
                    }

                    // Shivas
                    var shivas = Main.Shivas;
                    if (shivas != null
                        && Menu.AutoKillStealToggler.Value.IsEnabled(shivas.ToString())
                        && shivas.CanBeCasted
                        && shivas.CanHit(target))
                    {
                        shivas.UseAbility();
                        await Await.Delay(shivas.GetCastDelay(), token);
                    }

                    if (!multiSleeper.Sleeping("ethereal") || target.IsEthereal())
                    {
                        // SoulAssumption
                        var soulAssumption = Main.SoulAssumption;
                        if (Menu.AutoKillStealToggler.Value.IsEnabled(soulAssumption.ToString())
                            && soulAssumption.CanBeCasted
                            && soulAssumption.CanHit(target)
                            && soulAssumption.MaxCharges)
                        {
                            soulAssumption.UseAbility(target);
                            await Await.Delay(soulAssumption.GetCastDelay(target), token);
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
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

namespace NyxAssassinPlus.Features
{
    internal class AutoKillSteal
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private NyxAssassinPlus Main { get; }

        private DamageCalculation DamageCalculation { get; }

        private Unit Owner { get; }

        private TaskHandler Handler { get; }

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

                var damageCalculation = DamageCalculation.DamageList.OrderByDescending(x => x.GetHealth).OrderByDescending(x => x.GetHero.Player.Kills).ToList();
                var damage = damageCalculation.FirstOrDefault(x => (x.GetHealth - x.GetDamage) / x.GetHero.MaximumHealth <= 0.0f);

                if (damage == null)
                {
                    return;
                }

                var target = damage.GetHero;

                if (Cancel(target))
                {
                    return;
                }

                if (!target.IsBlockingAbilities())
                {
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
                        Config.MultiSleeper.Sleep(Ethereal.GetHitTime(target), "Ethereal");
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

                    if (!Config.MultiSleeper.Sleeping("Ethereal") || target.IsEthereal())
                    {
                        // Impale
                        var Impale = Main.Impale;
                        if (Menu.AutoKillStealToggler.Value.IsEnabled(Impale.ToString())
                            && Impale.CanBeCasted
                            && Impale.CanHit(target))
                        {
                            Impale.UseAbility(target.Position);
                            await Await.Delay(Impale.GetCastDelay(target.Position), token);
                        }

                        // Mana Burn
                        var ManaBurn = Main.ManaBurn;
                        if (Menu.AutoKillStealToggler.Value.IsEnabled(ManaBurn.ToString())
                            && ManaBurn.CanBeCasted
                            && ManaBurn.CanHit(target)
                            && target.Mana > 80)
                        {
                            ManaBurn.UseAbility(target);
                            await Await.Delay(ManaBurn.GetCastDelay(target), token);
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
            var duelAghanimsScepter = false;
            if (target.HasModifier("modifier_legion_commander_duel"))
            {
                duelAghanimsScepter = EntityManager<Hero>.Entities.Any(x => 
                                                                       x.HeroId == HeroId.npc_dota_hero_legion_commander &&
                                                                       x.IsValid &&
                                                                       x.IsVisible &&
                                                                       x.IsAlive &&
                                                                       x.HasAghanimsScepter());
            }

            var reincarnation = target.GetAbilityById(AbilityId.skeleton_king_reincarnation);

            return Owner.IsInvisible()
                || target.IsMagicImmune()
                || target.IsInvulnerable()
                || target.HasAnyModifiers("modifier_dazzle_shallow_grave", "modifier_necrolyte_reapers_scythe")
                || duelAghanimsScepter
                || (reincarnation != null && reincarnation.Cooldown == 0 && reincarnation.Level > 0);
        }
    }
}
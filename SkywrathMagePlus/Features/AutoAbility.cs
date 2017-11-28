using System;
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
    internal class AutoAbility
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private SkywrathMagePlus Main { get; }

        private Unit Owner { get; }

        private TaskHandler Handler { get; }

        public AutoAbility(Config config)
        {
            Config = config;
            Menu = config.Menu;
            Main = config.Main;
            Owner = config.Main.Context.Owner;

            Handler = UpdateManager.Run(ExecuteAsync, true, true);
        }

        public void Dispose()
        {
            Handler?.Cancel();
        }

        private async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                if (Game.IsPaused || !Owner.IsValid || !Owner.IsAlive || Owner.IsStunned())
                {
                    return;
                }

                // Eul
                if (Menu.EulBladeMailItem)
                {
                    var target = 
                        EntityManager<Hero>.Entities.Any(x =>
                                                         x.IsValid &&
                                                         x.IsVisible &&
                                                         x.IsAlive &&
                                                         !x.IsIllusion &&
                                                         x.IsEnemy(Owner) &&
                                                         x.HasModifier("modifier_skywrath_mystic_flare_aura_effect") &&
                                                         x.HasModifier("modifier_item_blade_mail_reflect"));

                    var eul = Main.Eul;
                    if (target && eul != null && eul.CanBeCasted)
                    {
                        eul.UseAbility(Owner);
                        await Await.Delay(eul.GetCastDelay(), token);
                    }
                }

                // ArcaneBolt
                if (Menu.AutoArcaneBoltKeyItem && !Menu.ComboKeyItem && !Menu.SpamArcaneBoltKeyItem)
                {
                    if (Menu.AutoArcaneBoltOwnerMinHealthItem <= ((float)Owner.Health / Owner.MaximumHealth) * 100)
                    {
                        var arcaneBolt = Main.ArcaneBolt;

                        var target =
                            EntityManager<Hero>.Entities.Where(x =>
                                                               x.IsValid &&
                                                               x.IsVisible &&
                                                               x.IsAlive &&
                                                               !x.IsIllusion &&
                                                               x.IsEnemy(Owner) &&
                                                               arcaneBolt.CanHit(x)).OrderBy(x => x.Health).FirstOrDefault();

                        if (target != null && Config.Extensions.Cancel(target) && !Owner.IsInvisible())
                        {
                            if (arcaneBolt.CanBeCasted)
                            {
                                arcaneBolt.UseAbility(target);

                                UpdateManager.BeginInvoke(() =>
                                {
                                    Config.MultiSleeper.Sleep(arcaneBolt.GetHitTime(target) - (arcaneBolt.GetCastDelay(target) + 350), $"arcanebolt_{ target.Name }");
                                },
                                arcaneBolt.GetCastDelay(target) + 50);

                                await Await.Delay(arcaneBolt.GetCastDelay(target), token);
                            }
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

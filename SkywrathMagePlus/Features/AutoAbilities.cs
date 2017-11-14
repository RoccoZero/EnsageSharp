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
    internal class AutoAbilities
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private SkywrathMagePlus Main { get; }

        private Unit Owner { get; }

        private TaskHandler Handler { get; }

        public AutoAbilities(Config config)
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
                    var target = EntityManager<Hero>.Entities.FirstOrDefault(x =>
                                                                             x.IsValid &&
                                                                             x.IsVisible &&
                                                                             x.IsAlive &&
                                                                             !x.IsIllusion &&
                                                                             x.IsEnemy(Owner) &&
                                                                             x.HasModifier("modifier_item_blade_mail_reflect") &&
                                                                             x.HasModifier("modifier_skywrath_mystic_flare_aura_effect"));

                    if (target != null && Main.Eul != null && Main.Eul.CanBeCasted)
                    {
                        Main.Eul.UseAbility(Owner);
                        await Await.Delay(Main.Eul.GetCastDelay(), token);
                    }
                }

                // ArcaneBolt
                if (Menu.AutoArcaneBoltKeyItem && !Menu.ComboKeyItem && !Menu.SpamArcaneBoltKeyItem)
                {
                    var ArcaneBolt = Main.ArcaneBolt;

                    var target =
                        EntityManager<Hero>.Entities.Where(x =>
                                                           x.IsVisible &&
                                                           x.IsAlive &&
                                                           !x.IsIllusion &&
                                                           x.IsValid &&
                                                           x.IsEnemy(Owner) &&
                                                           ArcaneBolt.CanHit(x)).OrderBy(x => x.Health).FirstOrDefault();

                    if (target != null && Config.Extensions.Cancel(target) && !Owner.IsInvisible())
                    {
                        if (ArcaneBolt.CanBeCasted)
                        {
                            ArcaneBolt.UseAbility(target);

                            UpdateManager.BeginInvoke(() =>
                            {
                                Config.MultiSleeper.Sleep(ArcaneBolt.GetHitTime(target) - (ArcaneBolt.GetCastDelay(target) + 350), $"arcanebolt_{ target.Name }");
                            },
                            ArcaneBolt.GetCastDelay(target) + 50);

                            await Await.Delay(ArcaneBolt.GetCastDelay(target), token);
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

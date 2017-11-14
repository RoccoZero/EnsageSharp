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
    internal class AutoDisable
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private Unit Owner { get; }

        private SkywrathMagePlus Main { get; }

        private TaskHandler Handler { get; }

        public AutoDisable(Config config)
        {
            Config = config;
            Menu = config.Menu;
            Main = config.Main;
            Owner = config.Main.Context.Owner;

            Handler = UpdateManager.Run(ExecuteAsync, true, false);

            if (config.Menu.AutoDisableItem)
            {
                Handler.RunAsync();
            }

            config.Menu.AutoDisableItem.PropertyChanged += AutoDisableChanged;
        }

        public void Dispose()
        {
            Menu.AutoDisableItem.PropertyChanged -= AutoDisableChanged;

            if (Menu.AutoDisableItem)
            {
                Handler?.Cancel();
            }
        }

        private void AutoDisableChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Menu.AutoDisableItem)
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
                if (Game.IsPaused || !Owner.IsValid || !Owner.IsAlive || Owner.IsStunned() || Owner.IsInvisible())
                {
                    return;
                }

                var target = EntityManager<Hero>.Entities.FirstOrDefault(x =>
                                                                         x.IsVisible &&
                                                                         x.IsAlive &&
                                                                         !x.IsIllusion &&
                                                                         x.IsValid &&
                                                                         x.IsEnemy(Owner) &&
                                                                         Config.Extensions.Disable(x));

                if (target == null)
                {
                    return;
                }

                // Hex
                var Hex = Main.Hex;
                if (Hex != null
                    && Menu.AutoDisableToggler.Value.IsEnabled(Hex.ToString())
                    && Hex.CanBeCasted
                    && Hex.CanHit(target))
                {
                    Hex.UseAbility(target);
                    await Await.Delay(Hex.GetCastDelay(target), token);
                    return;
                }

                // Orchid
                var Orchid = Main.Orchid;
                if (Orchid != null
                    && Menu.AutoDisableToggler.Value.IsEnabled(Orchid.ToString())
                    && Orchid.CanBeCasted
                    && Orchid.CanHit(target))
                {
                    Orchid.UseAbility(target);
                    await Await.Delay(Orchid.GetCastDelay(target), token);
                    return;
                }

                // Bloodthorn
                var Bloodthorn = Main.Bloodthorn;
                if (Bloodthorn != null
                    && Menu.AutoDisableToggler.Value.IsEnabled(Bloodthorn.ToString())
                    && Bloodthorn.CanBeCasted
                    && Bloodthorn.CanHit(target))
                {
                    Bloodthorn.UseAbility(target);
                    await Await.Delay(Bloodthorn.GetCastDelay(target), token);
                    return;
                }

                // AncientSeal
                var AncientSeal = Main.AncientSeal;
                if (Menu.AutoDisableToggler.Value.IsEnabled(AncientSeal.ToString())
                    && AncientSeal.CanBeCasted
                    && AncientSeal.CanHit(target))
                {
                    AncientSeal.UseAbility(target);
                    await Await.Delay(AncientSeal.GetCastDelay(target), token);
                    return;
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

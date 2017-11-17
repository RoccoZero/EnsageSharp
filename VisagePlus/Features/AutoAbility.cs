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
    internal class AutoAbility
    {
        private MenuManager Menu { get; }

        private VisagePlus Main { get; }

        private Extensions Extensions { get; }

        private Unit Owner { get; }

        private TaskHandler Handler { get; }

        public AutoAbility(Config config)
        {
            Menu = config.Menu;
            Main = config.Main;
            Extensions = config.Extensions;
            Owner = config.Main.Context.Owner;

            Handler = UpdateManager.Run(ExecuteAsync, true, false);

            if (config.Menu.AutoSoulAssumptionItem)
            {
                Handler.RunAsync();
            }

            config.Menu.AutoSoulAssumptionItem.PropertyChanged += AutoSoulAssumptionChanged;
        }

        public void Dispose()
        {
            Menu.AutoSoulAssumptionItem.PropertyChanged -= AutoSoulAssumptionChanged;

            Handler?.Cancel();
        }

        private void AutoSoulAssumptionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Menu.AutoSoulAssumptionItem)
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
                if (Game.IsPaused || !Owner.IsValid || !Owner.IsAlive || Owner.IsStunned() || Menu.ComboKeyItem || Owner.IsInvisible())
                {
                    return;
                }

                // SoulAssumption
                var soulAssumption = Main.SoulAssumption;
                if (soulAssumption.CanBeCasted && soulAssumption.MaxCharges)
                {
                    var target =
                        EntityManager<Hero>.Entities.Where(x =>
                                                           x.IsValid &&
                                                           !x.IsIllusion &&
                                                           x.IsAlive &&
                                                           x.IsVisible &&
                                                           x.IsEnemy(Owner) &&
                                                           soulAssumption.CanHit(x)).OrderBy(x => x.Health).FirstOrDefault();

                    if (target != null && Extensions.Cancel(target))
                    {
                        Main.SoulAssumption.UseAbility(target);
                        await Await.Delay(Main.SoulAssumption.GetCastDelay(target), token);
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
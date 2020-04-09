using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Ensage;
using Ensage.SDK.Extensions;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;

namespace PudgePlus.Features
{
    internal class AutoAbility
    {
        private MenuManager Menu { get; }

        private PudgePlus Main { get; }

        private UpdateMode UpdateMode { get; }

        private Helpers Extensions { get; }

        private Unit Owner { get; }

        private TaskHandler Handler { get; }

        private bool HasUserEnabledRot { get; set; }

        public AutoAbility(Config config)
        {
            Menu = config.Menu;
            Main = config.Main;
            UpdateMode = config.UpdateMode;
            Extensions = config.Helpers;
            Owner = config.Main.Context.Owner;

            Player.OnExecuteOrder += OnExecuteOrder;
            Handler = UpdateManager.Run(ExecuteAsync, true, true);
        }

        public void Dispose()
        {
            Handler?.Cancel();
            Player.OnExecuteOrder -= OnExecuteOrder;
        }

        private async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                if (Game.IsPaused || !Owner.IsValid || !Owner.IsAlive || Owner.IsStunned() || Owner.IsInvisible())
                {
                    return;
                }

                var rot = Main.Rot;
                if (!rot.CanBeCasted)
                {
                    return;
                }

                if (Menu.AbilityToggler.Value.IsEnabled(rot.ToString()))
                {
                    var target = UpdateMode.Target;
                    if (target != null && Menu.ComboKeyItem && Extensions.CancelMagicImmune(target))
                    {
                        // Rot Enabled
                        if (rot.CanHit(target) && !rot.Enabled)
                        {
                            rot.Enabled = true;
                            await Task.Delay(rot.GetCastDelay(), token);
                        }
                    }

                    // Rot Disable
                    if (rot.Enabled && !HasUserEnabledRot)
                    {
                        var enemyNear = EntityManager<Hero>.Entities.Any(x => x.IsVisible && x.IsAlive && Owner.IsEnemy(x) && rot.CanHit(x));
                        if (!enemyNear)
                        {
                            rot.Enabled = false;
                            await Task.Delay(rot.GetCastDelay(), token);
                        }
                    }
                }

                // Rot Deny
                if (Menu.AutoDenyItem)
                {
                    var denyHealth = rot.GetTickDamage(Owner);
                    var ownerHealth = (float)Owner.Health;
                    if (ownerHealth <= denyHealth && (Owner as Hero).RecentDamage >= ownerHealth)
                    {
                        if (!rot.Enabled)
                        {
                            rot.Enabled = true;
                            await Task.Delay(rot.GetCastDelay(), token);
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

        private void OnExecuteOrder(Player sender, ExecuteOrderEventArgs args)
        {
            var rot = Main.Rot;
            if (args.IsPlayerInput && args.OrderId == OrderId.ToggleAbility && args.Ability == rot.Ability)
            {
                HasUserEnabledRot = !rot.Enabled;
            }
        }
    }
}

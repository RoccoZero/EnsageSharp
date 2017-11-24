using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Ensage;
using Ensage.SDK.Extensions;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;

namespace PudgePlus.Features
{
    internal class LinkenBreaker
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private PudgePlus Main { get; }

        private Unit Owner { get; }

        public TaskHandler Handler { get; }

        public LinkenBreaker(Config config)
        {
            Config = config;
            Menu = config.Menu;
            Main = config.Main;
            Owner = config.Main.Context.Owner;

            Handler = UpdateManager.Run(ExecuteAsync, false, false);
        }

        private async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                var target = Config.UpdateMode.Target;
                if (target == null)
                {
                    return;
                }

                List<KeyValuePair<string, uint>> breakerChanger = new List<KeyValuePair<string, uint>>();

                if (target.IsLinkensProtected())
                {
                    breakerChanger = Menu.LinkenBreakerChanger.Value.Dictionary.Where(
                        x => Menu.LinkenBreakerToggler.Value.IsEnabled(x.Key)).OrderByDescending(x => x.Value).ToList();
                }
                else if (target.IsSpellShieldProtected())
                {
                    breakerChanger = Menu.AntiMageBreakerChanger.Value.Dictionary.Where(
                        x => Menu.AntiMageBreakerToggler.Value.IsEnabled(x.Key)).OrderByDescending(x => x.Value).ToList();
                }

                foreach (var order in breakerChanger)
                {
                    // Eul
                    var eul = Main.Eul;
                    if (eul != null
                        && eul.ToString() == order.Key
                        && eul.CanBeCasted)
                    {
                        if (eul.CanHit(target))
                        {
                            eul.UseAbility(target);
                            await Task.Delay(eul.GetCastDelay(target), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
                        }
                    }

                    // ForceStaff
                    var forceStaff = Main.ForceStaff;
                    if (forceStaff != null
                        && forceStaff.ToString() == order.Key
                        && forceStaff.CanBeCasted)
                    {
                        if (forceStaff.CanHit(target))
                        {
                            forceStaff.UseAbility(target);
                            await Task.Delay(forceStaff.GetCastDelay(target), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
                        }
                    }

                    // Orchid
                    var orchid = Main.Orchid;
                    if (orchid != null
                        && orchid.ToString() == order.Key
                        && orchid.CanBeCasted)
                    {
                        if (orchid.CanHit(target))
                        {
                            orchid.UseAbility(target);
                            await Task.Delay(orchid.GetCastDelay(target), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
                        }
                    }

                    // Bloodthorn
                    var bloodthorn = Main.Bloodthorn;
                    if (bloodthorn != null
                        && bloodthorn.ToString() == order.Key
                        && bloodthorn.CanBeCasted)
                    {
                        if (bloodthorn.CanHit(target))
                        {
                            bloodthorn.UseAbility(target);
                            await Task.Delay(bloodthorn.GetCastDelay(target), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
                        }
                    }

                    // Nullifier
                    var nullifier = Main.Nullifier;
                    if (nullifier != null
                        && nullifier.ToString() == order.Key
                        && nullifier.CanBeCasted)
                    {
                        if (nullifier.CanHit(target))
                        {
                            nullifier.UseAbility(target);
                            await Task.Delay(nullifier.GetCastDelay(target) + nullifier.GetHitTime(target), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
                        }
                    }

                    // Atos
                    var atos = Main.Atos;
                    if (atos != null
                        && atos.ToString() == order.Key
                        && atos.CanBeCasted)
                    {
                        if (atos.CanHit(target))
                        {
                            atos.UseAbility(target);
                            await Task.Delay(atos.GetCastDelay(target) + atos.GetHitTime(target), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
                        }
                    }

                    // Hex
                    var hex = Main.Hex;
                    if (hex != null
                        && hex.ToString() == order.Key
                        && hex.CanBeCasted)
                    {
                        if (hex.CanHit(target))
                        {
                            hex.UseAbility(target);
                            await Task.Delay(hex.GetCastDelay(target), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
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

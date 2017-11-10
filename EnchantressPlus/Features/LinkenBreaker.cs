using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Ensage;
using Ensage.Common.Threading;
using Ensage.SDK.Extensions;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;

namespace EnchantressPlus.Features
{
    internal class LinkenBreaker
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private EnchantressPlus Main { get; set; }

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

                List<KeyValuePair<string, uint>> BreakerChanger = new List<KeyValuePair<string, uint>>();

                if (target.IsLinkensProtected())
                {
                    BreakerChanger = Menu.LinkenBreakerChanger.Value.Dictionary.Where(
                        x => Menu.LinkenBreakerToggler.Value.IsEnabled(x.Key)).OrderByDescending(x => x.Value).ToList();
                }
                else if (target.IsSpellShieldProtected())
                {
                    BreakerChanger = Menu.AntiMageBreakerChanger.Value.Dictionary.Where(
                        x => Menu.AntiMageBreakerToggler.Value.IsEnabled(x.Key)).OrderByDescending(x => x.Value).ToList();
                }

                foreach (var order in BreakerChanger)
                {
                    // Eul
                    var Eul = Main.Eul;
                    if (Eul != null
                        && Eul.ToString() == order.Key
                        && Eul.CanBeCasted)
                    {
                        if (Eul.CanHit(target))
                        {
                            Eul.UseAbility(target);
                            await Await.Delay(Eul.GetCastDelay(target), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
                        }
                    }

                    // ForceStaff
                    var ForceStaff = Main.ForceStaff;
                    if (ForceStaff != null
                        && ForceStaff.ToString() == order.Key
                        && ForceStaff.CanBeCasted)
                    {
                        if (ForceStaff.CanHit(target))
                        {
                            ForceStaff.UseAbility(target);
                            await Await.Delay(ForceStaff.GetCastDelay(target), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
                        }
                    }

                    // Orchid
                    var Orchid = Main.Orchid;
                    if (Orchid != null
                        && Orchid.ToString() == order.Key
                        && Orchid.CanBeCasted)
                    {
                        if (Orchid.CanHit(target))
                        {
                            Orchid.UseAbility(target);
                            await Await.Delay(Orchid.GetCastDelay(target), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
                        }
                    }

                    // Bloodthorn
                    var Bloodthorn = Main.Bloodthorn;
                    if (Bloodthorn != null
                        && Bloodthorn.ToString() == order.Key
                        && Bloodthorn.CanBeCasted)
                    {
                        if (Bloodthorn.CanHit(target))
                        {
                            Bloodthorn.UseAbility(target);
                            await Await.Delay(Bloodthorn.GetCastDelay(target), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
                        }
                    }

                    // Nullifier
                    var Nullifier = Main.Nullifier;
                    if (Nullifier != null
                        && Nullifier.ToString() == order.Key
                        && Nullifier.CanBeCasted)
                    {
                        if (Nullifier.CanHit(target))
                        {
                            Nullifier.UseAbility(target);
                            await Await.Delay(Nullifier.GetCastDelay(target) + Nullifier.GetHitTime(target), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
                        }
                    }

                    // RodofAtos
                    var RodofAtos = Main.RodofAtos;
                    if (RodofAtos != null
                        && RodofAtos.ToString() == order.Key
                        && RodofAtos.CanBeCasted)
                    {
                        if (RodofAtos.CanHit(target))
                        {
                            RodofAtos.UseAbility(target);
                            await Await.Delay(RodofAtos.GetCastDelay(target) + (int)(Owner.Distance2D(target) / RodofAtos.Speed * 1000f), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
                        }
                    }

                    // HeavensHalberd
                    var HeavensHalberd = Main.HeavensHalberd;
                    if (HeavensHalberd != null
                        && HeavensHalberd.ToString() == order.Key
                        && HeavensHalberd.CanBeCasted)
                    {
                        if (HeavensHalberd.CanHit(target))
                        {
                            HeavensHalberd.UseAbility(target);
                            await Await.Delay(HeavensHalberd.GetCastDelay(target), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
                        }
                    }

                    // Enchant
                    var Enchant = Main.Enchant;
                    if (Enchant.ToString() == order.Key
                        && Enchant.CanBeCasted)
                    {
                        if (Enchant.CanHit(target))
                        {
                            Enchant.UseAbility(target);
                            await Await.Delay(Enchant.GetCastDelay(target), token);
                            return;
                        }
                        else if (Menu.UseOnlyFromRangeItem)
                        {
                            return;
                        }
                    }

                    // Hex
                    var Hex = Main.Hex;
                    if (Hex != null
                        && Hex.ToString() == order.Key
                        && Hex.CanBeCasted)
                    {
                        if (Hex.CanHit(target))
                        {
                            Hex.UseAbility(target);
                            await Await.Delay(Hex.GetCastDelay(target), token);
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

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Ensage;
using Ensage.Common.Menu;
using Ensage.Common.Threading;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.SDK.Extensions;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;

namespace VisagePlus.Features
{
    internal class FamiliarsControl
    {
        private MenuManager Menu { get; }

        private MultiSleeper MultiSleeper { get; }

        private VisagePlus Main { get; }

        private Extensions Extensions { get; }

        private Unit Owner { get; }

        private TaskHandler Handler { get; }

        public FamiliarsControl(Config config)
        {
            Main = config.Main;
            Menu = config.Menu;
            MultiSleeper = config.MultiSleeper;
            Extensions = config.Extensions;
            Owner = config.Main.Context.Owner;

            Handler = UpdateManager.Run(ExecuteAsync, true, true);

            if (config.Menu.FollowKeyItem)
            {
                config.Menu.FollowKeyItem.Item.SetValue(new KeyBind(Menu.FollowKeyItem.Value, KeyBindType.Toggle));
            }

            config.Menu.FollowKeyItem.PropertyChanged += FollowKeyChanged;
            config.Menu.EscapeKeyItem.PropertyChanged += EscapeKeyChanged;
        }

        public void Dispose()
        {
            Menu.EscapeKeyItem.PropertyChanged -= EscapeKeyChanged;
            Menu.FollowKeyItem.PropertyChanged -= FollowKeyChanged;

            Handler?.Cancel();
        }

        private void FollowKeyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!Menu.FollowKeyItem)
            {
                return;
            }

            Menu.LastHitItem.Item.SetValue(new KeyBind(Menu.LastHitItem.Value, KeyBindType.Toggle));
            Menu.FamiliarsLockItem.Item.SetValue(new KeyBind(Menu.FamiliarsLockItem.Value, KeyBindType.Toggle));
        }

        private void EscapeKeyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!Menu.EscapeKeyItem)
            {
                return;
            }

            Menu.LastHitItem.Item.SetValue(new KeyBind(Menu.LastHitItem.Value, KeyBindType.Toggle));
            Menu.FollowKeyItem.Item.SetValue(new KeyBind(Menu.FollowKeyItem.Value, KeyBindType.Toggle));
            Menu.FamiliarsLockItem.Item.SetValue(new KeyBind(Menu.FamiliarsLockItem.Value, KeyBindType.Toggle));
        }

        private async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                if (Game.IsPaused)
                {
                    return;
                }

                var familiars =
                    EntityManager<Unit>.Entities.Where(x =>
                                                       x.IsValid &&
                                                       x.IsAlive &&
                                                       x.IsControllable &&
                                                       x.IsAlly(Owner) &&
                                                       x.NetworkName == "CDOTA_Unit_VisageFamiliar").ToList();

                var Others =
                    EntityManager<Unit>.Entities.Where(x =>
                                                       x.IsValid &&
                                                       !x.IsIllusion &&
                                                       x.IsVisible &&
                                                       x.IsAlive &&
                                                       x.IsEnemy(Owner)).ToList();

                foreach (var familiar in familiars)
                {
                    // Auto Stone Form
                    var familiarsStoneForm = familiar.GetAbilityById(AbilityId.visage_summon_familiars_stone_form);
                    if (familiar.Health * 100 / familiar.MaximumHealth <= Menu.FamiliarsLowHPItem && Extensions.CanBeCasted(familiarsStoneForm, familiar))
                    {
                        familiarsStoneForm.UseAbility();
                        await Await.Delay(Extensions.GetDelay, token);
                    }

                    // Follow
                    if (Menu.FollowKeyItem)
                    {
                        Extensions.Follow(familiar, Owner);
                    }

                    // Courier
                    if (Menu.FamiliarsCourierItem)
                    {
                        var courier = Others.Where(x => x.NetworkName == "CDOTA_Unit_Courier").OrderBy(x => x.Distance2D(familiar)).FirstOrDefault();
                        if (courier != null && familiar.Distance2D(courier) <= 600 && !Menu.FollowKeyItem)
                        {
                            Extensions.Attack(familiar, courier);
                        }
                    }

                    // Escape
                    if (Menu.EscapeKeyItem && !Menu.ComboKeyItem && !Menu.FamiliarsLockItem && !Menu.LastHitItem && !Menu.FollowKeyItem)
                    {
                        var hero = Others.Where(x => x is Hero).OrderBy(x => x.Distance2D(Owner)).FirstOrDefault() as Hero;
                        if (hero == null || !Extensions.Cancel(hero) || Owner.Distance2D(hero) > 800)
                        {
                            Extensions.Follow(familiar, Owner);
                            continue;
                        }

                        if (Extensions.CanBeCasted(familiarsStoneForm, familiar)
                            && familiar.Distance2D(hero) <= 100
                            && !MultiSleeper.Sleeping("FamiliarsStoneForm"))
                        {
                            Extensions.UseAbility(familiarsStoneForm, familiar);
                            MultiSleeper.Sleep(familiarsStoneForm.GetAbilitySpecialData("stun_duration") * 1000 - 200, "FamiliarsStoneForm");
                            await Await.Delay(Extensions.GetDelay, token);
                        }
                        else if (Extensions.CanBeCasted(familiarsStoneForm, familiar))
                        {
                            Extensions.Move(familiar, hero.Position);
                        }
                        else
                        {
                            Extensions.Follow(familiar, Owner);
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
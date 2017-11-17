using System;
using System.ComponentModel;
using System.Linq;

using Ensage;
using Ensage.Common.Menu;
using Ensage.SDK.Extensions;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;

namespace VisagePlus.Features
{
    internal class FamiliarsLastHit
    {
        private MenuManager Menu { get; }

        private VisagePlus Main { get; }

        private Extensions Extensions { get; }

        private Unit Owner { get; }

        private IUpdateHandler Update { get; }

        public FamiliarsLastHit(Config config)
        {
            Menu = config.Menu;
            Main = config.Main;
            Extensions = config.Extensions;
            Owner = config.Main.Context.Owner;

            Update = UpdateManager.Subscribe(Execute);

            if (config.Menu.LastHitItem)
            {
                config.Menu.LastHitItem.Item.SetValue(new KeyBind(config.Menu.LastHitItem.Value, KeyBindType.Toggle));
            }

            config.Menu.LastHitItem.PropertyChanged += FamiliarsLastHitChanged;
        }

        public void Dispose()
        {
            Menu.LastHitItem.PropertyChanged -= FamiliarsLastHitChanged;

            UpdateManager.Unsubscribe(Execute);
        }

        private void FamiliarsLastHitChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Menu.LastHitItem)
            {
                Update.IsEnabled = true;

                Menu.FollowKeyItem.Item.SetValue(new KeyBind(Menu.FollowKeyItem.Value, KeyBindType.Toggle));
                Menu.FamiliarsLockItem.Item.SetValue(new KeyBind(Menu.FamiliarsLockItem.Value, KeyBindType.Toggle));
            }
            else
            {
                Update.IsEnabled = false;
            }
        }

        private void Execute()
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

                var attackingMe = ObjectManager.TrackingProjectiles.FirstOrDefault(x => x.Target.NetworkName == "CDOTA_Unit_VisageFamiliar");

                foreach (var Familiar in familiars)
                {
                    var enemyHero =
                        EntityManager<Hero>.Entities.FirstOrDefault(x =>
                                                                    x.IsAlive &&
                                                                    x.IsVisible &&
                                                                    x.IsEnemy(Owner) &&
                                                                    x.Distance2D(Familiar) <= x.AttackRange + 400);

                    var closestAllyTower =
                        EntityManager<Unit>.Entities.Where(x =>
                                                           x.IsAlive &&
                                                           x.IsAlly(Owner) &&
                                                           x.Distance2D(Familiar) >= 100 &&
                                                           x.NetworkName == "CDOTA_BaseNPC_Tower").OrderBy(
                                                                                x => x.Distance2D(Familiar)).FirstOrDefault();

                    if (enemyHero != null || (attackingMe != null && attackingMe.Target.Handle == Familiar.Handle))
                    {
                        if (closestAllyTower == null)
                        {
                            var closestAllyFountain =
                                EntityManager<Unit>.Entities.FirstOrDefault(x =>
                                                                            x.IsAlive &&
                                                                            x.IsAlly(Owner) &&
                                                                            x.NetworkName == "CDOTA_BaseNPC_Fort");

                            if (closestAllyFountain != null)
                            {
                                Extensions.Follow(Familiar, closestAllyFountain);
                            }
                        }
                        else
                        {
                            Extensions.Follow(Familiar, closestAllyTower);
                        }
                    }
                    else
                    {
                        var closestAllyCreep =
                            EntityManager<Unit>.Entities.Where(x =>
                                                               x.IsAlive &&
                                                               x.IsAlly(Owner) &&
                                                               Familiar.Distance2D(x) <= 3000 &&
                                                               (x.NetworkName == "CDOTA_BaseNPC_Creep_Lane" ||
                                                               x.NetworkName == "CDOTA_BaseNPC_Creep_Siege")).OrderBy(
                                                                                      x => x.Distance2D(Familiar)).FirstOrDefault();

                        var closestUnit =
                            EntityManager<Unit>.Entities.Where(x =>
                                                               x.IsAlive &&
                                                               x.IsVisible &&
                                                               (Menu.DenyItem && x.IsAlly(Owner) || x.IsEnemy(Owner)) &&
                                                               Familiar.Distance2D(x) <= 1000 &&
                                                               ((x.NetworkName == "CDOTA_BaseNPC_Tower" && x.Health <= 200) ||
                                                               x.NetworkName == "CDOTA_BaseNPC_Creep_Lane" ||
                                                               x.NetworkName == "CDOTA_BaseNPC_Creep" ||
                                                               x.NetworkName == "CDOTA_BaseNPC_Creep_Neutral" ||
                                                               x.NetworkName == "CDOTA_BaseNPC_Creep_Siege" ||
                                                               x.NetworkName == "CDOTA_BaseNPC_Additive" ||
                                                               x.NetworkName == "CDOTA_BaseNPC_Barracks" ||
                                                               x.NetworkName == "CDOTA_BaseNPC_Building" ||
                                                               x.NetworkName == "CDOTA_BaseNPC_Creature")).OrderBy(
                                                                                x => (float)x.Health / x.MaximumHealth).FirstOrDefault();

                        if (closestAllyCreep == null || closestAllyCreep.Distance2D(Familiar) >= 1000)
                        {
                            if (closestAllyTower == null)
                            {
                                var closestAllyFort =
                                    EntityManager<Unit>.Entities.FirstOrDefault(x =>
                                                                                x.IsAlive &&
                                                                                x.IsAlly(Owner) &&
                                                                                x.NetworkName == "CDOTA_BaseNPC_Fort");

                                Extensions.Follow(Familiar, closestAllyFort);
                            }
                            else
                            {
                                Extensions.Follow(Familiar, closestAllyTower);
                            }
                        }
                        else if (closestAllyCreep != null && closestUnit == null)
                        {
                            Extensions.Follow(Familiar, closestAllyCreep);
                        }
                        else if (closestAllyCreep != null && closestUnit != null)
                        {
                            var commonAttack = Menu.CommonAttackItem ? familiars.Count() : 1;
                            if (closestUnit.Health <= commonAttack * Familiar.GetAttackDamage(closestUnit))
                            {
                                Extensions.Attack(Familiar, closestUnit);
                            }
                            else
                            {
                                Extensions.Follow(Familiar, closestUnit);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Main.Log.Error(e);
            }
        }
    }
}
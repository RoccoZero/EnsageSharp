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
using Ensage.SDK.Orbwalker;
using Ensage.SDK.Service;

using SharpDX;

namespace SkywrathMagePlus
{
    internal class SpamMode
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private SkywrathMagePlus Main { get; }

        private IServiceContext Context { get; }

        private IOrbwalkerManager Orbwalker { get; }

        private Unit Owner { get; }

        private TaskHandler Handler { get; set; }

        private Unit Target { get; set; }

        public SpamMode(Config config)
        {
            Config = config;
            Menu = config.Menu;
            Main = config.Main;
            Context = config.Main.Context;
            Orbwalker = config.Main.Context.Orbwalker;
            Owner = config.Main.Context.Owner;

            config.Menu.SpamKeyItem.PropertyChanged += SpamKeyChanged;

            Handler = UpdateManager.Run(ExecuteAsync, true, false);
        }

        public void Dispose()
        {
            Menu.SpamKeyItem.PropertyChanged -= SpamKeyChanged;
        }

        private void SpamKeyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Menu.SpamKeyItem)
            {
                Handler.RunAsync();
                Target = null;
            }
            else
            {
                Handler?.Cancel();

                Context.Particle.Remove("SpamTarget");
            }
        }

        private async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                if (Target == null || !Target.IsValid || !Target.IsAlive)
                {
                    if (Menu.SpamUnitItem)
                    {
                        Target =
                            EntityManager<Unit>.Entities.Where(x =>
                                                               (x.NetworkName == "CDOTA_BaseNPC_Creep_Neutral" ||
                                                               x.NetworkName == "CDOTA_BaseNPC_Invoker_Forged_Spirit" ||
                                                               x.NetworkName == "CDOTA_BaseNPC_Warlock_Golem" ||
                                                               x.NetworkName == "CDOTA_BaseNPC_Creep" ||
                                                               x.NetworkName == "CDOTA_BaseNPC_Creep_Lane" ||
                                                               x.NetworkName == "CDOTA_BaseNPC_Creep_Siege" ||
                                                               x.NetworkName == "CDOTA_Unit_Hero_Beastmaster_Boar" ||
                                                               x.NetworkName == "CDOTA_Unit_SpiritBear" ||
                                                               x.NetworkName == "CDOTA_Unit_Broodmother_Spiderling") &&
                                                               x.IsVisible &&
                                                               x.IsAlive &&
                                                               !x.IsIllusion &&
                                                               x.IsSpawned &&
                                                               x.IsValid &&
                                                               x.IsEnemy(Owner) &&
                                                               x.Distance2D(Game.MousePosition) <= 100).OrderBy(x => x.Distance2D(Game.MousePosition)).FirstOrDefault();
                    }

                    if (Target == null)
                    {
                        Target = Config.UpdateMode.Target;
                    }
                }

                if (Target != null)
                {
                    Context.Particle.DrawTargetLine(
                        Owner,
                        "SpamTarget",
                        Target.Position,
                        Color.Green);

                    if (!Target.IsMagicImmune())
                    {
                        // ArcaneBolt
                        var ArcaneBolt = Main.ArcaneBolt;
                        if (ArcaneBolt.CanBeCasted && ArcaneBolt.CanHit(Target))
                        {
                            ArcaneBolt.UseAbility(Target);
                            await Await.Delay(ArcaneBolt.GetCastDelay(Target), token);
                        }
                    }

                    if (Target.IsInvulnerable() || Target.IsAttackImmune())
                    {
                        Orbwalker.Move(Game.MousePosition);
                    }
                    else
                    {
                        if (Menu.OrbwalkerItem.Value.SelectedValue.Contains("Default"))
                        {
                            Orbwalker.OrbwalkingPoint = Vector3.Zero;
                            Orbwalker.OrbwalkTo(Target);
                        }
                        else if (Menu.OrbwalkerItem.Value.SelectedValue.Contains("Distance"))
                        {
                            var ownerDis = Math.Min(Owner.Distance2D(Game.MousePosition), 230);
                            var ownerPos = Owner.Position.Extend(Game.MousePosition, ownerDis);
                            var pos = Target.Position.Extend(ownerPos, Menu.MinDisInOrbwalkItem);

                            Orbwalker.OrbwalkTo(Target);
                            Orbwalker.OrbwalkingPoint = pos;
                        }
                        else if (Menu.OrbwalkerItem.Value.SelectedValue.Contains("Free"))
                        {
                            if (Owner.Distance2D(Target) < Owner.AttackRange(Target) && Target.Distance2D(Game.MousePosition) < Owner.AttackRange(Target))
                            {
                                Orbwalker.OrbwalkingPoint = Vector3.Zero;
                                Orbwalker.OrbwalkTo(Target);
                            }
                            else
                            {
                                Orbwalker.Move(Game.MousePosition);
                            }
                        }
                    }
                }
                else
                {
                    Orbwalker.Move(Game.MousePosition);
                    Context.Particle.Remove("SpamTarget");
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

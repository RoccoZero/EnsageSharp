using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Ensage;
using Ensage.Common.Threading;
using Ensage.SDK.Extensions;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;

using SharpDX;

namespace ZeusPlus.Features
{
    internal class AbilityBreaker
    {
        private ZeusPlus Main { get; }

        private Unit Owner { get; }

        private MenuManager Menu { get; }

        private TaskHandler Handler { get; }

        private Vector3 Position { get; set; }

        private bool Particle { get; set; }

        private string ParticleName { get; set; }

        public AbilityBreaker(Config config)
        {
            Menu = config.Menu;
            Main = config.Main;
            Owner = config.Main.Context.Owner;

            Entity.OnParticleEffectAdded += OnParticleAdded;
            Handler = UpdateManager.Run(ExecuteAsync, true, true);
        }

        public void Dispose()
        {
            Handler?.Cancel();
            Entity.OnParticleEffectAdded -= OnParticleAdded;
        }

        private void OnParticleAdded(Entity sender, ParticleEffectAddedEventArgs args)
        {
            if (!args.Name.Contains("/teleport_start") && !args.Name.Contains("sandking_epicenter_tell"))
            {
                return;
            }

            if (Particle)
            {
                return;
            }
            
            UpdateManager.BeginInvoke(
                () =>
                {
                    Position = args.ParticleEffect.GetControlPoint(0);

                    var ignore = EntityManager<Hero>.Entities.Any(x => x.IsValid && x.IsAlly(Owner) && x.Distance2D(Position) < 100);

                    if (!ignore)
                    {
                        Particle = true;
                        ParticleName = args.Name;
                        UpdateManager.BeginInvoke(
                            () =>
                            {
                                Particle = false;
                            },
                            3000);
                    }
                },
                20);
        }

        private async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                if (Game.IsPaused || !Owner.IsValid || !Owner.IsAlive || Owner.IsStunned())
                {
                    return;
                }

                if (Menu.AbilityBreakerItem)
                {
                    var target = EntityManager<Hero>.Entities.Where(x =>
                                                                    x.IsValid &&
                                                                    x.IsVisible &&
                                                                    x.IsAlive &&
                                                                    x.IsEnemy(Owner) &&
                                                                    Disable(x)).OrderBy(x => x.Distance2D(Owner)).FirstOrDefault();

                    if (target != null)
                    {
                        // LightningBolt
                        var LightningBolt = Main.LightningBolt;
                        if (AbilityTogglers(target, LightningBolt.Ability)
                            && Owner.Distance2D(target) < LightningBolt.CastRange + 100
                            && LightningBolt.CanBeCasted)
                        {
                            LightningBolt.UseAbility(target);
                            await Await.Delay(LightningBolt.GetCastDelay(target), token);
                            return;
                        }

                        // Nimbus
                        var Nimbus = Main.Nimbus;
                        if (AbilityTogglers(target, Nimbus.Ability)
                            && Owner.Distance2D(target) > LightningBolt.CastRange + 100 && Owner.Distance2D(target) < Menu.TeleportNimbusRangeItem
                            && Nimbus.CanBeCasted)
                        {
                            Nimbus.UseAbility(target.Position);
                            await Await.Delay(Nimbus.GetCastDelay(target.Position), token);
                            return;
                        }
                    }
                }

                if (Particle)
                {
                    // LightningBolt
                    var LightningBolt = Main.LightningBolt;
                    if (ParticleTogglers(LightningBolt.Ability)
                        && Owner.Distance2D(Position) < LightningBolt.CastRange + 100
                        && LightningBolt.CanBeCasted)
                    {
                        LightningBolt.UseAbility(Position);
                        await Await.Delay(LightningBolt.GetCastDelay(Position), token);

                        Particle = false;
                        return;
                    }

                    // Nimbus
                    var Nimbus = Main.Nimbus;
                    if (ParticleTogglers(Nimbus.Ability)
                        && Owner.Distance2D(Position) > LightningBolt.CastRange + 100 && Owner.Distance2D(Position) < Menu.TeleportNimbusRangeItem
                        && Nimbus.CanBeCasted)
                    {
                        Nimbus.UseAbility(Position);
                        await Await.Delay(Nimbus.GetCastDelay(Position), token);

                        Particle = false;
                        return;
                    }
                }

                if (Menu.NimbusVisibleTeleportItem && Menu.TeleportBreakerItem)
                {
                    var visibleTarget = 
                        EntityManager<Hero>.Entities.Where(x =>
                                                           x.IsValid &&
                                                           x.IsVisible &&
                                                           x.IsAlive &&
                                                           x.IsEnemy(Owner) &&
                                                           x.HasModifier("modifier_teleporting")).OrderBy(x => x.Distance2D(Owner)).FirstOrDefault();

                    if (visibleTarget != null)
                    {
                        // Nimbus
                        var LightningBolt = Main.LightningBolt;
                        var Nimbus = Main.Nimbus;
                        if (AbilityTogglers(visibleTarget, Nimbus.Ability)
                           && Owner.Distance2D(visibleTarget) > LightningBolt.CastRange + 100 && Owner.Distance2D(visibleTarget) < Menu.TeleportNimbusRangeItem
                           && Nimbus.CanBeCasted)
                        {
                            Nimbus.UseAbility(visibleTarget.Position);
                            await Await.Delay(Nimbus.GetCastDelay(visibleTarget.Position), token);
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

        private bool Disable(Hero target)
        {
            var fiendsGrip = target.GetAbilityById(AbilityId.bane_fiends_grip);
            var deathWard = target.GetAbilityById(AbilityId.witch_doctor_death_ward);
            var freezingField = target.GetAbilityById(AbilityId.crystal_maiden_freezing_field);
            var blackHole = target.GetAbilityById(AbilityId.enigma_black_hole);
            var shackles = target.GetAbilityById(AbilityId.shadow_shaman_shackles);

            return (fiendsGrip != null && fiendsGrip.IsChanneling)
                || (deathWard != null && deathWard.IsChanneling)
                || (freezingField != null && freezingField.IsChanneling)
                || (blackHole != null && blackHole.IsChanneling)
                || (shackles != null && shackles.IsChanneling)
                || target.HasModifier("modifier_spirit_breaker_charge_of_darkness");
        }

        private bool AbilityTogglers(Hero target, Ability ability)
        {
            var abilityBreakerToggler = Menu.AbilityBreakerToggler.Value.IsEnabled(ability.Name);
            if (abilityBreakerToggler)
            {
                var abilityWeakBreakerToggler = Menu.AbilityWeakBreakerToggler.Value.IsEnabled(ability.Name);
                var shackles = target.GetAbilityById(AbilityId.shadow_shaman_shackles);

                if ((shackles != null && shackles.IsChanneling) || target.HasModifier("modifier_spirit_breaker_charge_of_darkness"))
                {
                    return abilityWeakBreakerToggler;
                }
            }

            return abilityBreakerToggler;
        }

        private bool ParticleTogglers(Ability ability)
        {
            if (ParticleName.Contains("sandking_epicenter_tell"))
            {
                return Menu.AbilityBreakerToggler.Value.IsEnabled(ability.Name) && Menu.AbilityBreakerItem;
            }

            var teleportBreakerToggler = Menu.TeleportBreakerToggler.Value.IsEnabled(ability.Name);
            if (ability.Id == AbilityId.zuus_cloud)
            {
                return teleportBreakerToggler && Menu.TeleportBreakerItem && !Menu.NimbusVisibleTeleportItem;
            }

            return teleportBreakerToggler && Menu.TeleportBreakerItem;
        }
    }
}

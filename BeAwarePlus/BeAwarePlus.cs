using System.ComponentModel.Composition;
using System.Linq;

using Ensage;
using Ensage.Common;
using Ensage.SDK.Helpers;
using Ensage.SDK.Extensions;
using Ensage.SDK.Renderer;
using Ensage.SDK.Service;
using Ensage.SDK.Service.Metadata;
using System;

namespace BeAwarePlus
{
    [ExportPlugin("BeAwarePlus", StartupMode.Auto, "YEEEEEEE", "3.0.2.0")]
    internal class BeAwarePlus : Plugin
    {
        private BeAwarePlusConfig Config { get; set; }

        public IRendererManager Render { get; }

        public IEntityContext<Unit> Context { get; }

        private Hero CheckEnemyBlink { get; set; }

        [ImportingConstructor]
        public BeAwarePlus(
            [Import] IRendererManager render, 
            [Import] IEntityContext<Unit> context)
        {
            Render = render;
            Context = context;
        }

        protected override void OnActivate()
        {
            Config = new BeAwarePlusConfig(this);

            Unit.OnModifierAdded += OnModifierEvent;
            Entity.OnParticleEffectAdded += OnParticleEvent;
            Entity.OnParticleEffectReleased += OnParticleEffectReleased;
            ObjectManager.OnAddEntity += OnEntityEvent;
        }

        protected override void OnDeactivate()
        {
            Unit.OnModifierAdded -= OnModifierEvent;
            Entity.OnParticleEffectAdded -= OnParticleEvent;
            Entity.OnParticleEffectReleased -= OnParticleEffectReleased;
            ObjectManager.OnAddEntity -= OnEntityEvent;

            Config?.Dispose();
        }

        private void OnModifierEvent(Unit sender, ModifierChangedEventArgs args)
        {
            if (Config.ModifierToTexture.ModifierAllyList.Any(x => args.Modifier.Name == x.Key))
            {
                if (sender.Team == Context.Owner.Team)
                {
                    Config.Modifiers.ModifierAlly(sender, args);
                }
            }
            else if (Config.ModifierToTexture.ModifierEnemyList.Any(x => args.Modifier.Name == x.Key))
            {
                if (sender.Team != Context.Owner.Team)
                {
                    Config.Modifiers.ModifierEnemy(sender, args);
                }
            }
            else if (Config.ModifierToTexture.ModifierOthersList.Any(x => args.Modifier.Name == x.Key))
            {
                if (sender.Team != Context.Owner.Team && Utils.SleepCheck(args.Modifier.TextureName))
                {
                    var Tick = Config.ModifierToTexture.ModifierOthersList.FirstOrDefault(
                        x => 
                        args.Modifier.Name == x.Key).Value;

                    Config.Modifiers.ModifierOthers(sender, args);
                    Utils.Sleep(Tick, args.Modifier.TextureName);
                }
            }
        }

        private void OnEntityEvent(EntityEventArgs args)
        {
            if (args.Entity.Name.Contains("npc_dota_base"))
            {
                var Unit = args.Entity as Unit;

                if (Config.EntityToTexture.EntityVisionTexture.Any(x => Unit.DayVision == x.Key))
                {
                    if (Unit.DayVision == 200 && EntityManager<Hero>.Entities.Any(x => 
                    x.HeroId == HeroId.npc_dota_hero_invoker)
                    || Unit.DayVision == 500 && !EntityManager<Hero>.Entities.Any(x => 
                    x.HeroId == HeroId.npc_dota_hero_mirana))
                    {
                        return;
                    }
                        
                    var AbilityTexturName = Config.EntityToTexture.EntityVisionTexture.FirstOrDefault(x => 
                    Unit.DayVision == x.Key).Value;

                    var Hero = EntityManager<Hero>.Entities.FirstOrDefault(x =>
                    x.Spellbook.Spells.Any(v =>
                    v.Name.Contains(AbilityTexturName)));

                   Config.Entities.Entity(
                        Hero, 
                        args, 
                        AbilityTexturName);
                }
            }

            if (args.Entity.Name.Contains("npc_dota_thinker")
                || args.Entity.Name.Contains("tusk_frozen_sigil")
                || args.Entity.Name.Contains("gyrocopter_homing_missile")
                || args.Entity.Name.Contains("juggernaut_healing_ward")
                || args.Entity.Name.Contains("slark_visual")
                || args.Entity.Name.Contains("templar_assassin_psionic_trap")
                || args.Entity.Name.Contains("pugna_nether_ward")
                || args.Entity.Name.Contains("shadow_shaman_ward")
                || args.Entity.Name.Contains("stormspirit_remnant"))
            {
                if (Config.EntityToTexture.EntityTexture.Any(x => args.Entity.Owner.Name.Contains(x.Key)))
                {
                    var AbilityTexturName = Config.EntityToTexture.EntityTexture.FirstOrDefault(x =>
                    args.Entity.Owner.Name.Contains(x.Key)).Value;

                    Config.Entities.Entity(
                        args.Entity.Owner as Hero, 
                        args, 
                        AbilityTexturName);
                }
            }
        }

        private void OnParticleEvent(Entity sender, ParticleEffectAddedEventArgs args)
        {
            var particleEffect = args.ParticleEffect;
            var name = particleEffect.Name;

            DelayAction.Add(-1, () => 
            {
                if (!particleEffect.IsValid)
                {
                    return;
                }

                // GetControlPoint 0
                if (Config.ParticleToTexture.ControlPoint_0.Any(x => name.Contains(x.Key)))
                {
                    var AbilityTexturName = Config.ParticleToTexture.ControlPoint_0.FirstOrDefault(x => name.Contains(x.Key)).Value;

                    Config.ParticleSpells.Spells(sender as Hero, name, AbilityTexturName, particleEffect.GetControlPoint(0));

                    return;
                }

                // GetControlPoint 0 Fix
                if (Config.ParticleToTexture.ControlPoint_0Fix.Any(x => name.Contains(x.Key)))
                {
                    var AbilityTexturName = Config.ParticleToTexture.ControlPoint_0Fix.FirstOrDefault(x => name.Contains(x.Key)).Value;

                    var Hero = EntityManager<Hero>.Entities.FirstOrDefault(
                        x =>
                        x.IsValid &&
                        !x.IsIllusion &&
                        x.Spellbook.Spells.Any(v =>
                        v.Name.Contains(AbilityTexturName)));

                    Config.ParticleSpells.Spells(
                        Hero,
                        name,
                        AbilityTexturName,
                        particleEffect.GetControlPoint(0));

                    return;
                }

                // GetControlPoint 1
                if (Config.ParticleToTexture.ControlPoint_1.Any(x => name.Contains(x.Key)))
                {
                    var AbilityTexturName = Config.
                            ParticleToTexture.ControlPoint_1.FirstOrDefault(
                                x => name.Contains(x.Key)).Value;

                    Config.ParticleSpells.Spells(
                        sender as Hero,
                        name,
                        AbilityTexturName,
                        particleEffect.GetControlPoint(1));

                    return;
                }

                // GetControlPoint 1 Fix
                if (Config.ParticleToTexture.ControlPoint_1Fix.Any(x => name.Contains(x.Key)))
                {
                    var AbilityTexturName = Config.
                             ParticleToTexture.ControlPoint_1Fix.FirstOrDefault(
                                 x => name.Contains(x.Key)).Value;

                    var Hero = EntityManager<Hero>.Entities.FirstOrDefault(
                        x =>
                        x.IsValid &&
                        !x.IsIllusion &&
                        x.Spellbook.Spells.Any(v =>
                        v.Name.Contains(AbilityTexturName)));

                    Config.ParticleSpells.Spells(
                        Hero,
                        name,
                        AbilityTexturName,
                        particleEffect.GetControlPoint(1));

                    return;
                }

                // GetControlPoint 2
                if (Config.ParticleToTexture.ControlPoint_2.Any(x => name.Contains(x.Key)))
                {
                    var AbilityTexturName = Config.
                            ParticleToTexture.ControlPoint_2.FirstOrDefault(
                                x => name.Contains(x.Key)).Value;

                    Config.ParticleSpells.Spells(
                        sender as Hero,
                        name,
                        AbilityTexturName,
                        particleEffect.GetControlPoint(2));

                    return;
                }

                // GetControlPoint 2 Fix
                if (Config.ParticleToTexture.ControlPoint_2Fix.Any(x => name.Contains(x.Key)))
                {
                    var AbilityTexturName = Config.
                             ParticleToTexture.ControlPoint_2Fix.FirstOrDefault(
                                 x => name.Contains(x.Key)).Value;

                    var Hero = EntityManager<Hero>.Entities.FirstOrDefault(
                        x =>
                        x.IsValid &&
                        !x.IsIllusion &&
                        x.Spellbook.Spells.Any(v =>
                        v.Name.Contains(AbilityTexturName)));

                    Config.ParticleSpells.Spells(
                        Hero,
                        name,
                        AbilityTexturName,
                        particleEffect.GetControlPoint(2));

                    return;
                }

                // GetControlPoint 5
                if (Config.ParticleToTexture.ControlPoint_5.Any(x => name.Contains(x.Key)))
                {
                    var AbilityTexturName = Config.
                            ParticleToTexture.ControlPoint_5.FirstOrDefault(
                                x => name.Contains(x.Key)).Value;

                    Config.ParticleSpells.Spells(
                        sender as Hero,
                        name,
                        AbilityTexturName,
                        particleEffect.GetControlPoint(5));

                    return;
                }

                // GetControlPoint 5 Fix
                if (Config.ParticleToTexture.ControlPoint_5Fix.Any(x => name.Contains(x.Key)))
                {
                    var AbilityTexturName = Config.
                            ParticleToTexture.ControlPoint_5Fix.FirstOrDefault(
                                x => name.Contains(x.Key)).Value;

                    var Hero = EntityManager<Hero>.Entities.FirstOrDefault(
                        x =>
                        x.IsValid &&
                        !x.IsIllusion &&
                        x.Spellbook.Spells.Any(v =>
                        v.Name.Contains(AbilityTexturName)));

                    Config.ParticleSpells.Spells(
                        Hero,
                        name,
                        AbilityTexturName,
                        particleEffect.GetControlPoint(5));

                    return;
                }

                // GetControlPoint 1 Plus
                if (Config.ParticleToTexture.ControlPoint_1Plus.Any(x => name.Contains(x.Key)))
                {
                    var AbilityTexturName = Config.
                             ParticleToTexture.ControlPoint_1Plus.FirstOrDefault(
                                 x => name.Contains(x.Key)).Value;
                    if (particleEffect.GetControlPoint(0).ToVector2()
                    == particleEffect.GetControlPoint(1).ToVector2())
                    {
                        return;
                    }

                    Config.ParticleSpells.Spells(
                        sender as Hero,
                        name,
                        AbilityTexturName,
                        particleEffect.GetControlPoint(1));

                    return;
                }

                //Town Portall Scrol Teleport Start 
                if (name.Contains("/teleport_start.vpcf") || name.Contains("/teleport_end.vpcf"))
                {
                    var ParticleColor = particleEffect.GetControlPoint(2);

                    if (ParticleColor.IsZero || particleEffect.GetControlPoint(0).IsZero)
                    {
                        return;
                    }

                    var Hero = ObjectManager.GetPlayerById((uint)Config.
                        Colors.Vector3ToID.FindIndex(
                        x =>
                        x == ParticleColor)).Hero;

                    Config.ParticleTeleport.Teleport(
                        Hero,
                        particleEffect.GetControlPoint(0),
                        "item_tpscroll",
                        ParticleColor,
                        name.Contains("/teleport_end.vpcf"));

                    return;
                }
            });

            UpdateManager.BeginInvoke(() =>
            {
                // Items
                if (Config.ParticleToTexture.Items.Any(x => name.Contains(x.Key)))
                {
                    var AbilityTexturName = Config.ParticleToTexture.Items.FirstOrDefault(x => name.Contains(x.Key)).Value;

                    if (AbilityTexturName.Contains("item_refresher"))
                    {
                        Config.ParticleItems.Items(
                            sender as Hero,
                            name,
                            AbilityTexturName,
                            particleEffect.GetControlPoint(0));
                    }
                    else if (AbilityTexturName.Contains("item_bfury"))
                    {
                        if (particleEffect.GetControlPoint(0).ToVector2()
                        == particleEffect.GetControlPoint(1).ToVector2())
                        {
                            return;
                        }

                        Config.ParticleItems.Items(
                            sender as Hero,
                            name,
                            AbilityTexturName,
                            particleEffect.GetControlPoint(1));
                    }
                    else if (AbilityTexturName.Contains("item_pipe") || AbilityTexturName.Contains("item_hood_of_defiance") || AbilityTexturName.Contains("item_crimson_guard"))
                    {
                        Config.ParticleItems.Items(
                            sender as Hero,
                            name,
                            AbilityTexturName,
                            particleEffect.GetControlPoint(1));
                    }

                    return;
                }

                // Items Semi Null CP0
                if (Config.ParticleToTexture.ItemsSemiNullCP0.Any(x => name.Contains(x.Key)))
                {
                    var AbilityTexturName = Config.
                            ParticleToTexture.ItemsSemiNullCP0.FirstOrDefault(
                                x => name.Contains(x.Key)).Value;

                    var Hero = EntityManager<Hero>.Entities.FirstOrDefault(
                        x =>
                        x.IsValid &&
                        !x.IsIllusion &&
                        x.Inventory.Items.Any(v =>
                        v.Name.Contains(AbilityTexturName)
                        && v?.Cooldown / v?.CooldownLength * 100 >= 99));

                    Config.ParticleItems.ItemsNull(
                        Hero,
                        name,
                        AbilityTexturName,
                        particleEffect.GetControlPoint(0));

                    return;
                }

                // Items Semi Null CP1
                if (Config.ParticleToTexture.ItemsSemiNullCP1.Any(x => name.Contains(x.Key)))
                {
                    var AbilityTexturName = Config.
                           ParticleToTexture.ItemsSemiNullCP1.FirstOrDefault(
                               x => name.Contains(x.Key)).Value;

                    var Hero = EntityManager<Hero>.Entities.FirstOrDefault(
                           x =>
                           x.IsValid &&
                           !x.IsIllusion &&
                           x.Inventory.Items.Any(v =>
                           v.Name.Contains(AbilityTexturName)
                           && v?.Cooldown / v?.CooldownLength * 100 >= 99));

                    Config.ParticleItems.ItemsNull(
                        Hero,
                        name,
                        AbilityTexturName,
                        particleEffect.GetControlPoint(1));

                    return;
                }

                if (Config.ParticleToTexture.ItemsNullCP1.Any(x => name.Contains(x.Key)))
                {
                    var AbilityTexturName = Config.
                            ParticleToTexture.ItemsNullCP1.FirstOrDefault(
                                x => name.Contains(x.Key)).Value;

                    Config.ParticleItems.ItemsNull(
                        null,
                        name,
                        AbilityTexturName,
                        particleEffect.GetControlPoint(1));
                }
            },
            1);
        }

        private void OnParticleEffectReleased(Entity sender, ParticleEffectReleasedEventArgs args)
        {
            var particleEffect = args.ParticleEffect;
            var name = particleEffect.Name;

            if (name.Contains("blink_dagger_end"))
            {
                CheckEnemyBlink = sender as Hero;
            }

            // GetControlPoint 0
            if (Config.ParticleToTexture.ControlPoint_0.Any(x => name.Contains(x.Key)))
            {
                var AbilityTexturName = Config.
                        ParticleToTexture.ControlPoint_0.FirstOrDefault(
                            x => name.Contains(x.Key)).Value;

                Config.ParticleSpells.Spells(
                    sender as Hero,
                    name,
                    AbilityTexturName,
                    particleEffect.GetControlPoint(0));

                return;
            }

            // GetControlPoint 0 Fix
            if (Config.ParticleToTexture.ControlPoint_0Fix.Any(x => name.Contains(x.Key)))
            {
                var AbilityTexturName = Config.
                        ParticleToTexture.ControlPoint_0Fix.FirstOrDefault(x => name.Contains(x.Key)).Value;

                var Hero = EntityManager<Hero>.Entities.FirstOrDefault(
                    x =>
                    x.IsValid &&
                    !x.IsIllusion &&
                    x.Spellbook.Spells.Any(v =>
                    v.Name.Contains(AbilityTexturName)));

                Config.ParticleSpells.Spells(
                    Hero,
                    name,
                    AbilityTexturName,
                    particleEffect.GetControlPoint(0));

                return;
            }

            // GetControlPoint 1
            if (Config.ParticleToTexture.ControlPoint_1.Any(x => name.Contains(x.Key)))
            {
                var AbilityTexturName = Config.
                        ParticleToTexture.ControlPoint_1.FirstOrDefault(x => name.Contains(x.Key)).Value;

                Config.ParticleSpells.Spells(
                    sender as Hero,
                    name,
                    AbilityTexturName,
                    particleEffect.GetControlPoint(1));

                return;
            }

            // GetControlPoint 1 Fix
            if (Config.ParticleToTexture.ControlPoint_1Fix.Any(x => name.Contains(x.Key)))
            {
                var AbilityTexturName = Config.
                        ParticleToTexture.ControlPoint_1Fix.FirstOrDefault(x => name.Contains(x.Key)).Value;

                var Hero = EntityManager<Hero>.Entities.FirstOrDefault(
                    x =>
                    x.IsValid &&
                    !x.IsIllusion &&
                    x.Spellbook.Spells.Any(v =>
                    v.Name.Contains(AbilityTexturName)));

                Config.ParticleSpells.Spells(
                    Hero,
                    name,
                    AbilityTexturName,
                    particleEffect.GetControlPoint(1));

                return;
            }

            // GetControlPoint 2
            if (Config.ParticleToTexture.ControlPoint_2.Any(x => name.Contains(x.Key)))
            {
                var AbilityTexturName = Config.
                        ParticleToTexture.ControlPoint_2.FirstOrDefault(x => name.Contains(x.Key)).Value;

                Config.ParticleSpells.Spells(
                    sender as Hero,
                    name,
                    AbilityTexturName,
                    particleEffect.GetControlPoint(2));

                return;
            }

            // GetControlPoint 2 Fix
            if (Config.ParticleToTexture.ControlPoint_2Fix.Any(x => name.Contains(x.Key)))
            {
                var AbilityTexturName = Config.
                        ParticleToTexture.ControlPoint_2Fix.FirstOrDefault(x => name.Contains(x.Key)).Value;

                var Hero = EntityManager<Hero>.Entities.FirstOrDefault(
                    x =>
                    x.IsValid &&
                    !x.IsIllusion &&
                    x.Spellbook.Spells.Any(v =>
                    v.Name.Contains(AbilityTexturName)));

                Config.ParticleSpells.Spells(
                    Hero,
                    name,
                    AbilityTexturName,
                    particleEffect.GetControlPoint(2));

                return;
            }

            // GetControlPoint 5
            if (Config.ParticleToTexture.ControlPoint_5.Any(
                x => name.Contains(x.Key)))
            {
                var AbilityTexturName = Config.
                        ParticleToTexture.ControlPoint_5.FirstOrDefault(x => name.Contains(x.Key)).Value;

                Config.ParticleSpells.Spells(
                    sender as Hero,
                    name,
                    AbilityTexturName,
                    particleEffect.GetControlPoint(5));

                return;
            }

            // GetControlPoint 5 Fix
            if (Config.ParticleToTexture.ControlPoint_5Fix.Any(x => name.Contains(x.Key)))
            {
                var AbilityTexturName = Config.
                        ParticleToTexture.ControlPoint_5Fix.FirstOrDefault(x => name.Contains(x.Key)).Value;

                var Hero = EntityManager<Hero>.Entities.FirstOrDefault(
                    x =>
                    x.IsValid &&
                    !x.IsIllusion &&
                    x.Spellbook.Spells.Any(v =>
                    v.Name.Contains(AbilityTexturName)));

                Config.ParticleSpells.Spells(
                    Hero,
                    name,
                    AbilityTexturName,
                    particleEffect.GetControlPoint(5));

                return;
            }

            // GetControlPoint 1 Plus
            if (Config.ParticleToTexture.ControlPoint_1Plus.Any(
                x => name.Contains(x.Key)))
            {
                var AbilityTexturName = Config.
                        ParticleToTexture.ControlPoint_1Plus.FirstOrDefault(x => name.Contains(x.Key)).Value;
                if (particleEffect.GetControlPoint(0).ToVector2()
                == particleEffect.GetControlPoint(1).ToVector2())
                {
                    return;
                }

                Config.ParticleSpells.Spells(
                    sender as Hero,
                    name,
                    AbilityTexturName,
                    particleEffect.GetControlPoint(1));

                return;
            }

            // Items
            if (Config.ParticleToTexture.Items.Any(
                x => name.Contains(x.Key)))
            {

                var AbilityTexturName = Config.ParticleToTexture.Items.FirstOrDefault(x => name.Contains(x.Key)).Value;

                if (AbilityTexturName.Contains("item_blink"))
                {
                    var position = particleEffect.GetControlPoint(0);
                    UpdateManager.BeginInvoke(() =>
                    {
                        Config.ParticleItems.Items(
                        CheckEnemyBlink,
                        name,
                        AbilityTexturName,
                        position);
                    });
                }

                else if (AbilityTexturName.Contains("item_refresher"))
                {
                    Config.ParticleItems.Items(
                        sender as Hero,
                        name,
                        AbilityTexturName,
                        particleEffect.GetControlPoint(0));
                }

                else if (AbilityTexturName.Contains("item_bfury"))
                {
                    if (particleEffect.GetControlPoint(0).ToVector2()
                    == particleEffect.GetControlPoint(1).ToVector2())
                    {
                        return;
                    }

                    Config.ParticleItems.Items(
                        sender as Hero,
                        name,
                        AbilityTexturName,
                        particleEffect.GetControlPoint(1));
                }
                else

                if (AbilityTexturName.Contains("item_pipe")
                || AbilityTexturName.Contains("item_hood_of_defiance")
                || AbilityTexturName.Contains("item_crimson_guard"))
                {
                    Config.ParticleItems.Items(
                        sender as Hero,
                        name,
                        AbilityTexturName,
                        particleEffect.GetControlPoint(1));
                }

                return;
            }

            // Items Semi Null CP0
            if (Config.ParticleToTexture.ItemsSemiNullCP0.Any(
                x => name.Contains(x.Key)))
            {
                var AbilityTexturName = Config.
                        ParticleToTexture.ItemsSemiNullCP0.FirstOrDefault(
                            x => name.Contains(x.Key)).Value;

                var Hero = EntityManager<Hero>.Entities.FirstOrDefault(
                    x =>
                    x.IsValid &&
                    !x.IsIllusion &&
                    x.Inventory.Items.Any(v =>
                    v.Name.Contains(AbilityTexturName)
                    && v?.Cooldown / v?.CooldownLength * 100 >= 99));

                Config.ParticleItems.ItemsNull(
                    Hero,
                    name,
                    AbilityTexturName,
                    particleEffect.GetControlPoint(0));

                return;
            }

            // Items Semi Null CP1
            if (Config.ParticleToTexture.ItemsSemiNullCP1.Any(
                x => name.Contains(x.Key)))
            {
                var AbilityTexturName = Config.
                        ParticleToTexture.ItemsSemiNullCP1.FirstOrDefault(
                            x => name.Contains(x.Key)).Value;

                var Hero = EntityManager<Hero>.Entities.FirstOrDefault(
                       x =>
                       x.IsValid &&
                       !x.IsIllusion &&
                       x.Inventory.Items.Any(v =>
                       v.Name.Contains(AbilityTexturName)
                       && v?.Cooldown / v?.CooldownLength * 100 >= 99));

                Config.ParticleItems.ItemsNull(
                    Hero,
                    name,
                    AbilityTexturName,
                    particleEffect.GetControlPoint(1));

                return;
            }

            // Item Smoke
            if (Config.ParticleToTexture.ItemsNullCP0.Any(x => name.Contains(x.Key)))
            {
                var AbilityTexturName = Config.
                        ParticleToTexture.ItemsNullCP0.FirstOrDefault(
                            x => name.Contains(x.Key)).Value;

                var IgnorAllySmoke = EntityManager<Hero>.Entities.Any(
                    x =>
                    x.Team == Context.Owner.Team &&
                    x.HasModifier("modifier_smoke_of_deceit"));

                if (!IgnorAllySmoke)
                {
                    Config.ParticleItems.ItemsNull(
                        null,
                        name,
                        AbilityTexturName,
                        particleEffect.GetControlPoint(0));
                }

                return;
            }

            if (Config.ParticleToTexture.ItemsNullCP1.Any( x => name.Contains(x.Key)))
            {
                var AbilityTexturName = Config.
                        ParticleToTexture.ItemsNullCP1.FirstOrDefault(
                            x => name.Contains(x.Key)).Value;

                Config.ParticleItems.ItemsNull(
                    null,
                    name,
                    AbilityTexturName,
                    particleEffect.GetControlPoint(1));

                return;
            }
        }
    }
}
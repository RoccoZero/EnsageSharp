using System.ComponentModel;
using System.Linq;

using Ensage;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Service;
using Ensage.SDK.Renderer.Particle;

using SharpDX;

namespace SkywrathMagePlus
{
    internal class UpdateMode
    {
        public MenuManager Menu { get; }

        private SkywrathMagePlus Main { get; }

        private IServiceContext Context { get; }

        private Unit Owner { get; }

        public Hero WShowTarget { get; set; }

        public Hero Target { get; set; }

        public UpdateMode(Config config)
        {
            Menu = config.Menu;
            Main = config.Main;
            Context = config.Main.Context;
            Owner = config.Main.Context.Owner;

            UpdateManager.Subscribe(OnUpdate, 25);
        }

        public void Dispose()
        {
            UpdateManager.Unsubscribe(OnUpdate);
        }

        private void OnUpdate()
        {
            var ArcaneBolt = Main.ArcaneBolt;
            if (Menu.ArcaneBoltRadiusItem && ArcaneBolt.Ability.Level > 0)
            {
                Context.Particle.DrawRange(
                    Context.Owner,
                    "ArcaneBolt",
                    ArcaneBolt.CastRange,
                    ArcaneBolt.IsReady ? Color.Aqua : Color.Gray);
            }
            else
            {
                Context.Particle.Remove("ArcaneBolt");
            }

            var ConcussiveShot = Main.ConcussiveShot;
            if (Menu.ConcussiveShotRadiusItem && ConcussiveShot.Ability.Level > 0)
            {
                Context.Particle.DrawRange(
                    Context.Owner,
                    "ConcussiveShot",
                    ConcussiveShot.Radius,
                    ConcussiveShot.IsReady ? Color.Aqua : Color.Gray);
            }
            else
            {
                Context.Particle.Remove("ConcussiveShot");
            }

            var AncientSeal = Main.AncientSeal;
            if (Menu.AncientSealRadiusItem && AncientSeal.Ability.Level > 0)
            {
                Context.Particle.DrawRange(
                    Context.Owner,
                    "AncientSeal",
                    AncientSeal.CastRange,
                    AncientSeal.IsReady ? Color.Aqua : Color.Gray);
            }
            else
            {
                Context.Particle.Remove("AncientSeal");
            }

            var MysticFlare = Main.MysticFlare;
            if (Menu.MysticFlareRadiusItem && MysticFlare.Ability.Level > 0)
            {
                Context.Particle.DrawRange(
                    Context.Owner,
                    "MysticFlare",
                    MysticFlare.CastRange,
                    MysticFlare.IsReady ? Color.Aqua : Color.Gray);
            }
            else
            {
                Context.Particle.Remove("MysticFlare");
            }

            var Blink = Main.Blink;
            if (Menu.BlinkRadiusItem && Blink != null)
            {
                var color = Color.Red;
                if (!Blink.IsReady)
                {
                    color = Color.Gray;
                }
                else if (Context.Owner.Distance2D(Game.MousePosition) > Menu.BlinkActivationItem)
                {
                    color = Color.Aqua;
                }

                Context.Particle.DrawRange(
                    Context.Owner,
                    "Blink",
                    Blink.CastRange,
                    color);
            }
            else
            {
                Context.Particle.Remove("Blink");
            }

            if (Menu.WDrawItem)
            {
                WShowTarget = EntityManager<Hero>.Entities.Where(x =>
                                                                 x.IsAlive &&
                                                                 x.IsVisible &&
                                                                 !x.IsIllusion &&
                                                                 x.IsValid &&
                                                                 x.IsEnemy(Owner) &&
                                                                 x.Distance2D(Owner) <= ConcussiveShot.Radius - 25).OrderBy(x => 
                                                                                                                            x.Distance2D(Context.Owner)).FirstOrDefault();

                if (WShowTarget != null && ConcussiveShot && ConcussiveShot.Ability.Cooldown <= 1)
                {
                    Context.Particle.AddOrUpdate(
                        WShowTarget,
                        "ConcussiveShotEffect",
                        "particles/units/heroes/hero_skywrath_mage/skywrath_mage_concussive_shot.vpcf",
                        ParticleAttachment.AbsOrigin,
                        RestartType.None,
                        0,
                        WShowTarget.Position + new Vector3(0, 200, WShowTarget.HealthBarOffset),
                        1,
                        WShowTarget.Position + new Vector3(0, 200, WShowTarget.HealthBarOffset),
                        2,
                        new Vector3(1000));
                }
                else
                {
                    Context.Particle.Remove("ConcussiveShotEffect");
                }
            }
            else
            {
                Context.Particle.Remove("ConcussiveShotEffect");
            }

            if (Menu.TargetItem.Value.SelectedValue.Contains("Lock") && Context.TargetSelector.IsActive
                && (!Menu.ComboKeyItem || Target == null || !Target.IsValid || !Target.IsAlive))
            {
                Target = Context.TargetSelector.Active.GetTargets().FirstOrDefault() as Hero;
            }
            else if (Menu.TargetItem.Value.SelectedValue.Contains("Default") && Context.TargetSelector.IsActive)
            {
                Target = Context.TargetSelector.Active.GetTargets().FirstOrDefault() as Hero;
            }

            if (Target != null && !Menu.SpamArcaneBoltKeyItem && (Menu.DrawOffTargetItem && !Menu.ComboKeyItem || Menu.DrawTargetItem && Menu.ComboKeyItem))
            {
                switch (Menu.TargetEffectTypeItem.Value.SelectedIndex)
                {
                    case 0:
                        Context.Particle.DrawTargetLine(
                            Context.Owner,
                            "SkywrathMagePlusTarget",
                            Target.Position,
                            Menu.ComboKeyItem
                            ? new Color(Menu.TargetRedItem, Menu.TargetGreenItem, Menu.TargetBlueItem)
                            : new Color(Menu.OffTargetRedItem, Menu.OffTargetGreenItem, Menu.OffTargetBlueItem));
                        break;

                    case 1:
                        Context.Particle.DrawDangerLine(
                            Context.Owner,
                            "SkywrathMagePlusTarget",
                            Target.Position,
                            Menu.ComboKeyItem
                            ? new Color(Menu.TargetRedItem, Menu.TargetGreenItem, Menu.TargetBlueItem)
                            : new Color(Menu.OffTargetRedItem, Menu.OffTargetGreenItem, Menu.OffTargetBlueItem));
                        break;

                    default:
                        Context.Particle.AddOrUpdate(
                            Target,
                            "SkywrathMagePlusTarget",
                            Menu.Effects[Menu.TargetEffectTypeItem.Value.SelectedIndex],
                            ParticleAttachment.AbsOriginFollow,
                            RestartType.NormalRestart,
                            1,
                            Menu.ComboKeyItem
                            ? new Color(Menu.TargetRedItem, Menu.TargetGreenItem, Menu.TargetBlueItem)
                            : new Color(Menu.OffTargetRedItem, Menu.OffTargetGreenItem, Menu.OffTargetBlueItem),
                            2,
                            new Vector3(255));
                        break;
                }
            }
            else
            {
                Context.Particle.Remove("SkywrathMagePlusTarget");
            }
        }
    }
}

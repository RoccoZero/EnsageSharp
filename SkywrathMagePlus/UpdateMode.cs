using System.Linq;

using Ensage;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Renderer.Particle;
using Ensage.SDK.TargetSelector;

using SharpDX;

namespace SkywrathMagePlus
{
    internal class UpdateMode
    {
        private MenuManager Menu { get; }

        private SkywrathMagePlus Main { get; }

        private ITargetSelectorManager TargetSelector { get; }

        private IParticleManager Particle { get; }

        private Unit Owner { get; }

        public Hero Target { get; set; }

        public UpdateMode(Config config)
        {
            Menu = config.Menu;
            Main = config.Main;
            TargetSelector = config.Main.Context.TargetSelector;
            Particle = config.Main.Context.Particle;
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
                Particle.DrawRange(
                    Owner,
                    "ArcaneBolt",
                    ArcaneBolt.CastRange,
                    ArcaneBolt.IsReady ? Color.Aqua : Color.Gray);
            }
            else
            {
                Particle.Remove("ArcaneBolt");
            }

            var ConcussiveShot = Main.ConcussiveShot;
            if (Menu.ConcussiveShotRadiusItem && ConcussiveShot.Ability.Level > 0)
            {
                Particle.DrawRange(
                    Owner,
                    "ConcussiveShot",
                    ConcussiveShot.Radius,
                    ConcussiveShot.IsReady ? Color.Aqua : Color.Gray);
            }
            else
            {
                Particle.Remove("ConcussiveShot");
            }

            var AncientSeal = Main.AncientSeal;
            if (Menu.AncientSealRadiusItem && AncientSeal.Ability.Level > 0)
            {
                Particle.DrawRange(
                    Owner,
                    "AncientSeal",
                    AncientSeal.CastRange,
                    AncientSeal.IsReady ? Color.Aqua : Color.Gray);
            }
            else
            {
                Particle.Remove("AncientSeal");
            }

            var MysticFlare = Main.MysticFlare;
            if (Menu.MysticFlareRadiusItem && MysticFlare.Ability.Level > 0)
            {
                Particle.DrawRange(
                    Owner,
                    "MysticFlare",
                    MysticFlare.CastRange,
                    MysticFlare.IsReady ? Color.Aqua : Color.Gray);
            }
            else
            {
                Particle.Remove("MysticFlare");
            }

            var Blink = Main.Blink;
            if (Menu.BlinkRadiusItem && Blink != null)
            {
                var color = Color.Red;
                if (!Blink.IsReady)
                {
                    color = Color.Gray;
                }
                else if (Owner.Distance2D(Game.MousePosition) > Menu.BlinkActivationItem)
                {
                    color = Color.Aqua;
                }

                Particle.DrawRange(
                    Owner,
                    "Blink",
                    Blink.CastRange,
                    color);
            }
            else
            {
                Particle.Remove("Blink");
            }

            var targetHit = ConcussiveShot.TargetHit;
            if (Menu.TargetHitConcussiveShotItem && targetHit != null && ConcussiveShot.Ability.Cooldown <= 1 && ConcussiveShot.Ability.Level > 0)
            {
                Particle.AddOrUpdate(
                    targetHit,
                    "TargetHitConcussiveShot",
                    "particles/units/heroes/hero_skywrath_mage/skywrath_mage_concussive_shot.vpcf",
                    ParticleAttachment.AbsOrigin,
                    RestartType.None,
                    0,
                    targetHit.Position + new Vector3(0, 200, targetHit.HealthBarOffset),
                    1,
                    targetHit.Position + new Vector3(0, 200, targetHit.HealthBarOffset),
                    2,
                    new Vector3(1000));
            }
            else
            {
                Particle.Remove("TargetHitConcussiveShot");
            }

            if (Menu.TargetItem.Value.SelectedValue.Contains("Lock") && TargetSelector.IsActive
                && (!Menu.ComboKeyItem || Target == null || !Target.IsValid || !Target.IsAlive))
            {
                Target = TargetSelector.Active.GetTargets().FirstOrDefault() as Hero;
            }
            else if (Menu.TargetItem.Value.SelectedValue.Contains("Default") && TargetSelector.IsActive)
            {
                Target = TargetSelector.Active.GetTargets().FirstOrDefault() as Hero;
            }

            if (Target != null && !Menu.SpamArcaneBoltKeyItem && (Menu.DrawOffTargetItem && !Menu.ComboKeyItem || Menu.DrawTargetItem && Menu.ComboKeyItem))
            {
                switch (Menu.TargetEffectTypeItem.Value.SelectedIndex)
                {
                    case 0:
                        Particle.DrawTargetLine(
                            Owner,
                            "SkywrathMagePlusTarget",
                            Target.Position,
                            Menu.ComboKeyItem
                            ? new Color(Menu.TargetRedItem, Menu.TargetGreenItem, Menu.TargetBlueItem)
                            : new Color(Menu.OffTargetRedItem, Menu.OffTargetGreenItem, Menu.OffTargetBlueItem));
                        break;

                    case 1:
                        Particle.DrawDangerLine(
                            Owner,
                            "SkywrathMagePlusTarget",
                            Target.Position,
                            Menu.ComboKeyItem
                            ? new Color(Menu.TargetRedItem, Menu.TargetGreenItem, Menu.TargetBlueItem)
                            : new Color(Menu.OffTargetRedItem, Menu.OffTargetGreenItem, Menu.OffTargetBlueItem));
                        break;

                    default:
                        Particle.AddOrUpdate(
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
                Particle.Remove("SkywrathMagePlusTarget");
            }
        }
    }
}

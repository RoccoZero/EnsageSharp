using System.Linq;

using Ensage;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Renderer.Particle;
using Ensage.SDK.TargetSelector;

using SharpDX;

namespace PudgePlus
{
    internal class UpdateMode
    {
        private MenuManager Menu { get; }

        private PudgePlus Main { get; }

        private ITargetSelectorManager TargetSelector { get; }

        private IParticleManager Particle { get; }

        private Unit Owner { get; }

        public Hero Target { get; private set; }

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
            var hook = Main.Hook;
            if (Menu.HookRadiusItem && hook.Ability.Level > 0)
            {
                Particle.DrawRange(
                    Owner,
                    "HookRadius",
                    hook.CastRange,
                    hook.IsReady ? Color.Aqua : Color.Gray);
            }
            else
            {
                Particle.Remove("HookRadius");
            }

            var rot = Main.Rot;
            if (Menu.RotRadiusItem && rot.Ability.Level > 0)
            {
                Particle.DrawRange(
                    Owner,
                    "RotRadius",
                    rot.Radius,
                    rot.IsReady ? Color.Aqua : Color.Gray);
            }
            else
            {
                Particle.Remove("RotRadius");
            }

            var dismember = Main.Dismember;
            if (Menu.DismemberRadiusItem && dismember.Ability.Level > 0)
            {
                Particle.DrawRange(
                    Owner,
                    "DismemberRadius",
                    dismember.CastRange,
                    dismember.IsReady ? Color.Aqua : Color.Gray);
            }
            else
            {
                Particle.Remove("DismemberRadius");
            }

            var blink = Main.Blink;
            if (Menu.BlinkRadiusItem && blink != null)
            {
                Particle.DrawRange(
                    Owner,
                    "BlinkRadius",
                    blink.CastRange,
                    blink.IsReady ? Color.Aqua : Color.Gray);
            }
            else
            {
                Particle.Remove("BlinkRadius");
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

            if (Target != null && Menu.DrawHookPredictionItem)
            {
                var hookOutput = hook.GetPredictionOutput(hook.GetPredictionInput(Target));
                var pos = Owner.NetworkPosition.Extend(hookOutput.CastPosition, Owner.Distance2D(Target, true));

                if (Owner.Distance2D(pos) <= hook.CastRange && hook.IsReady)
                {
                    Particle.AddOrUpdate(
                        Owner,
                        "HookLine",
                        "materials/ensage_ui/particles/line.vpcf",
                        ParticleAttachment.AbsOrigin,
                        RestartType.None,
                        1,
                        pos.Extend(Target.NetworkPosition, 80),
                        2,
                        Target.Position.Extend(pos, 80),
                        3,
                        new Vector3(Target.Distance2D(pos) > 80 ? 255 : 0, 50, 0),
                        4,
                        Color.Red);

                    Particle.AddOrUpdate(
                        Owner,
                        "HookCircle",
                        "materials/ensage_ui/particles/fat_ring.vpcf",
                        ParticleAttachment.AbsOrigin,
                        RestartType.None,
                        0,
                        pos,
                        1,
                        Color.Red,
                        2,
                        new Vector3(80, 255, 20));
                }
                else
                {
                    Particle.Remove("HookLine");
                    Particle.Remove("HookCircle");
                }
            }
            else
            {
                Particle.Remove("HookLine");
                Particle.Remove("HookCircle");
            }

            if (Target != null && (Menu.DrawOffTargetItem && !Menu.ComboKeyItem || Menu.DrawTargetItem && Menu.ComboKeyItem))
            {
                switch (Menu.TargetEffectTypeItem.Value.SelectedIndex)
                {
                    case 0:
                        Particle.DrawTargetLine(
                            Owner,
                            "PudgePlusTarget",
                            Target.Position,
                            Menu.ComboKeyItem
                            ? new Color(Menu.TargetRedItem, Menu.TargetGreenItem, Menu.TargetBlueItem)
                            : new Color(Menu.OffTargetRedItem, Menu.OffTargetGreenItem, Menu.OffTargetBlueItem));
                        break;

                    case 1:
                        Particle.DrawDangerLine(
                            Owner,
                            "PudgePlusTarget",
                            Target.Position,
                            Menu.ComboKeyItem
                            ? new Color(Menu.TargetRedItem, Menu.TargetGreenItem, Menu.TargetBlueItem)
                            : new Color(Menu.OffTargetRedItem, Menu.OffTargetGreenItem, Menu.OffTargetBlueItem));
                        break;

                    default:
                        Particle.AddOrUpdate(
                            Target,
                            "PudgePlusTarget",
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
                Particle.Remove("PudgePlusTarget");
            }
        }
    }
}

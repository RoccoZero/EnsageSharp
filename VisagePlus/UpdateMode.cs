using System.Linq;

using Ensage;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Orbwalker;
using Ensage.SDK.Renderer.Particle;
using Ensage.SDK.TargetSelector;

using SharpDX;

namespace VisagePlus
{
    internal class UpdateMode
    {
        private MenuManager Menu { get; }

        private VisagePlus Main { get; }

        private ITargetSelectorManager TargetSelector { get; }

        private IOrbwalkerManager Orbwalker { get; }

        private IParticleManager Particle { get; }

        private Unit Owner { get; }

        public Hero Target { get; set; }

        public Hero FamiliarTarget { get; set; }

        public UpdateMode(Config config)
        {
            Menu = config.Menu;
            Main = config.Main;
            TargetSelector = config.Main.Context.TargetSelector;
            Orbwalker = config.Main.Context.Orbwalker;
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
            if (Menu.EscapeKeyItem && !Menu.ComboKeyItem)
            {
                Orbwalker.Move(Game.MousePosition);
            }

            var graveChill = Main.GraveChill;
            if (Menu.GraveChillRadiusItem && graveChill.Ability.Level > 0)
            {
                Particle.DrawRange(
                    Owner,
                    "GraveChill",
                    graveChill.CastRange,
                    graveChill.IsReady ? Color.Aqua : Color.Gray);
            }
            else
            {
                Particle.Remove("GraveChill");
            }

            var soulAssumption = Main.SoulAssumption;
            if (Menu.SoulAssumptionRadiusItem && soulAssumption.Ability.Level > 0)
            {
                Particle.DrawRange(
                    Owner,
                    "SoulAssumption",
                    soulAssumption.CastRange,
                    soulAssumption.IsReady ? Color.Aqua : Color.Gray);
            }
            else
            {
                Particle.Remove("SoulAssumption");
            }

            var blink = Main.Blink;
            if (Menu.BlinkRadiusItem && blink != null)
            {
                var color = Color.Red;
                if (!blink.IsReady)
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
                    blink.CastRange,
                    color);
            }
            else
            {
                Particle.Remove("Blink");
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

            if (TargetSelector.IsActive 
                && (!Menu.FamiliarsLockItem || FamiliarTarget == null || !FamiliarTarget.IsValid || !FamiliarTarget.IsAlive))
            {
                FamiliarTarget = TargetSelector.Active.GetTargets().FirstOrDefault() as Hero;
            }

            if (Target != null && (Menu.DrawOffTargetItem && !Menu.ComboKeyItem || Menu.DrawTargetItem && Menu.ComboKeyItem))
            {
                switch (Menu.TargetEffectTypeItem.Value.SelectedIndex)
                {
                    case 0:
                        Particle.DrawTargetLine(
                            Owner,
                            "VisagePlusTarget",
                            Target.Position,
                            Menu.ComboKeyItem
                            ? new Color(Menu.TargetRedItem, Menu.TargetGreenItem, Menu.TargetBlueItem)
                            : new Color(Menu.OffTargetRedItem, Menu.OffTargetGreenItem, Menu.OffTargetBlueItem));
                        break;

                    case 1:
                        Particle.DrawDangerLine(
                            Owner,
                            "VisagePlusTarget",
                            Target.Position,
                            Menu.ComboKeyItem
                            ? new Color(Menu.TargetRedItem, Menu.TargetGreenItem, Menu.TargetBlueItem)
                            : new Color(Menu.OffTargetRedItem, Menu.OffTargetGreenItem, Menu.OffTargetBlueItem));
                        break;

                    default:
                        Particle.AddOrUpdate(
                            Target,
                            "VisagePlusTarget",
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
                Particle.Remove("VisagePlusTarget");
            }
        }
    }
}

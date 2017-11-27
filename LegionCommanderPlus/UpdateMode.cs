using System.Linq;

using Ensage;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Renderer.Particle;
using Ensage.SDK.TargetSelector;

using SharpDX;

namespace LegionCommanderPlus
{
    internal class UpdateMode
    {
        private MenuManager Menu { get; }

        private LegionCommanderPlus Main { get; }

        private ITargetSelectorManager TargetSelector { get; }

        private IParticleManager Particle { get; }

        private Unit Owner { get; }

        public Hero Target { get; private set; }

        public Unit[] OverwhelmingOddsUnits { get; private set; }

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
            var overwhelmingOdds = Main.OverwhelmingOdds;
            if (Menu.OverwhelmingOddsRadiusItem && overwhelmingOdds.Ability.Level > 0)
            {
                Particle.DrawRange(
                    Owner,
                    "OverwhelmingOddsRadius",
                    overwhelmingOdds.CastRange,
                    overwhelmingOdds.IsReady ? Color.Aqua : Color.Gray);
            }
            else
            {
                Particle.Remove("OverwhelmingOddsRadius");
            }

            var pressTheAttack = Main.PressTheAttack;
            if (Menu.PressTheAttackRadiusItem && pressTheAttack.Ability.Level > 0)
            {
                Particle.DrawRange(
                    Owner,
                    "PressTheAttackRadius",
                    pressTheAttack.CastRange,
                    pressTheAttack.IsReady ? Color.Aqua : Color.Gray);
            }
            else
            {
                Particle.Remove("PressTheAttackRadius");
            }

            var duel = Main.Duel;
            if (Menu.DuelRadiusItem && duel.Ability.Level > 0)
            {
                Particle.DrawRange(
                    Owner,
                    "DuelRadius",
                    duel.CastRange,
                    duel.IsReady ? Color.Aqua : Color.Gray);
            }
            else
            {
                Particle.Remove("DuelRadius");
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

            OverwhelmingOddsUnits = Units();

            if (Target != null && (Menu.DrawOffTargetItem && !Menu.ComboKeyItem || Menu.DrawTargetItem && Menu.ComboKeyItem))
            {
                switch (Menu.TargetEffectTypeItem.Value.SelectedIndex)
                {
                    case 0:
                        Particle.DrawTargetLine(
                            Owner,
                            "LegionCommanderPlusTarget",
                            Target.Position,
                            Menu.ComboKeyItem
                            ? new Color(Menu.TargetRedItem, Menu.TargetGreenItem, Menu.TargetBlueItem)
                            : new Color(Menu.OffTargetRedItem, Menu.OffTargetGreenItem, Menu.OffTargetBlueItem));
                        break;

                    case 1:
                        Particle.DrawDangerLine(
                            Owner,
                            "LegionCommanderPlusTarget",
                            Target.Position,
                            Menu.ComboKeyItem
                            ? new Color(Menu.TargetRedItem, Menu.TargetGreenItem, Menu.TargetBlueItem)
                            : new Color(Menu.OffTargetRedItem, Menu.OffTargetGreenItem, Menu.OffTargetBlueItem));
                        break;

                    default:
                        Particle.AddOrUpdate(
                            Target,
                            "LegionCommanderPlusTarget",
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
                Particle.Remove("LegionCommanderPlusTarget");
            }
        }

        private Unit[] Units()
        {
            return EntityManager<Unit>.Entities.Where(x =>
                                                      x.IsValid &&
                                                      x.IsVisible &&
                                                      x.IsAlive &&
                                                      x.IsSpawned &&
                                                      !(x is Building) &&
                                                      x.IsEnemy(Owner) &&
                                                      !x.IsInvulnerable()).ToArray();
        }
    }
}

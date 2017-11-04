using System.Linq;

using Ensage;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Renderer.Particle;
using Ensage.SDK.Service;

using SharpDX;

namespace NyxAssassinPlus
{
    internal class UpdateMode
    {
        private MenuManager Menu { get; }

        private NyxAssassinPlus Main { get; }

        private IServiceContext Context { get; }

        private Unit Owner { get; }

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
            var Impale = Main.Impale;
            if (Menu.ImpaleRadiusItem && Impale.Ability.Level > 0)
            {
                Context.Particle.DrawRange(
                    Owner,
                    "Impale",
                    Impale.CastRange,
                    Impale.IsReady ? Color.Aqua : Color.Gray);
            }
            else
            {
                Context.Particle.Remove("Impale");
            }

            var ManaBurn = Main.ManaBurn;
            if (Menu.ManaBurnRadiusItem && ManaBurn.Ability.Level > 0)
            {
                Context.Particle.DrawRange(
                    Owner,
                    "ManaBurn",
                    ManaBurn.CastRange,
                    ManaBurn.IsReady ? Color.Aqua : Color.Gray);
            }
            else
            {
                Context.Particle.Remove("ManaBurn");
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

                Context.Particle.DrawRange(
                    Owner,
                    "Blink",
                    Blink.CastRange,
                    color);
            }
            else
            {
                Context.Particle.Remove("Blink");
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

            if (Target != null && (Menu.DrawOffTargetItem && !Menu.ComboKeyItem || Menu.DrawTargetItem && Menu.ComboKeyItem))
            {
                switch (Menu.TargetEffectTypeItem.Value.SelectedIndex)
                {
                    case 0:
                        Context.Particle.DrawTargetLine(
                            Owner,
                            "NyxAssassinPlusTarget",
                            Target.Position,
                            Menu.ComboKeyItem
                            ? new Color(Menu.TargetRedItem, Menu.TargetGreenItem, Menu.TargetBlueItem)
                            : new Color(Menu.OffTargetRedItem, Menu.OffTargetGreenItem, Menu.OffTargetBlueItem));
                        break;

                    case 1:
                        Context.Particle.DrawDangerLine(
                            Owner,
                            "NyxAssassinPlusTarget",
                            Target.Position,
                            Menu.ComboKeyItem
                            ? new Color(Menu.TargetRedItem, Menu.TargetGreenItem, Menu.TargetBlueItem)
                            : new Color(Menu.OffTargetRedItem, Menu.OffTargetGreenItem, Menu.OffTargetBlueItem));
                        break;

                    default:
                        Context.Particle.AddOrUpdate(
                            Target,
                            "NyxAssassinPlusTarget",
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
                Context.Particle.Remove("NyxAssassinPlusTarget");
            }
        }
    }
}

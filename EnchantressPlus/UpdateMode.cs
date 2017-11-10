using System.Linq;

using Ensage;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Renderer.Particle;
using Ensage.SDK.Service;

using SharpDX;

namespace EnchantressPlus
{
    internal class UpdateMode
    {
        private MenuManager Menu { get; }

        private EnchantressPlus Main { get; }

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
            var Enchant = Main.Enchant;
            if (Menu.EnchantRadiusItem && Enchant.Ability.Level > 0)
            {
                Context.Particle.DrawRange(
                    Owner,
                    "Enchant",
                    Enchant.CastRange,
                    Enchant.IsReady ? Color.Aqua : Color.Gray);
            }
            else
            {
                Context.Particle.Remove("Enchant");
            }

            var Impetus = Main.Impetus;
            if (Menu.ImpetusRadiusItem && Impetus.Ability.Level > 0)
            {
                Context.Particle.DrawRange(
                    Owner,
                    "Impetus",
                    Impetus.CastRange,
                    Impetus.IsReady ? Color.Aqua : Color.Gray);
            }
            else
            {
                Context.Particle.Remove("Impetus");
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

            if (Menu.TargetItem.Value.SelectedValue.Contains("Lock") && Context.TargetSelector.IsActive
                && (!Menu.ComboKeyItem || Target == null || !Target.IsValid || !Target.IsAlive))
            {
                Target = Context.TargetSelector.Active.GetTargets().FirstOrDefault() as Hero;
            }
            else if (Menu.TargetItem.Value.SelectedValue.Contains("Default") && Context.TargetSelector.IsActive)
            {
                Target = Context.TargetSelector.Active.GetTargets().FirstOrDefault() as Hero;
            }

            var comboKey = Menu.ComboKeyItem;
            if (Target != null && (Menu.DrawOffTargetItem && !comboKey || Menu.DrawTargetItem && comboKey))
            {
                switch (Menu.TargetEffectTypeItem.Value.SelectedIndex)
                {
                    case 0:
                        Context.Particle.DrawTargetLine(
                            Owner,
                            "EnchantressPlusTarget",
                            Target.Position,
                            comboKey
                            ? new Color(Menu.TargetRedItem, Menu.TargetGreenItem, Menu.TargetBlueItem)
                            : new Color(Menu.OffTargetRedItem, Menu.OffTargetGreenItem, Menu.OffTargetBlueItem));
                        break;

                    case 1:
                        Context.Particle.DrawDangerLine(
                            Owner,
                            "EnchantressPlusTarget",
                            Target.Position,
                            comboKey
                            ? new Color(Menu.TargetRedItem, Menu.TargetGreenItem, Menu.TargetBlueItem)
                            : new Color(Menu.OffTargetRedItem, Menu.OffTargetGreenItem, Menu.OffTargetBlueItem));
                        break;

                    default:
                        Context.Particle.AddOrUpdate(
                            Target,
                            "EnchantressPlusTarget",
                            Menu.Effects[Menu.TargetEffectTypeItem.Value.SelectedIndex],
                            ParticleAttachment.AbsOriginFollow,
                            RestartType.NormalRestart,
                            1,
                            comboKey
                            ? new Color(Menu.TargetRedItem, Menu.TargetGreenItem, Menu.TargetBlueItem)
                            : new Color(Menu.OffTargetRedItem, Menu.OffTargetGreenItem, Menu.OffTargetBlueItem),
                            2,
                            new Vector3(255));
                        break;
                }
            }
            else
            {
                Context.Particle.Remove("EnchantressPlusTarget");
            }
        }
    }
}

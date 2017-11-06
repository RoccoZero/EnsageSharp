using System;
using System.Linq;

using Ensage;
using Ensage.SDK.Extensions;
using Ensage.SDK.Geometry;
using Ensage.SDK.Helpers;
using Ensage.SDK.Prediction;
using Ensage.SDK.Prediction.Collision;
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

        private IPredictionManager Prediction { get; }

        private Unit Owner { get; }

        public Hero Target { get; set; }

        public Vector3 BlinkPos { get; set; }

        public Vector3 ImpalePos { get; set; }

        public UpdateMode(Config config)
        {
            Menu = config.Menu;
            Main = config.Main;
            Context = config.Main.Context;
            Prediction = config.Main.Context.Prediction;
            Owner = config.Main.Context.Owner;

            UpdateManager.Subscribe(OnUpdate);
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
                else if (Impale.CanBeCasted)
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
                && ((!Menu.ComboKeyItem && !Menu.MaxStunKeyItem) || Target == null || !Target.IsValid || !Target.IsAlive))
            {
                Target = Context.TargetSelector.Active.GetTargets().FirstOrDefault() as Hero;
            }
            else if (Menu.TargetItem.Value.SelectedValue.Contains("Default") && Context.TargetSelector.IsActive)
            {
                Target = Context.TargetSelector.Active.GetTargets().FirstOrDefault() as Hero;
            }

            if (Target != null)
            {
                var otherTarget = 
                    EntityManager<Hero>.Entities.Where(x =>
                                                       x.IsValid &&
                                                       x.IsVisible &&
                                                       x.IsAlive &&
                                                       !x.IsIllusion &&
                                                       x != Target &&
                                                       x.Distance2D(Target) < Impale.Range - 100 &&
                                                       x.IsEnemy(Owner)).OrderBy(x => Target.Distance2D(x)).FirstOrDefault();

                BlinkPos = Vector3.Zero;
                ImpalePos = Vector3.Zero;

                if (otherTarget != null)
                {
                    var input = new PredictionInput
                    {
                        Owner = Owner,
                        AreaOfEffect = false,
                        CollisionTypes = CollisionTypes.None,
                        Delay = Impale.CastPoint + Impale.ActivationDelay,
                        Speed = float.MaxValue,
                        Range = 0,
                        Radius = 0,
                        PredictionSkillshotType = PredictionSkillshotType.SkillshotCircle
                    };

                    var predictionInput = input.WithTarget(Target);

                    var OutputPrediction = Prediction.GetPrediction(predictionInput);
                    var blinkTargetPos = OutputPrediction.CastPosition.Extend(OutputPrediction.CastPosition, Owner.Distance2D(OutputPrediction.CastPosition));

                    var Output = Impale.GetPredictionOutput(Impale.GetPredictionInput(otherTarget));
                    BlinkPos = Output.CastPosition.Extend(blinkTargetPos, Output.CastPosition.Distance2D(Target.Position) + 100);

                    var impaleTargetPos = Output.CastPosition.Extend(Output.CastPosition, Owner.Distance2D(Output.CastPosition));
                    ImpalePos = Target.Position.Extend(impaleTargetPos, Math.Min(Target.Distance2D(Output.CastPosition), Impale.Range - 100));

                    if (Menu.ImpaleLineItem)
                    {
                        Context.Particle.AddOrUpdate(
                        Owner,
                        "Line",
                        "materials/ensage_ui/particles/line.vpcf",
                        ParticleAttachment.AbsOrigin,
                        RestartType.None,
                        1,
                        BlinkPos,
                        2,
                        ImpalePos,
                        3,
                        new Vector3(255, 30, 0),
                        4,
                        Color.Red);

                        CircleParticle("BlinkCast", BlinkPos, true);
                        CircleParticle("ImpaleCast", ImpalePos, true);
                    }
                    else
                    {
                        Remover();
                    }
                }
                else
                {
                    Remover();
                }
            }
            else
            {
                Remover();
            }

            var comboKey = Menu.ComboKeyItem || Menu.MaxStunKeyItem;
            if (Target != null && (Menu.DrawOffTargetItem && !comboKey || Menu.DrawTargetItem && comboKey))
            {
                switch (Menu.TargetEffectTypeItem.Value.SelectedIndex)
                {
                    case 0:
                        Context.Particle.DrawTargetLine(
                            Owner,
                            "NyxAssassinPlusTarget",
                            Target.Position,
                            comboKey
                            ? new Color(Menu.TargetRedItem, Menu.TargetGreenItem, Menu.TargetBlueItem)
                            : new Color(Menu.OffTargetRedItem, Menu.OffTargetGreenItem, Menu.OffTargetBlueItem));
                        break;

                    case 1:
                        Context.Particle.DrawDangerLine(
                            Owner,
                            "NyxAssassinPlusTarget",
                            Target.Position,
                            comboKey
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
                Context.Particle.Remove("NyxAssassinPlusTarget");
            }

            if (Owner.HasAghanimsScepter())
            {
                Menu.MaxStunKeyItem.Item.SetFontColor(new Color(163, 185, 176, 255));
            }
            else
            {
                Menu.MaxStunKeyItem.Item.SetFontColor(Color.Black);
            }
        }

        private void CircleParticle(string cast, Vector3 pos, bool remove)
        {
            if (remove)
            {
                Context.Particle.AddOrUpdate(
                Owner,
                cast,
                "particles/ui_mouseactions/drag_selected_ring.vpcf",
                ParticleAttachment.AbsOrigin,
                RestartType.None,
                0,
                pos,
                1,
                Color.Red,
                2,
                new Vector3(50, 255, 255));
            }
            else
            {
                Context.Particle.Remove(cast);
            }
        }

        private void Remover()
        {
            Context.Particle.Remove("Line");

            CircleParticle("BlinkCast", Vector3.Zero, false);
            CircleParticle("ImpaleCast", Vector3.Zero, false);
        }
    }
}

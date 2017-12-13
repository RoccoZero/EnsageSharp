using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using Ensage;
using Ensage.Common;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Renderer;
using Ensage.SDK.Service;
using Ensage.SDK.Service.Metadata;

using SharpDX;

using Color = System.Drawing.Color;

namespace JungleScanPlus
{
    [ExportPlugin("JungleScanPlus", StartupMode.Auto, "YEEEEEEE", "2.0.1.1")]
    public class JungleScanPlus : Plugin
    {
        private Config Config { get; set; }

        private Lazy<IRendererManager> RendererManager { get; }

        private List<Position> Pos { get; } = new List<Position>();

        private Vector2 ExtraPos { get; set; }

        private int ExtraSize { get; set; }

        [ImportingConstructor]
        public JungleScanPlus([Import] Lazy<IRendererManager> rendererManager)
        {
            RendererManager = rendererManager;
        }

        protected override void OnActivate()
        {
            Config = new Config();

            if (Drawing.RenderMode == RenderMode.Dx9)
            {
                ExtraPos = new Vector2(10, 28);
                ExtraSize = 45;
            }
            else if (Drawing.RenderMode == RenderMode.Dx11)
            {
                ExtraPos = new Vector2(10, 20);
                ExtraSize = 25;
            }
            
            Entity.OnParticleEffectAdded += OnParticleEvent;
            RendererManager.Value.Draw += OnDraw;
        }

        protected override void OnDeactivate()
        {
            RendererManager.Value.Draw -= OnDraw;
            Entity.OnParticleEffectAdded -= OnParticleEvent;
            
            Config?.Dispose();
        }

        private void OnParticleEvent(Entity sender, ParticleEffectAddedEventArgs args)
        {
            if (args.ParticleEffect.Owner == null || !args.ParticleEffect.IsValid || args.ParticleEffect.Owner.IsVisible)
            {
                return;
            }

            if (!args.Name.Contains("generic_hit_blood") || !sender.Name.Contains("npc_dota_neutral_") && !sender.Name.Contains("npc_dota_roshan"))
            {
                return;
            }

            UpdateManager.BeginInvoke(
                    () =>
                    {
                        Pos.RemoveAll(x => x.GetPos.Distance(args.ParticleEffect.GetControlPoint(0)) < 500);
                        Pos.Add(new Position(args.ParticleEffect.GetControlPoint(0)));

                        var rawGameTime = Game.RawGameTime;
                        UpdateManager.BeginInvoke(
                            () =>
                            {
                                Pos.RemoveAll(x => x.GetGameTime == rawGameTime);
                            },
                            Config.TimerItem.Value * 1000);
                    },
                    20);
        }

        private void OnDraw(object sender, EventArgs e)
        {
            var color = Color.FromArgb(Config.RedItem, Config.GreenItem, Config.BlueItem);
            foreach (var position in Pos.ToList())
            {
                var pos = position.GetPos;
                if (pos.IsZero)
                {
                    continue;
                }

                RendererManager.Value.DrawText(
                    pos.WorldToMinimap() - ExtraPos,
                    "○",
                    color,
                    ExtraSize,
                    "Arial Black");

                if (Config.DrawWorldItem)
                {
                    var screenPos = Drawing.WorldToScreen(pos);
                    if (!screenPos.IsZero)
                    {
                        RendererManager.Value.DrawText(
                        screenPos,
                        "Enemy",
                        color,
                        35,
                        "Arial Black");
                    }
                }
            }
        }

        internal class Position
        {
            public Vector3 GetPos { get; }

            public float GetGameTime { get; }

            public Position(Vector3 pos)
            {
                GetPos = pos;
                GetGameTime = Game.RawGameTime;
            }
        }
    }
}

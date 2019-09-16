using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Ensage;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Renderer.Particle;
using Ensage.SDK.Service;
using Ensage.SDK.Service.Metadata;

using SharpDX;

namespace VisibleByEnemyPlus
{
    [ExportPlugin("VisibleByEnemyPlus", StartupMode.Auto, "YEEEEEEE", "3.0.1.0")]
    public class VisibleByEnemyPlus : Plugin
    {
        private Unit Owner { get; }

        private IParticleManager ParticleManager { get; }

        private Config Config { get; set; }

        private bool AddEffectType { get; set; }

        private int Red => Config.RedItem;

        private int Green => Config.GreenItem;

        private int Blue => Config.BlueItem;

        private int Alpha => Config.AlphaItem;

        [ImportingConstructor]
        public VisibleByEnemyPlus([Import] IServiceContext context, [Import] IParticleManager particlemanager)
        {
            Owner = context.Owner;
            ParticleManager = particlemanager;
        }

        protected override void OnActivate()
        {
            Config = new Config();

            Config.EffectTypeItem.PropertyChanged += ItemChanged;

            Config.RedItem.PropertyChanged += ItemChanged;
            Config.GreenItem.PropertyChanged += ItemChanged;
            Config.BlueItem.PropertyChanged += ItemChanged;
            Config.AlphaItem.PropertyChanged += ItemChanged;

            UpdateManager.Subscribe(LoopEntities, 250);
        }

        protected override void OnDeactivate()
        {
            UpdateManager.Unsubscribe(LoopEntities);

            Config.EffectTypeItem.PropertyChanged -= ItemChanged;

            Config.RedItem.PropertyChanged -= ItemChanged;
            Config.GreenItem.PropertyChanged -= ItemChanged;
            Config.BlueItem.PropertyChanged -= ItemChanged;
            Config.AlphaItem.PropertyChanged -= ItemChanged;

            Config?.Dispose();
            ParticleManager.Dispose();
        }

        private void ItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Config.EffectTypeItem.Value.SelectedIndex == 0)
            {
                Config.RedItem.Item.SetFontColor(Color.Black);
                Config.GreenItem.Item.SetFontColor(Color.Black);
                Config.BlueItem.Item.SetFontColor(Color.Black);
                Config.AlphaItem.Item.SetFontColor(Color.Black);
            }
            else
            {
                Config.RedItem.Item.SetFontColor(new Color(Red, 0, 0, 255));
                Config.GreenItem.Item.SetFontColor(new Color(0, Green, 0, 255));
                Config.BlueItem.Item.SetFontColor(new Color(0, 0, Blue, 255));
                Config.AlphaItem.Item.SetFontColor(new Color(185, 176, 163, Alpha));
            }

            Owner.Stop();

            HandleEffect(Owner, true);
            AddEffectType = false;
        }

        private bool IsMine(Entity sender)
        {
            return sender.NetworkName == "CDOTA_NPC_TechiesMines";
        }

        private bool IsShrine(Entity sender)
        {
            return sender.NetworkName == "CDOTA_BaseNPC_Healer";
        }

        private bool IsNeutral(Unit sender)
        {
            return sender.NetworkName == "CDOTA_BaseNPC_Creep_Neutral";
        }

        private bool IsUnit(Unit sender)
        {
            return !(sender is Hero) && !(sender is Building)
                   && (sender.NetworkName != "CDOTA_BaseNPC_Creep_Lane"
                   && sender.NetworkName != "CDOTA_BaseNPC_Creep_Siege"
                   || sender.IsControllable)
                   && sender.NetworkName != "CDOTA_NPC_TechiesMines"
                   && sender.NetworkName != "CDOTA_NPC_Observer_Ward"
                   && sender.NetworkName != "CDOTA_NPC_Observer_Ward_TrueSight"
                   && sender.NetworkName != "CDOTA_BaseNPC_Healer";
        }

        private bool IsWard(Entity sender)
        {
            return sender.NetworkName == "CDOTA_NPC_Observer_Ward"
                || sender.NetworkName == "CDOTA_NPC_Observer_Ward_TrueSight";
        }

        private void LoopEntities()
        {
            if (Config.AlliedHeroesItem)
            {
                foreach (var hero in EntityManager<Hero>.Entities.Where(x => x.Team == Owner.Team))
                {
                    HandleEffect(hero, hero.IsVisibleToEnemies);
                }
            }

            if (Config.BuildingsItem)
            {
                foreach (var building in EntityManager<Building>.Entities.Where(x => x.Team == Owner.Team))
                {
                    HandleEffect(building, building.IsVisibleToEnemies);
                }
            }

            if (Config.NeutralsItem)
            {
                foreach (var neutral in EntityManager<Unit>.Entities.Where(IsNeutral))
                {
                    HandleEffect(neutral, neutral.IsVisibleToEnemies);
                }
            }

            var Units = EntityManager<Unit>.Entities.Where(x => x.Team == Owner.Team).ToList();

            if (Config.WardsItem )
            {
                foreach (var ward in Units.Where(IsWard))
                {
                    HandleEffect(ward, ward.IsVisibleToEnemies);
                }
            }

            if (Config.MinesItem)
            {
                foreach (var mine in Units.Where(IsMine))
                {
                    HandleEffect(mine, mine.IsVisibleToEnemies);
                }
            }

            if (Config.ShrinesItem)
            {
                foreach (var shrine in Units.Where(IsShrine))
                {
                    HandleEffect(shrine, shrine.IsVisibleToEnemies);
                }
            }

            if (Config.UnitsItem)
            {
                foreach (var unit in Units.Where(IsUnit))
                {
                    HandleEffect(unit, unit.IsVisibleToEnemies);
                }
            }
        }

        private void HandleEffect(Unit unit, bool visible)
        {
            if (!AddEffectType && Owner.Animation.Name != "idle")
            {
                AddEffectType = true;
            }

            if (!unit.IsValid)
            {
                return;
            }

            if (visible && unit.IsAlive && unit.Position.IsOnScreen())
            {
                ParticleManager.AddOrUpdate(
                    unit,
                    $"unit_{unit.Handle}",
                    Config.Effects[Config.EffectTypeItem.Value.SelectedIndex],
                    ParticleAttachment.AbsOriginFollow,
                    RestartType.NormalRestart,
                    1,
                    new Vector3(Red, Green, Blue),
                    2,
                    new Vector3(Alpha));
            }
            else if (AddEffectType)
            {
                ParticleManager.Remove($"unit_{ unit.Handle }");
            }
        }
    }
}
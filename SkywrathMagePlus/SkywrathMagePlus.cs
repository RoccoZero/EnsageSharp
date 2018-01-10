using System.ComponentModel.Composition;
using System.Reflection;

using Ensage;
using Ensage.SDK.Abilities;
using Ensage.SDK.Abilities.Aggregation;
using Ensage.SDK.Abilities.Items;
using Ensage.SDK.Abilities.npc_dota_hero_skywrath_mage;
using Ensage.SDK.Inventory.Metadata;
using Ensage.SDK.Service;
using Ensage.SDK.Service.Metadata;

using log4net;

using PlaySharp.Toolkit.Logging;

namespace SkywrathMagePlus
{
    [ExportPlugin(
        name: "SkywrathMagePlus",
        author: "YEEEEEEE", 
        version: "2.2.0.3",
        priority: 10000,
        units: HeroId.npc_dota_hero_skywrath_mage)]
    internal class SkywrathMagePlus : Plugin
    {
        public IServiceContext Context { get; }

        public ILog Log { get; } = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private Config Config { get; set; }

        [ImportingConstructor]
        public SkywrathMagePlus([Import] IServiceContext context)
        {
            Context = context;
        }

        protected override void OnActivate()
        {
            Config = new Config(this);
        }

        protected override void OnDeactivate()
        {
            Config?.Dispose();
        }
    }
}

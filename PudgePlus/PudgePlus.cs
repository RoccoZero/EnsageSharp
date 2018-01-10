using System.ComponentModel.Composition;
using System.Reflection;

using Ensage;
using Ensage.SDK.Abilities;
using Ensage.SDK.Abilities.Aggregation;
using Ensage.SDK.Abilities.Items;
using Ensage.SDK.Abilities.npc_dota_hero_pudge;
using Ensage.SDK.Inventory.Metadata;
using Ensage.SDK.Service;
using Ensage.SDK.Service.Metadata;

using log4net;

using PlaySharp.Toolkit.Logging;

namespace PudgePlus
{
    [ExportPlugin(
        name: "PudgePlus",
        mode: StartupMode.Auto,
        author: "YEEEEEEE", 
        version: "1.0.1.1",
        priority: 10000,
        units: HeroId.npc_dota_hero_pudge)]
    internal class PudgePlus : Plugin
    {
        public IServiceContext Context { get; }

        private AbilityFactory AbilityFactory { get; }

        public ILog Log { get; } = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private Config Config { get; set; }

        [ImportingConstructor]
        public PudgePlus([Import] IServiceContext context)
        {
            Context = context;
            AbilityFactory = context.AbilityFactory;
        }

        public pudge_meat_hook Hook { get; private set; }

        public pudge_rot Rot { get; private set; }

        public pudge_dismember Dismember { get; private set; }

        public Dagon Dagon
        {
            get
            {
                return Dagon1 ?? Dagon2 ?? Dagon3 ?? Dagon4 ?? (Dagon)Dagon5;
            }
        }

        [ItemBinding]
        public item_sheepstick Hex { get; private set; }

        [ItemBinding]
        public item_orchid Orchid { get; private set; }

        [ItemBinding]
        public item_bloodthorn Bloodthorn { get; private set; }

        [ItemBinding]
        public item_rod_of_atos Atos { get; private set; }

        [ItemBinding]
        public item_veil_of_discord Veil { get; private set; }

        [ItemBinding]
        public item_ethereal_blade Ethereal { get; private set; }

        [ItemBinding]
        public item_dagon Dagon1 { get; private set; }

        [ItemBinding]
        public item_dagon_2 Dagon2 { get; private set; }

        [ItemBinding]
        public item_dagon_3 Dagon3 { get; private set; }

        [ItemBinding]
        public item_dagon_4 Dagon4 { get; private set; }

        [ItemBinding]
        public item_dagon_5 Dagon5 { get; private set; }

        [ItemBinding]
        public item_force_staff ForceStaff { get; private set; }

        [ItemBinding]
        public item_cyclone Eul { get; private set; }

        [ItemBinding]
        public item_blink Blink { get; private set; }

        [ItemBinding]
        public item_shivas_guard Shivas { get; private set; }

        [ItemBinding]
        public item_nullifier Nullifier { get; private set; }

        [ItemBinding]
        public item_urn_of_shadows Urn { get; private set; }

        [ItemBinding]
        public item_spirit_vessel Vessel { get; private set; }

        protected override void OnActivate()
        {
            Hook = AbilityFactory.GetAbility<pudge_meat_hook>();
            Rot = AbilityFactory.GetAbility<pudge_rot>();
            Dismember = AbilityFactory.GetAbility<pudge_dismember>();

            Context.Inventory.Attach(this);

            Config = new Config(this);
        }

        protected override void OnDeactivate()
        {
            Config?.Dispose();

            Context.Inventory.Detach(this);
        }
    }
}

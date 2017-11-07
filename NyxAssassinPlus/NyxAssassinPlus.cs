using System.ComponentModel.Composition;
using System.Reflection;

using Ensage;
using Ensage.SDK.Abilities;
using Ensage.SDK.Abilities.Aggregation;
using Ensage.SDK.Abilities.Items;
using Ensage.SDK.Abilities.npc_dota_hero_nyx_assassin;
using Ensage.SDK.Inventory.Metadata;
using Ensage.SDK.Service;
using Ensage.SDK.Service.Metadata;

using log4net;

using PlaySharp.Toolkit.Logging;

namespace NyxAssassinPlus
{
    [ExportPlugin(
        name: "NyxAssassinPlus",
        mode: StartupMode.Auto,
        author: "YEEEEEEE", 
        version: "1.1.0.1",
        units: HeroId.npc_dota_hero_nyx_assassin)]
    internal class NyxAssassinPlus : Plugin
    {
        private Config Config { get; set; }

        public IServiceContext Context { get; }

        private AbilityFactory AbilityFactory { get; }

        public ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [ImportingConstructor]
        public NyxAssassinPlus([Import] IServiceContext context)
        {
            Context = context;
            AbilityFactory = context.AbilityFactory;
        }

        public nyx_assassin_impale Impale { get; set; }

        public nyx_assassin_mana_burn ManaBurn { get; set; }

        public nyx_assassin_spiked_carapace SpikedCarapace { get; set; }

        public nyx_assassin_burrow Burrow { get; set; }

        public nyx_assassin_unburrow UnBurrow { get; set; }

        public nyx_assassin_vendetta Vendetta { get; set; }

        public Dagon Dagon
        {
            get
            {
                return Dagon1 ?? Dagon2 ?? Dagon3 ?? Dagon4 ?? (Dagon)Dagon5;
            }
        }

        [ItemBinding]
        public item_sheepstick Hex { get; set; }

        [ItemBinding]
        public item_orchid Orchid { get; set; }

        [ItemBinding]
        public item_bloodthorn Bloodthorn { get; set; }

        [ItemBinding]
        public item_rod_of_atos RodofAtos { get; set; }

        [ItemBinding]
        public item_veil_of_discord Veil { get; set; }

        [ItemBinding]
        public item_ethereal_blade Ethereal { get; set; }

        [ItemBinding]
        public item_dagon Dagon1 { get; set; }

        [ItemBinding]
        public item_dagon_2 Dagon2 { get; set; }

        [ItemBinding]
        public item_dagon_3 Dagon3 { get; set; }

        [ItemBinding]
        public item_dagon_4 Dagon4 { get; set; }

        [ItemBinding]
        public item_dagon_5 Dagon5 { get; set; }

        [ItemBinding]
        public item_force_staff ForceStaff { get; set; }

        [ItemBinding]
        public item_cyclone Eul { get; set; }

        [ItemBinding]
        public item_shivas_guard Shivas { get; set; }

        [ItemBinding]
        public item_blink Blink { get; set; }

        [ItemBinding]
        public item_nullifier Nullifier { get; set; }

        [ItemBinding]
        public item_urn_of_shadows UrnOfShadows { get; set; }

        [ItemBinding]
        public item_spirit_vessel SpiritVessel { get; set; }

        protected override void OnActivate()
        {
            Impale = AbilityFactory.GetAbility<nyx_assassin_impale>();
            ManaBurn = AbilityFactory.GetAbility<nyx_assassin_mana_burn>();
            SpikedCarapace = AbilityFactory.GetAbility<nyx_assassin_spiked_carapace>();
            Burrow = AbilityFactory.GetAbility<nyx_assassin_burrow>();
            UnBurrow = AbilityFactory.GetAbility<nyx_assassin_unburrow>();
            Vendetta = AbilityFactory.GetAbility<nyx_assassin_vendetta>();

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

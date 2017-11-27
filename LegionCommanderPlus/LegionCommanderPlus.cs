using System.ComponentModel.Composition;
using System.Reflection;

using Ensage;
using Ensage.SDK.Abilities;
using Ensage.SDK.Abilities.Aggregation;
using Ensage.SDK.Abilities.Items;
using Ensage.SDK.Abilities.npc_dota_hero_legion_commander;
using Ensage.SDK.Inventory.Metadata;
using Ensage.SDK.Service;
using Ensage.SDK.Service.Metadata;

using log4net;

using PlaySharp.Toolkit.Logging;

namespace LegionCommanderPlus
{
    [ExportPlugin(
        name: "LegionCommanderPlus",
        mode: StartupMode.Auto,
        author: "YEEEEEEE", 
        version: "1.0.0.0",
        units: HeroId.npc_dota_hero_legion_commander)]
    internal class LegionCommanderPlus : Plugin
    {
        public IServiceContext Context { get; }

        private AbilityFactory AbilityFactory { get; }

        public ILog Log { get; } = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private Config Config { get; set; }

        [ImportingConstructor]
        public LegionCommanderPlus([Import] IServiceContext context)
        {
            Context = context;
            AbilityFactory = context.AbilityFactory;
        }

        public legion_commander_overwhelming_odds OverwhelmingOdds { get; private set; }

        public legion_commander_press_the_attack PressTheAttack { get; private set; }

        public legion_commander_duel Duel { get; private set; }

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

        [ItemBinding]
        public item_blade_mail BladeMail { get; private set; }

        [ItemBinding]
        public item_abyssal_blade AbyssalBlade { get; private set; }

        [ItemBinding]
        public item_mjollnir Mjollnir { get; private set; }

        [ItemBinding]
        public item_black_king_bar BlackKingBar { get; private set; }

        [ItemBinding]
        public item_armlet Armlet { get; private set; }

        [ItemBinding]
        public item_satanic Satanic { get; private set; }

        protected override void OnActivate()
        {
            OverwhelmingOdds = AbilityFactory.GetAbility<legion_commander_overwhelming_odds>();
            PressTheAttack = AbilityFactory.GetAbility<legion_commander_press_the_attack>();
            Duel = AbilityFactory.GetAbility<legion_commander_duel>();

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

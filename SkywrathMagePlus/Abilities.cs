using System;
using System.Collections.Specialized;
using Ensage.SDK.Abilities;
using Ensage.SDK.Abilities.Aggregation;
using Ensage.SDK.Abilities.Items;
using Ensage.SDK.Abilities.npc_dota_hero_skywrath_mage;
using Ensage.SDK.Helpers;
using Ensage.SDK.Inventory;
using Ensage.SDK.Inventory.Metadata;

namespace SkywrathMagePlus
{
    internal class Abilities
    {
        private AbilityFactory AbilityFactory { get; }

        private IInventoryManager Inventory { get; }

        public Abilities(Config config)
        {
            AbilityFactory = config.Main.Context.AbilityFactory;
            Inventory = config.Main.Context.Inventory;

            ArcaneBolt = AbilityFactory.GetAbility<skywrath_mage_arcane_bolt>();
            ConcussiveShot = AbilityFactory.GetAbility<skywrath_mage_concussive_shot>();
            AncientSeal = AbilityFactory.GetAbility<skywrath_mage_ancient_seal>();
            MysticFlare = AbilityFactory.GetAbility<skywrath_mage_mystic_flare>();

            UpdateManager.BeginInvoke(() =>
            {
                Inventory.Attach(this);
            },
            3000);
        }

        public skywrath_mage_arcane_bolt ArcaneBolt { get; set; }

        public skywrath_mage_concussive_shot ConcussiveShot { get; set; }

        public skywrath_mage_ancient_seal AncientSeal { get; set; }

        public skywrath_mage_mystic_flare MysticFlare { get; set; }

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
        public item_blink Blink { get; set; }

        [ItemBinding]
        public item_shivas_guard Shivas { get; set; }

        [ItemBinding]
        public item_nullifier Nullifier { get; set; }

        [ItemBinding]
        public item_urn_of_shadows UrnOfShadows { get; set; }

        [ItemBinding]
        public item_spirit_vessel SpiritVessel { get; set; }

        public void Dispose()
        {
            Inventory.Detach(this);
        }
    }
}

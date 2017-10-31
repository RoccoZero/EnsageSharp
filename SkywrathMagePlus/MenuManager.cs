using System.Collections.Generic;
using System.ComponentModel;

using Ensage.Common.Menu;
using Ensage.SDK.Menu;

using SharpDX;

namespace SkywrathMagePlus
{
    internal class MenuManager
    {
        private MenuFactory Factory { get; }

        public MenuItem<AbilityToggler> AbilitiesToggler { get; }

        public MenuItem<AbilityToggler> ItemsToggler { get; }

        public MenuItem<Slider> BlinkActivationItem { get; }

        public MenuItem<Slider> BlinkDistanceEnemyItem { get; }

        public MenuItem<bool> AutoComboItem { get; }

        public MenuItem<AbilityToggler> AutoAbilitiesToggler { get; }

        public MenuItem<AbilityToggler> AutoItemsToggler { get; }

        public MenuItem<bool> AutoKillStealItem { get; }

        public MenuItem<AbilityToggler> AutoKillStealToggler { get; }

        public MenuItem<bool> AutoDisableItem { get; }

        public MenuItem<AbilityToggler> AutoDisableToggler { get; }

        public MenuItem<AbilityToggler> LinkenBreakerToggler { get; }

        public MenuItem<PriorityChanger> LinkenBreakerChanger { get; }

        public MenuItem<AbilityToggler> AntiMageBreakerToggler { get; }

        public MenuItem<PriorityChanger> AntiMageBreakerChanger { get; }

        public MenuItem<bool> UseOnlyFromRangeItem { get; }

        public MenuItem<StringList> TargetEffectTypeItem { get; }

        public MenuItem<bool> DrawTargetItem { get; }

        public MenuItem<Slider> TargetRedItem { get; }

        public MenuItem<Slider> TargetGreenItem { get; }

        public MenuItem<Slider> TargetBlueItem { get; }

        public MenuItem<bool> DrawOffTargetItem { get; }

        public MenuItem<Slider> OffTargetRedItem { get; }

        public MenuItem<Slider> OffTargetGreenItem { get; }

        public MenuItem<Slider> OffTargetBlueItem { get; }

        public MenuItem<bool> ArcaneBoltRadiusItem { get; }

        public MenuItem<bool> ConcussiveShotRadiusItem { get; }

        public MenuItem<bool> AncientSealRadiusItem { get; }

        public MenuItem<bool> MysticFlareRadiusItem { get; }

        public MenuItem<bool> WDrawItem { get; }

        public MenuItem<bool> BlinkRadiusItem { get; }

        public MenuItem<bool> CalculationItem { get; }

        public MenuItem<Slider> CalculationXItem { get; }

        public MenuItem<Slider> CalculationYItem { get; }

        public MenuItem<bool> TextItem { get; }

        public MenuItem<Slider> TextXItem { get; }

        public MenuItem<Slider> TextYItem { get; }

        public MenuItem<KeyBind> ComboKeyItem { get; }

        public MenuItem<StringList> OrbwalkerItem { get; }

        public MenuItem<Slider> MinDisInOrbwalkItem { get; }

        public MenuItem<StringList> TargetItem { get; }

        public MenuItem<KeyBind> StartComboKeyItem { get; }

        public MenuItem<KeyBind> AutoQKeyItem { get; }

        public MenuItem<KeyBind> SpamKeyItem { get; }

        public MenuItem<bool> SpamUnitItem { get; }

        public MenuItem<bool> WWithoutFailItem { get; }

        public MenuItem<bool> WTargetItem { get; }

        public MenuItem<Slider> WRadiusItem { get; }

        public MenuItem<bool> BladeMailItem { get; }

        public MenuItem<bool> EulBladeMailItem { get; }


        public MenuManager(Config config)
        {
            Factory = MenuFactory.CreateWithTexture("SkywrathMagePlus", "npc_dota_hero_skywrath_mage");
            Factory.Target.SetFontColor(Color.Aqua);

            var AbilitiesMenu = Factory.Menu("Abilities");
            AbilitiesToggler = AbilitiesMenu.Item("Use: ", new AbilityToggler(new Dictionary<string, bool>
            {
                { "skywrath_mage_mystic_flare", true },
                { "skywrath_mage_ancient_seal", true },
                { "skywrath_mage_concussive_shot", true },
                { "skywrath_mage_arcane_bolt", true }
            }));

            var ItemsMenu = Factory.Menu("Items");
            ItemsToggler = ItemsMenu.Item("Use: ", new AbilityToggler(new Dictionary<string, bool>
            {
                { "item_blink", false },
                { "item_shivas_guard", true },
                { "item_dagon_5", true },
                { "item_veil_of_discord", true },
                { "item_ethereal_blade", true },
                { "item_rod_of_atos", true },
                { "item_bloodthorn", true },
                { "item_orchid", true },
                { "item_sheepstick", true }
            }));

            BlinkActivationItem = ItemsMenu.Item("Blink Activation Distance Mouse", new Slider(1000, 0, 1200));
            BlinkDistanceEnemyItem = ItemsMenu.Item("Blink Distance From Enemy", new Slider(300, 0, 500));

            var AutoComboMenu = Factory.Menu("Auto Combo");
            AutoComboItem = AutoComboMenu.Item("Enable", true);
            AutoAbilitiesToggler = AutoComboMenu.Item("Abilities: ", "AutoAbilitiesToggler", new AbilityToggler(new Dictionary<string, bool>
            {
                { "skywrath_mage_mystic_flare", true },
                { "skywrath_mage_ancient_seal", true },
                { "skywrath_mage_concussive_shot", true },
                { "skywrath_mage_arcane_bolt", true }
            }));

            AutoItemsToggler = AutoComboMenu.Item("Items: ", "AutoItemsToggler", new AbilityToggler(new Dictionary<string, bool>
            {
                { "item_shivas_guard", true },
                { "item_dagon_5", true },
                { "item_veil_of_discord", true },
                { "item_ethereal_blade", true },
                { "item_rod_of_atos", true },
                { "item_bloodthorn", true },
                { "item_orchid", true },
                { "item_sheepstick", true }
            }));

            var AutoKillStealMenu = Factory.Menu("Auto Kill Steal");
            AutoKillStealItem = AutoKillStealMenu.Item("Enable", true);
            AutoKillStealToggler = AutoKillStealMenu.Item("Use: ", "AutoKillStealToggler", new AbilityToggler(new Dictionary<string, bool>
            {
                { "skywrath_mage_arcane_bolt", true },
                { "skywrath_mage_concussive_shot", true },
                { "item_shivas_guard", true },
                { "item_dagon_5", true },
                { "item_ethereal_blade", true },
                { "item_veil_of_discord", true },
                { "skywrath_mage_ancient_seal", true }
            }));

            var AutoDisableMenu = Factory.MenuWithTexture("Auto Disable", "item_sheepstick");
            AutoDisableItem = AutoDisableMenu.Item("Enable", true);
            AutoDisableToggler = AutoDisableMenu.Item("Use: ", new AbilityToggler(new Dictionary<string, bool>
            {
                { "skywrath_mage_ancient_seal", true },
                { "item_bloodthorn", true },
                { "item_orchid", true },
                { "item_sheepstick", true }
            }));

            var LinkenBreakerMenu = Factory.MenuWithTexture("Linken Breaker", "item_sphere");
            LinkenBreakerMenu.Target.AddItem(new MenuItem("linkensphere", "Linkens Sphere:"));
            LinkenBreakerToggler = LinkenBreakerMenu.Item("Use: ", "linkentoggler", new AbilityToggler(new Dictionary<string, bool>
            {
                { "skywrath_mage_ancient_seal", true },
                { "skywrath_mage_arcane_bolt", true },
                { "item_sheepstick", true},
                { "item_rod_of_atos", true},
                { "item_bloodthorn", true },
                { "item_orchid", true },
                { "item_cyclone", true },
                { "item_force_staff", true }
            }));

            LinkenBreakerChanger = LinkenBreakerMenu.Item("Priority: ", "linkenchanger", new PriorityChanger(new List<string>
            {
                { "skywrath_mage_ancient_seal" },
                { "skywrath_mage_arcane_bolt" },
                { "item_sheepstick" },
                { "item_rod_of_atos" },
                { "item_bloodthorn" },
                { "item_orchid" },
                { "item_cyclone" },
                { "item_force_staff" }
            }));

            LinkenBreakerMenu.Target.AddItem(new MenuItem("empty", ""));

            LinkenBreakerMenu.Target.AddItem(new MenuItem("antiMagespellshield", "AntiMage Spell Shield:"));
            AntiMageBreakerToggler = LinkenBreakerMenu.Item("Use: ", "antimagetoggler", new AbilityToggler(new Dictionary<string, bool>
            {
                { "skywrath_mage_ancient_seal", true },
                { "skywrath_mage_arcane_bolt", true },
                { "item_rod_of_atos", true},
                { "item_cyclone", true },
                { "item_force_staff", true }
            }));

            AntiMageBreakerChanger = LinkenBreakerMenu.Item("Priority: ", "antimagechanger", new PriorityChanger(new List<string>
            {
                { "skywrath_mage_ancient_seal" },
                { "skywrath_mage_arcane_bolt" },
                { "item_rod_of_atos" },
                { "item_cyclone" },
                { "item_force_staff" }
            }));

            UseOnlyFromRangeItem = LinkenBreakerMenu.Item("Use Only From Range", false);
            UseOnlyFromRangeItem.Item.SetTooltip("Use only from the Range and do not use another Ability");

            var BladeMailMenu = Factory.MenuWithTexture("Blade Mail", "item_blade_mail");
            BladeMailItem = BladeMailMenu.Item("Cancel Combo", true);
            BladeMailItem.Item.SetTooltip("Cancel Combo if there is enemy Blade Mail");
            EulBladeMailItem = BladeMailMenu.Item("Use Eul", true);
            EulBladeMailItem.Item.SetTooltip("Use Eul if there is BladeMail with ULT");

            var ConcussiveShotMenu = Factory.MenuWithTexture("Smart Concussive Shot", "skywrath_mage_concussive_shot");
            WWithoutFailItem = ConcussiveShotMenu.Item("Without Fail", true);
            WTargetItem = ConcussiveShotMenu.Item("Use Only Target", true);
            WTargetItem.Item.SetTooltip("This only works with Combo");
            WRadiusItem = ConcussiveShotMenu.Item("Use in Radius", new Slider(1400, 800, 1600));
            WRadiusItem.Item.SetTooltip("This only works with Combo");

            var DrawingMenu = Factory.Menu("Drawing");
            var TargetMenu = DrawingMenu.Menu("Target");
            TargetEffectTypeItem = TargetMenu.Item("Target Effect Type", new StringList(EffectsName));
            DrawTargetItem = TargetMenu.Item("Target Enable", true);
            TargetRedItem = TargetMenu.Item("Red", "red", new Slider(255, 0, 255));
            TargetRedItem.Item.SetFontColor(Color.Red);
            TargetGreenItem = TargetMenu.Item("Green", "green", new Slider(0, 0, 255));
            TargetGreenItem.Item.SetFontColor(Color.Green);
            TargetBlueItem = TargetMenu.Item("Blue", "blue", new Slider(0, 0, 255));
            TargetBlueItem.Item.SetFontColor(Color.Blue);

            DrawOffTargetItem = TargetMenu.Item("Off Target Enable", true);
            OffTargetRedItem = TargetMenu.Item("Red", "offred", new Slider(0, 0, 255));
            OffTargetRedItem.Item.SetFontColor(Color.Red);
            OffTargetGreenItem = TargetMenu.Item("Green", "offgreen", new Slider(255, 0, 255));
            OffTargetGreenItem.Item.SetFontColor(Color.Green);
            OffTargetBlueItem = TargetMenu.Item("Blue", "offblue", new Slider(255, 0, 255));
            OffTargetBlueItem.Item.SetFontColor(Color.Blue);

            var RadiusMenu = DrawingMenu.Menu("Radius");
            ArcaneBoltRadiusItem = RadiusMenu.Item("Arcane Bolt Radius", true);
            ConcussiveShotRadiusItem = RadiusMenu.Item("Concussive Shot Radius", true);
            AncientSealRadiusItem = RadiusMenu.Item("Ancient Seal Radius", true);
            MysticFlareRadiusItem = RadiusMenu.Item("Mystic Flare Radius", true);
            WDrawItem = RadiusMenu.Item("Show W Target", true);
            BlinkRadiusItem = RadiusMenu.Item("Blink Radius", false);

            CalculationItem = DrawingMenu.Item("Damage Calculation", true);
            CalculationXItem = DrawingMenu.Item("X", "calculation_x", new Slider(0, 0, (int)config.Screen.X + 65));
            CalculationYItem = DrawingMenu.Item("Y", "calculation_y", new Slider((int)config.Screen.Y - 260, 0, (int)config.Screen.Y - 200));

            TextItem = DrawingMenu.Item("Text", true);
            TextXItem = DrawingMenu.Item("X", new Slider((int)config.Screen.X - 50, 0, (int)config.Screen.X - 50));
            TextYItem = DrawingMenu.Item("Y", new Slider(0, 0, (int)config.Screen.Y - 280));

            ComboKeyItem = Factory.Item("Combo Key", new KeyBind('D'));
            OrbwalkerItem = Factory.Item("Orbwalker", new StringList("Default", "Distance", "Free"));
            MinDisInOrbwalkItem = Factory.Item("Min Distance In Orbwalker", new Slider(600, 200, 600));
            TargetItem = Factory.Item("Target", new StringList("Lock", "Default"));
            StartComboKeyItem = Factory.Item("Start Combo With Mute", new KeyBind('0', KeyBindType.Toggle, false));
            StartComboKeyItem.Item.SetTooltip("Start Combo With Hex or Ancient Seal");

            AutoQKeyItem = Factory.Item("Auto Q Key", new KeyBind('F', KeyBindType.Toggle, false));
            AutoQKeyItem.Item.SetValue(new KeyBind(AutoQKeyItem.Item.GetValue<KeyBind>().Key, KeyBindType.Toggle, false));
            SpamKeyItem = Factory.Item("Spam Q Key", new KeyBind('Q'));
            SpamUnitItem = Factory.Item("Spam Q Units", true);

            ItemsToggler.PropertyChanged += Changed;
            DrawTargetItem.PropertyChanged += Changed;
            DrawOffTargetItem.PropertyChanged += Changed;
            OrbwalkerItem.PropertyChanged += Changed;

            Changed(null, null);
        }

        private void Changed(object sender, PropertyChangedEventArgs e)
        {
            // Blink
            if (ItemsToggler.Value.IsEnabled("item_blink"))
            {
                BlinkActivationItem.Item.ShowItem = true;
                BlinkDistanceEnemyItem.Item.ShowItem = true;
            }
            else
            {
                BlinkActivationItem.Item.ShowItem = false;
                BlinkDistanceEnemyItem.Item.ShowItem = false;
            }

            // Draw Target
            if (DrawTargetItem)
            {
                TargetRedItem.Item.ShowItem = true;
                TargetGreenItem.Item.ShowItem = true;
                TargetBlueItem.Item.ShowItem = true;
            }
            else
            {
                TargetRedItem.Item.ShowItem = false;
                TargetGreenItem.Item.ShowItem = false;
                TargetBlueItem.Item.ShowItem = false;
            }

            // Draw Off Target
            if (DrawOffTargetItem)
            {
                OffTargetRedItem.Item.ShowItem = true;
                OffTargetGreenItem.Item.ShowItem = true;
                OffTargetBlueItem.Item.ShowItem = true;
            }
            else
            {
                OffTargetRedItem.Item.ShowItem = false;
                OffTargetGreenItem.Item.ShowItem = false;
                OffTargetBlueItem.Item.ShowItem = false;
            }

            // Orbwalker Distance
            if (OrbwalkerItem.Value.SelectedValue.Contains("Distance"))
            {
                MinDisInOrbwalkItem.Item.ShowItem = true;
            }
            else
            {
                MinDisInOrbwalkItem.Item.ShowItem = false;
            }
        }

        private string[] EffectsName { get; } =
        {
            "Default",
            "Without Circle",
            "VBE",
            "Omniknight",
            "Assault",
            "Arrow",
            "Glyph",
            "Energy Orb",
            "Pentagon",
            "Beam Jagged",
            "Beam Rainbow",
            "Walnut Statue",
            "Thin Thick",
            "Ring Wave"
        };

        public string[] Effects { get; } =
        {
            "",
            "",
            "materials/ensage_ui/particles/vbe.vpcf",
            "materials/ensage_ui/particles/visiblebyenemy_omniknight.vpcf",
            "materials/ensage_ui/particles/visiblebyenemy_assault.vpcf",
            "materials/ensage_ui/particles/visiblebyenemy_arrow.vpcf",
            "materials/ensage_ui/particles/visiblebyenemy_glyph.vpcf",
            "materials/ensage_ui/particles/visiblebyenemy_energy_orb.vpcf",
            "materials/ensage_ui/particles/visiblebyenemy_pentagon.vpcf",
            "materials/ensage_ui/particles/visiblebyenemy_beam_jagged.vpcf",
            "materials/ensage_ui/particles/visiblebyenemy_beam_rainbow.vpcf",
            "materials/ensage_ui/particles/visiblebyenemy_walnut_statue.vpcf",
            "materials/ensage_ui/particles/visiblebyenemy_thin_thick.vpcf",
            "materials/ensage_ui/particles/visiblebyenemy_ring_wave.vpcf"
        };

        public void Dispose()
        {
            OrbwalkerItem.PropertyChanged -= Changed;
            DrawOffTargetItem.PropertyChanged -= Changed;
            DrawTargetItem.PropertyChanged -= Changed;
            ItemsToggler.PropertyChanged -= Changed;
            Factory.Dispose();
        }
    }
}

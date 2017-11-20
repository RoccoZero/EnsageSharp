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

        public MenuItem<AbilityToggler> AbilityToggler { get; }

        public MenuItem<Slider> MinHealthToUltItem { get; }

        public MenuItem<bool> BadUltItem { get; }

        public MenuItem<Slider> BadUltMovementSpeedItem { get; }

        public MenuItem<AbilityToggler> ItemToggler { get; }

        public MenuItem<Slider> BlinkActivationItem { get; }

        public MenuItem<Slider> BlinkDistanceEnemyItem { get; }

        public MenuItem<bool> ComboBreakerItem { get; }

        public MenuItem<KeyBind> ComboKeyItem { get; }

        public MenuItem<StringList> OrbwalkerItem { get; }

        public MenuItem<Slider> MinDisInOrbwalkItem { get; }

        public MenuItem<StringList> TargetItem { get; }

        public MenuItem<KeyBind> StartComboKeyItem { get; }

        public MenuItem<bool> AutoComboItem { get; }

        public MenuItem<bool> AutoComboWhenComboItem { get; }

        public MenuItem<Slider> AutoOwnerMinHealthItem { get; }

        public MenuItem<AbilityToggler> AutoAbilityToggler { get; }

        public MenuItem<AbilityToggler> AutoItemToggler { get; }

        public MenuItem<Slider> AutoMinHealthToUltItem { get; }

        public MenuItem<bool> AutoKillStealItem { get; }

        public MenuItem<bool> AutoKillWhenComboItem { get; }

        public MenuItem<AbilityToggler> AutoKillStealToggler { get; }

        public MenuItem<bool> AutoDisableItem { get; }

        public MenuItem<AbilityToggler> AutoDisableToggler { get; }

        public MenuItem<AbilityToggler> LinkenBreakerToggler { get; }

        public MenuItem<PriorityChanger> LinkenBreakerChanger { get; }

        public MenuItem<AbilityToggler> AntiMageBreakerToggler { get; }

        public MenuItem<PriorityChanger> AntiMageBreakerChanger { get; }

        public MenuItem<bool> UseOnlyFromRangeItem { get; }

        public MenuItem<bool> BladeMailItem { get; }

        public MenuItem<bool> EulBladeMailItem { get; }

        public MenuItem<KeyBind> AutoArcaneBoltKeyItem { get; }

        public MenuItem<Slider> AutoArcaneBoltOwnerMinHealthItem { get; }

        public MenuItem<KeyBind> SpamArcaneBoltKeyItem { get; }

        public MenuItem<bool> SpamArcaneBoltUnitItem { get; }

        public MenuItem<StringList> OrbwalkerArcaneBoltItem { get; }

        public MenuItem<Slider> MinDisInOrbwalkArcaneBoltItem { get; }

        public MenuItem<bool> ConcussiveShotWithoutFailItem { get; }

        public MenuItem<bool> ConcussiveShotTargetItem { get; }

        public MenuItem<Slider> ConcussiveShotUseRadiusItem { get; }

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

        public MenuItem<bool> TargetHitConcussiveShotItem { get; }

        public MenuItem<bool> BlinkRadiusItem { get; }

        public MenuItem<bool> CalculationItem { get; }

        public MenuItem<Slider> CalculationXItem { get; }

        public MenuItem<Slider> CalculationYItem { get; }

        public MenuItem<bool> HPBarCalculationItem { get; }

        public MenuItem<Slider> HPBarCalculationPosItem { get; }

        public MenuItem<bool> TextItem { get; }

        public MenuItem<Slider> TextXItem { get; }

        public MenuItem<Slider> TextYItem { get; }

        public MenuManager(Config config)
        {
            Factory = MenuFactory.CreateWithTexture("SkywrathMagePlus", "npc_dota_hero_skywrath_mage");
            Factory.Target.SetFontColor(Color.Aqua);

            var comboMenu = Factory.Menu("Combo");
            var abilitiesMenu = comboMenu.Menu("Abilities");
            AbilityToggler = abilitiesMenu.Item("Use: ", new AbilityToggler(new Dictionary<string, bool>
            {
                { "skywrath_mage_mystic_flare", true },
                { "skywrath_mage_ancient_seal", true },
                { "skywrath_mage_concussive_shot", true },
                { "skywrath_mage_arcane_bolt", true }
            }));

            MinHealthToUltItem = abilitiesMenu.Item("Target Min Health % To Ult", new Slider(0, 0, 70));
            BadUltItem = abilitiesMenu.Item("Bad Ult", false);
            BadUltItem.Item.SetTooltip("It is not recommended to enable this. If you do not have these items (RodofAtos, Hex, Ethereal) then this function is activated");
            BadUltMovementSpeedItem = abilitiesMenu.Item("Bad Ult Movement Speed", new Slider(500, 240, 500));
            BadUltMovementSpeedItem.Item.SetTooltip("If the enemy has less Movement Speed from this value, then immediately ULT");

            var itemsMenu = comboMenu.Menu("Items");
            ItemToggler = itemsMenu.Item("Use: ", new AbilityToggler(new Dictionary<string, bool>
            {
                { "item_blink", false },
                { "item_spirit_vessel", true },
                { "item_urn_of_shadows", true },
                { "item_shivas_guard", true },
                { "item_dagon_5", true },
                { "item_veil_of_discord", true },
                { "item_ethereal_blade", true },
                { "item_rod_of_atos", true },
                { "item_nullifier", true },
                { "item_bloodthorn", true },
                { "item_orchid", true },
                { "item_sheepstick", true }
            }));

            BlinkActivationItem = itemsMenu.Item("Blink Activation Distance Mouse", new Slider(1000, 0, 1200));
            BlinkDistanceEnemyItem = itemsMenu.Item("Blink Distance From Enemy", new Slider(300, 0, 500));

            var comboBreakerMenu = comboMenu.MenuWithTexture("Combo Breaker", "item_combo_breaker");
            ComboBreakerItem = comboBreakerMenu.Item("Cancel Important Items and Abilities", true);
            ComboBreakerItem.Item.SetTooltip("If Combo Breaker is ready then it will not use Important Items and Abilities");

            ComboKeyItem = comboMenu.Item("Combo Key", new KeyBind('D'));
            OrbwalkerItem = comboMenu.Item("Orbwalker", new StringList("Default", "Distance", "Free", "Only Attack", "No Move"));
            MinDisInOrbwalkItem = comboMenu.Item("Min Distance In Orbwalker", new Slider(600, 200, 600));
            TargetItem = comboMenu.Item("Target", new StringList("Lock", "Default"));
            StartComboKeyItem = comboMenu.Item("Start Combo With Mute", new KeyBind('0', KeyBindType.Toggle, false));
            StartComboKeyItem.Item.SetTooltip("Start Combo With Hex or Ancient Seal");

            var autoComboMenu = Factory.Menu("Auto Combo");
            AutoComboItem = autoComboMenu.Item("Enable", true);
            AutoComboWhenComboItem = autoComboMenu.Item("Disable When Combo", true);
            AutoOwnerMinHealthItem = autoComboMenu.Item("Owner Min Health % To Auto Combo", new Slider(0, 0, 70));
            AutoAbilityToggler = autoComboMenu.Item("Abilities: ", "autoabilitiestoggler", new AbilityToggler(new Dictionary<string, bool>
            {
                { "skywrath_mage_mystic_flare", true },
                { "skywrath_mage_ancient_seal", true },
                { "skywrath_mage_concussive_shot", true },
                { "skywrath_mage_arcane_bolt", true }
            }));

            AutoItemToggler = autoComboMenu.Item("Items: ", "autoitemstoggler", new AbilityToggler(new Dictionary<string, bool>
            {
                { "item_shivas_guard", true },
                { "item_dagon_5", true },
                { "item_veil_of_discord", true },
                { "item_ethereal_blade", true },
                { "item_rod_of_atos", true },
                { "item_nullifier", true },
                { "item_bloodthorn", true },
                { "item_orchid", true },
                { "item_sheepstick", true }
            }));

            AutoMinHealthToUltItem = autoComboMenu.Item("Target Min Health % To Ult", new Slider(0, 0, 70));

            var autoKillStealMenu = Factory.Menu("Auto Kill Steal");
            AutoKillStealItem = autoKillStealMenu.Item("Enable", true);
            AutoKillWhenComboItem = autoKillStealMenu.Item("Disable When Combo", false);
            AutoKillStealToggler = autoKillStealMenu.Item("Use: ", "autokillstealtoggler", new AbilityToggler(new Dictionary<string, bool>
            {
                { "skywrath_mage_arcane_bolt", true },
                { "skywrath_mage_concussive_shot", true },
                { "item_shivas_guard", true },
                { "item_dagon_5", true },
                { "item_ethereal_blade", true },
                { "item_veil_of_discord", true },
                { "skywrath_mage_ancient_seal", true }
            }));

            var autoDisableMenu = Factory.MenuWithTexture("Auto Disable", "item_sheepstick");
            AutoDisableItem = autoDisableMenu.Item("Enable", true);
            AutoDisableToggler = autoDisableMenu.Item("Use: ", new AbilityToggler(new Dictionary<string, bool>
            {
                { "skywrath_mage_ancient_seal", true },
                { "item_bloodthorn", true },
                { "item_orchid", true },
                { "item_sheepstick", true }
            }));

            var linkenBreakerMenu = Factory.MenuWithTexture("Linken Breaker", "item_sphere");
            linkenBreakerMenu.Target.AddItem(new MenuItem("linkensphere", "Linkens Sphere:"));
            LinkenBreakerToggler = linkenBreakerMenu.Item("Use: ", "linkentoggler", new AbilityToggler(new Dictionary<string, bool>
            {
                { "skywrath_mage_ancient_seal", true },
                { "skywrath_mage_arcane_bolt", true },
                { "item_sheepstick", true},
                { "item_rod_of_atos", true},
                { "item_nullifier", true },
                { "item_bloodthorn", true },
                { "item_orchid", true },
                { "item_cyclone", true },
                { "item_force_staff", true }
            }));

            LinkenBreakerChanger = linkenBreakerMenu.Item("Priority: ", "linkenchanger", new PriorityChanger(new List<string>
            {
                { "skywrath_mage_ancient_seal" },
                { "skywrath_mage_arcane_bolt" },
                { "item_sheepstick" },
                { "item_rod_of_atos" },
                { "item_nullifier" },
                { "item_bloodthorn" },
                { "item_orchid" },
                { "item_cyclone" },
                { "item_force_staff" }
            }));

            linkenBreakerMenu.Target.AddItem(new MenuItem("empty", ""));

            linkenBreakerMenu.Target.AddItem(new MenuItem("antimagespellshield", "Anti Mage Spell Shield:"));
            AntiMageBreakerToggler = linkenBreakerMenu.Item("Use: ", "antimagetoggler", new AbilityToggler(new Dictionary<string, bool>
            {
                { "skywrath_mage_ancient_seal", true },
                { "skywrath_mage_arcane_bolt", true },
                { "item_rod_of_atos", true},
                { "item_cyclone", true },
                { "item_force_staff", true }
            }));

            AntiMageBreakerChanger = linkenBreakerMenu.Item("Priority: ", "antimagechanger", new PriorityChanger(new List<string>
            {
                { "skywrath_mage_ancient_seal" },
                { "skywrath_mage_arcane_bolt" },
                { "item_rod_of_atos" },
                { "item_cyclone" },
                { "item_force_staff" }
            }));

            UseOnlyFromRangeItem = linkenBreakerMenu.Item("Use Only From Range", false);
            UseOnlyFromRangeItem.Item.SetTooltip("Use only from the Range and do not use another Ability");

            var arcaneBoltMenu = Factory.MenuWithTexture("Smart Arcane Bolt", "skywrath_mage_arcane_bolt");
            AutoArcaneBoltKeyItem = arcaneBoltMenu.Item("Auto Arcane Bolt Key", new KeyBind('F', KeyBindType.Toggle, false));
            AutoArcaneBoltKeyItem.Item.SetValue(new KeyBind(AutoArcaneBoltKeyItem.Item.GetValue<KeyBind>().Key, KeyBindType.Toggle, false));
            AutoArcaneBoltOwnerMinHealthItem = arcaneBoltMenu.Item("Owner Min Health % To Auto Arcane Bolt", new Slider(20, 0, 70));

            arcaneBoltMenu.Target.AddItem(new MenuItem("empty2", ""));

            SpamArcaneBoltKeyItem = arcaneBoltMenu.Item("Spam Arcane Bolt Key", new KeyBind('Q'));
            SpamArcaneBoltUnitItem = arcaneBoltMenu.Item("Spam Arcane Bolt Units", true);
            OrbwalkerArcaneBoltItem = arcaneBoltMenu.Item("Orbwalker", new StringList("Distance", "Default", "Free", "Only Attack", "No Move"));
            MinDisInOrbwalkArcaneBoltItem = arcaneBoltMenu.Item("Min Distance In Orbwalker", new Slider(600, 200, 600));

            var concussiveShotMenu = Factory.MenuWithTexture("Smart Concussive Shot", "skywrath_mage_concussive_shot");
            ConcussiveShotWithoutFailItem = concussiveShotMenu.Item("Without Fail", true);
            ConcussiveShotTargetItem = concussiveShotMenu.Item("Use Only Target", true);
            ConcussiveShotTargetItem.Item.SetTooltip("This only works with Combo");
            ConcussiveShotUseRadiusItem = concussiveShotMenu.Item("Use in Radius", new Slider(1400, 800, 1600));
            ConcussiveShotUseRadiusItem.Item.SetTooltip("This only works with Combo");

            var bladeMailMenu = Factory.MenuWithTexture("Blade Mail", "item_blade_mail");
            BladeMailItem = bladeMailMenu.Item("Cancel Combo", true);
            BladeMailItem.Item.SetTooltip("Cancel Combo if there is enemy Blade Mail");
            EulBladeMailItem = bladeMailMenu.Item("Use Eul", true);
            EulBladeMailItem.Item.SetTooltip("Use Eul if there is BladeMail with ULT");

            var drawingMenu = Factory.Menu("Drawing");
            var targetMenu = drawingMenu.Menu("Target");
            TargetEffectTypeItem = targetMenu.Item("Target Effect Type", new StringList(EffectsName));
            DrawTargetItem = targetMenu.Item("Target Enable", true);
            TargetRedItem = targetMenu.Item("Red", "red", new Slider(255, 0, 255));
            TargetRedItem.Item.SetFontColor(Color.Red);
            TargetGreenItem = targetMenu.Item("Green", "green", new Slider(0, 0, 255));
            TargetGreenItem.Item.SetFontColor(Color.Green);
            TargetBlueItem = targetMenu.Item("Blue", "blue", new Slider(0, 0, 255));
            TargetBlueItem.Item.SetFontColor(Color.Blue);

            DrawOffTargetItem = targetMenu.Item("Off Target Enable", true);
            OffTargetRedItem = targetMenu.Item("Red", "offred", new Slider(0, 0, 255));
            OffTargetRedItem.Item.SetFontColor(Color.Red);
            OffTargetGreenItem = targetMenu.Item("Green", "offgreen", new Slider(255, 0, 255));
            OffTargetGreenItem.Item.SetFontColor(Color.Green);
            OffTargetBlueItem = targetMenu.Item("Blue", "offblue", new Slider(255, 0, 255));
            OffTargetBlueItem.Item.SetFontColor(Color.Blue);

            var calculationMenu = drawingMenu.Menu("Damage Calculation");
            CalculationItem = calculationMenu.Item("Enable", true);
            CalculationXItem = calculationMenu.Item("X", new Slider(0, 0, (int)config.Screen.X + 65));
            CalculationYItem = calculationMenu.Item("Y", new Slider((int)config.Screen.Y - 260, 0, (int)config.Screen.Y - 200));

            var hpBarCalculationMenu = drawingMenu.Menu("HP Bar Damage Calculation");
            HPBarCalculationItem = hpBarCalculationMenu.Item("Enable", true);
            HPBarCalculationPosItem = hpBarCalculationMenu.Item("Damage Bar Position", new Slider(84, 0, 100));

            var textMenu = drawingMenu.Menu("Text");
            TextItem = textMenu.Item("Enable", true);
            TextXItem = textMenu.Item("X", new Slider((int)config.Screen.X, 0, (int)config.Screen.X));
            TextYItem = textMenu.Item("Y", new Slider(0, 0, (int)config.Screen.Y - 240));

            var radiusMenu = drawingMenu.Menu("Radius");
            ArcaneBoltRadiusItem = radiusMenu.Item("Arcane Bolt", true);
            ConcussiveShotRadiusItem = radiusMenu.Item("Concussive Shot", true);
            AncientSealRadiusItem = radiusMenu.Item("Ancient Seal", true);
            MysticFlareRadiusItem = radiusMenu.Item("Mystic Flare", true);
            TargetHitConcussiveShotItem = radiusMenu.Item("Target Hit Concussive Shot", true);
            BlinkRadiusItem = radiusMenu.Item("Blink", false);

            AbilityToggler.PropertyChanged += Changed;
            BadUltItem.PropertyChanged += Changed;
            ItemToggler.PropertyChanged += Changed;
            AutoComboItem.PropertyChanged += Changed;
            AutoAbilityToggler.PropertyChanged += Changed;
            OrbwalkerArcaneBoltItem.PropertyChanged += Changed;
            DrawTargetItem.PropertyChanged += Changed;
            DrawOffTargetItem.PropertyChanged += Changed;
            OrbwalkerItem.PropertyChanged += Changed;

            Changed(null, null);
        }

        private void Changed(object sender, PropertyChangedEventArgs e)
        {
            // Bad Ult
            if (BadUltItem)
            {
                BadUltMovementSpeedItem.Item.ShowItem = true;
            }
            else
            {
                BadUltMovementSpeedItem.Item.ShowItem = false;
            }

            // Mystic Flare
            if (AbilityToggler.Value.IsEnabled("skywrath_mage_mystic_flare"))
            {
                MinHealthToUltItem.Item.ShowItem = true;
                BadUltItem.Item.ShowItem = true;
            }
            else
            {
                MinHealthToUltItem.Item.ShowItem = false;
                BadUltItem.Item.ShowItem = false;
                BadUltMovementSpeedItem.Item.ShowItem = false;
            }

            // Blink
            if (ItemToggler.Value.IsEnabled("item_blink"))
            {
                BlinkActivationItem.Item.ShowItem = true;
                BlinkDistanceEnemyItem.Item.ShowItem = true;
            }
            else
            {
                BlinkActivationItem.Item.ShowItem = false;
                BlinkDistanceEnemyItem.Item.ShowItem = false;
            }

            // Auto Mystic Flare
            if (AutoAbilityToggler.Value.IsEnabled("skywrath_mage_mystic_flare"))
            {
                AutoMinHealthToUltItem.Item.ShowItem = true;
            }
            else
            {
                AutoMinHealthToUltItem.Item.ShowItem = false;
            }

            // Auto Combo
            if (AutoComboItem)
            {
                AutoAbilityToggler.Item.ShowItem = true;
                AutoItemToggler.Item.ShowItem = true;
            }
            else
            {
                AutoAbilityToggler.Item.ShowItem = false;
                AutoItemToggler.Item.ShowItem = false;
                AutoMinHealthToUltItem.Item.ShowItem = false;
            }
            
            // Orbwalker Arcane Bolt Distance
            if (OrbwalkerArcaneBoltItem.Value.SelectedValue.Contains("Distance"))
            {
                MinDisInOrbwalkArcaneBoltItem.Item.ShowItem = true;
            }
            else
            {
                MinDisInOrbwalkArcaneBoltItem.Item.ShowItem = false;
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
            OrbwalkerArcaneBoltItem.PropertyChanged -= Changed;
            AutoAbilityToggler.PropertyChanged -= Changed;
            AutoComboItem.PropertyChanged -= Changed;
            ItemToggler.PropertyChanged -= Changed;
            BadUltItem.PropertyChanged -= Changed;
            AbilityToggler.PropertyChanged -= Changed;

            Factory.Dispose();
        }
    }
}

using System.Collections.Generic;
using System.ComponentModel;

using Ensage.Common.Menu;
using Ensage.SDK.Menu;

using SharpDX;

namespace VisagePlus
{
    internal class MenuManager
    {
        private MenuFactory Factory { get; }

        public MenuItem<AbilityToggler> AbilityToggler { get; }

        public MenuItem<AbilityToggler> ItemToggler { get; }

        public MenuItem<Slider> BlinkActivationItem { get; }

        public MenuItem<Slider> BlinkDistanceEnemyItem { get; }

        public MenuItem<bool> AutoKillStealItem { get; }

        public MenuItem<AbilityToggler> AutoKillStealToggler { get; }

        public MenuItem<AbilityToggler> LinkenBreakerToggler { get; }

        public MenuItem<PriorityChanger> LinkenBreakerChanger { get; }

        public MenuItem<AbilityToggler> AntiMageBreakerToggler { get; }

        public MenuItem<PriorityChanger> AntiMageBreakerChanger { get; }

        public MenuItem<bool> UseOnlyFromRangeItem { get; }

        public MenuItem<bool> AutoSoulAssumptionItem { get; }

        public MenuItem<KeyBind> LastHitItem { get; }

        public MenuItem<bool> DenyItem { get; }

        public MenuItem<bool> CommonAttackItem { get; }

        public MenuItem<KeyBind> FamiliarsLockItem { get; }

        public MenuItem<KeyBind> FollowKeyItem { get; }

        public MenuItem<bool> FamiliarsFollowItem { get; }

        public MenuItem<Slider> FamiliarsLowHPItem { get; }

        public MenuItem<bool> FamiliarsCourierItem { get; }

        public MenuItem<StringList> TargetEffectTypeItem { get; }

        public MenuItem<bool> DrawTargetItem { get; }

        public MenuItem<Slider> TargetRedItem { get; }

        public MenuItem<Slider> TargetGreenItem { get; }

        public MenuItem<Slider> TargetBlueItem { get; }

        public MenuItem<bool> DrawOffTargetItem { get; }

        public MenuItem<Slider> OffTargetRedItem { get; }

        public MenuItem<Slider> OffTargetGreenItem { get; }

        public MenuItem<Slider> OffTargetBlueItem { get; }

        public MenuItem<bool> GraveChillRadiusItem { get; }

        public MenuItem<bool> SoulAssumptionRadiusItem { get; }

        public MenuItem<bool> BlinkRadiusItem { get; }

        public MenuItem<bool> CalculationItem { get; }

        public MenuItem<Slider> CalculationXItem { get; }

        public MenuItem<Slider> CalculationYItem { get; }

        public MenuItem<bool> HPBarCalculationItem { get; }

        public MenuItem<Slider> HPBarCalculationPosItem { get; }

        public MenuItem<bool> TextItem { get; }

        public MenuItem<Slider> TextXItem { get; }

        public MenuItem<Slider> TextYItem { get; }

        public MenuItem<KeyBind> ComboKeyItem { get; }

        public MenuItem<StringList> OrbwalkerItem { get; }

        public MenuItem<Slider> MinDisInOrbwalkItem { get; }

        public MenuItem<StringList> TargetItem { get; }

        public MenuItem<KeyBind> EscapeKeyItem { get; }

        public MenuItem<bool> BladeMailItem { get; }

        public MenuManager(Config config)
        {
            Factory = MenuFactory.CreateWithTexture("VisagePlus", "npc_dota_hero_visage");
            Factory.Target.SetFontColor(Color.Aqua);

            var abilitiesMenu = Factory.Menu("Abilities");
            AbilityToggler = abilitiesMenu.Item("Use: ", new AbilityToggler(new Dictionary<string, bool>
            {
                { "visage_summon_familiars_stone_form", true },
                { "visage_soul_assumption", true },
                { "visage_grave_chill", true }
            }));

            var itemsMenu = Factory.Menu("Items");
            ItemToggler = itemsMenu.Item("", "itemtoggler", new AbilityToggler(new Dictionary<string, bool>
            {
                { "item_blink", false },
                { "item_armlet", true },
                { "item_necronomicon_3", true },
                { "item_spirit_vessel", true },
                { "item_urn_of_shadows", true },
                { "item_shivas_guard", true },
                { "item_dagon_5", true },
                { "item_veil_of_discord", true },
                { "item_ethereal_blade", true },
                { "item_heavens_halberd", true },
                { "item_hurricane_pike", true },
                { "item_solar_crest", true },
                { "item_medallion_of_courage", true },
                { "item_rod_of_atos", true },
                { "item_nullifier", true },
                { "item_bloodthorn", true },
                { "item_orchid", true },
                { "item_sheepstick", true }
            }));

            BlinkActivationItem = itemsMenu.Item("Blink Activation Distance Mouse", new Slider(1000, 0, 1200));
            BlinkDistanceEnemyItem = itemsMenu.Item("Blink Distance From Enemy", new Slider(300, 0, 500));

            var autoKillStealMenu = Factory.Menu("Auto Kill Steal");
            AutoKillStealItem = autoKillStealMenu.Item("Enable", true);
            AutoKillStealToggler = autoKillStealMenu.Item("Use: ", "autokillstealtoggler", new AbilityToggler(new Dictionary<string, bool>
            {
                { "visage_soul_assumption", true },
                { "item_shivas_guard", true },
                { "item_dagon_5", true },
                { "item_ethereal_blade", true },
                { "item_veil_of_discord", true }
            }));

            var linkenBreakerMenu = Factory.MenuWithTexture("Linken Breaker", "item_sphere");
            linkenBreakerMenu.Target.AddItem(new MenuItem("linkensphere", "Linkens Sphere:"));
            LinkenBreakerToggler = linkenBreakerMenu.Item("Use: ", "linkentoggler", new AbilityToggler(new Dictionary<string, bool>
            {
                { "visage_soul_assumption", true },
                { "item_sheepstick", true},
                { "item_rod_of_atos", true},
                { "item_nullifier", true },
                { "item_bloodthorn", true },
                { "item_orchid", true },
                { "item_heavens_halberd", true },
                { "item_cyclone", true },
                { "item_force_staff", true }
            }));

            LinkenBreakerChanger = linkenBreakerMenu.Item("Priority: ", "linkenchanger", new PriorityChanger(new List<string>
            {
                { "visage_soul_assumption" },
                { "item_sheepstick" },
                { "item_rod_of_atos" },
                { "item_nullifier" },
                { "item_bloodthorn" },
                { "item_orchid" },
                { "item_heavens_halberd" },
                { "item_cyclone" },
                { "item_force_staff" }
            }));

            linkenBreakerMenu.Target.AddItem(new MenuItem("empty", ""));

            linkenBreakerMenu.Target.AddItem(new MenuItem("antiMagespellshield", "AntiMage Spell Shield:"));
            AntiMageBreakerToggler = linkenBreakerMenu.Item("Use: ", "antimagetoggler", new AbilityToggler(new Dictionary<string, bool>
            {
                { "visage_soul_assumption", true },
                { "item_rod_of_atos", true},
                { "item_heavens_halberd", true },
                { "item_cyclone", true },
                { "item_force_staff", true }
            }));

            AntiMageBreakerChanger = linkenBreakerMenu.Item("Priority: ", "antimagechanger", new PriorityChanger(new List<string>
            {
                { "visage_soul_assumption" },
                { "item_rod_of_atos" },
                { "item_heavens_halberd" },
                { "item_cyclone" },
                { "item_force_staff" }
            }));

            UseOnlyFromRangeItem = linkenBreakerMenu.Item("Use Only From Range", false);
            UseOnlyFromRangeItem.Item.SetTooltip("Use only from the Range and do not use another Ability");

            var autoAbilityMenu = Factory.Menu("Auto Ability");
            AutoSoulAssumptionItem = autoAbilityMenu.Item("Auto Soul Assumption", true);

            var familiarsMenu = Factory.Menu("Familiars");
            var familiarsLastHitMenu = familiarsMenu.Menu("Last Hit");
            LastHitItem = familiarsLastHitMenu.Item("LastHit Key", new KeyBind('W', KeyBindType.Toggle, false));
            DenyItem = familiarsLastHitMenu.Item("Deny", true);
            CommonAttackItem = familiarsLastHitMenu.Item("Common Attack", true);

            FamiliarsLockItem = familiarsMenu.Item("Familiars Target Lock Key", new KeyBind('F', KeyBindType.Toggle, false));
            FollowKeyItem = familiarsMenu.Item("Follow Key", new KeyBind('E', KeyBindType.Toggle, false));
            FamiliarsFollowItem = familiarsMenu.Item("Follow Mouse Position", true);
            FamiliarsFollowItem.Item.SetTooltip("When Combo if there is No Enemy then Follow Mouse Position, Otherwise he Returns to the Hero");
            FamiliarsLowHPItem = familiarsMenu.Item("Low HP %", new Slider(30, 0, 80));
            FamiliarsCourierItem = familiarsMenu.Item("Attack Courier", true);

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
            TextXItem = textMenu.Item("X", new Slider((int)config.Screen.X + 5, 0, (int)config.Screen.X + 5));
            TextYItem = textMenu.Item("Y", new Slider(0, 0, (int)config.Screen.Y - 390));

            var radiusMenu = drawingMenu.Menu("Radius");
            GraveChillRadiusItem = radiusMenu.Item("Grave Chill", true);
            SoulAssumptionRadiusItem = radiusMenu.Item("Soul Assumption", true);
            BlinkRadiusItem = radiusMenu.Item("Blink", false);

            ComboKeyItem = Factory.Item("Combo Key", new KeyBind('D'));
            OrbwalkerItem = Factory.Item("Orbwalker", new StringList("Default", "Distance", "Free", "Only Attack", "No Move"));
            MinDisInOrbwalkItem = Factory.Item("Min Distance In Orbwalker", new Slider(600, 200, 600));
            TargetItem = Factory.Item("Target", new StringList("Lock", "Default"));

            EscapeKeyItem = Factory.Item("Escape Key", new KeyBind('0'));

            BladeMailItem = Factory.Item("Blade Mail Cancel", false);
            BladeMailItem.Item.SetTooltip("Cancel Combo if there is enemy Blade Mail");

            ItemToggler.PropertyChanged += Changed;
            DrawTargetItem.PropertyChanged += Changed;
            DrawOffTargetItem.PropertyChanged += Changed;
            OrbwalkerItem.PropertyChanged += Changed;

            Changed(null, null);
        }

        private void Changed(object sender, PropertyChangedEventArgs e)
        {
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
            ItemToggler.PropertyChanged -= Changed;

            Factory.Dispose();
        }
    }
}

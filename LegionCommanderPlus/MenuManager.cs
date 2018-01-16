using System.Collections.Generic;
using System.ComponentModel;

using Ensage;
using Ensage.Common.Menu;
using Ensage.SDK.Menu;

using SharpDX;

namespace LegionCommanderPlus
{
    internal class MenuManager
    {
        private MenuFactory Factory { get; }

        public MenuItem<AbilityToggler> AbilityToggler { get; }

        public MenuItem<AbilityToggler> ItemToggler { get; }

        public MenuItem<bool> ComboBreakerItem { get; }

        public MenuItem<KeyBind> ComboKeyItem { get; }

        public MenuItem<StringList> OrbwalkerItem { get; }

        public MenuItem<bool> FullFreeModeItem { get; }

        public MenuItem<StringList> TargetItem { get; }

        public MenuItem<AbilityToggler> LinkenBreakerToggler { get; }

        public MenuItem<PriorityChanger> LinkenBreakerChanger { get; }

        public MenuItem<AbilityToggler> AntiMageBreakerToggler { get; }

        public MenuItem<PriorityChanger> AntiMageBreakerChanger { get; }

        public MenuItem<bool> UseOnlyFromRangeItem { get; }

        public MenuItem<bool> BladeMailItem { get; }

        public MenuItem<StringList> TargetEffectTypeItem { get; }

        public MenuItem<bool> DrawTargetItem { get; }

        public MenuItem<Slider> TargetRedItem { get; }

        public MenuItem<Slider> TargetGreenItem { get; }

        public MenuItem<Slider> TargetBlueItem { get; }

        public MenuItem<bool> DrawOffTargetItem { get; }

        public MenuItem<Slider> OffTargetRedItem { get; }

        public MenuItem<Slider> OffTargetGreenItem { get; }

        public MenuItem<Slider> OffTargetBlueItem { get; }

        public MenuItem<bool> OverwhelmingOddsRadiusItem { get; }

        public MenuItem<bool> PressTheAttackRadiusItem { get; }

        public MenuItem<bool> DuelRadiusItem { get; }

        public MenuItem<bool> BlinkRadiusItem { get; }

        public MenuManager(Config config)
        {
            Factory = MenuFactory.CreateWithTexture("LegionCommanderPlus", HeroId.npc_dota_hero_legion_commander.ToString());
            Factory.Target.SetFontColor(Color.Aqua);

            var comboMenu = Factory.Menu("Combo");
            var abilitiesMenu = comboMenu.Menu("Abilities");
            AbilityToggler = abilitiesMenu.Item("Use: ", new AbilityToggler(new Dictionary<string, bool>
            {
                { AbilityId.legion_commander_duel.ToString(), true },
                { AbilityId.legion_commander_press_the_attack.ToString(), true },
                { AbilityId.legion_commander_overwhelming_odds.ToString(), false }
            }));

            var itemsMenu = comboMenu.Menu("Items");
            ItemToggler = itemsMenu.Item("", "items", new AbilityToggler(new Dictionary<string, bool>
            {
                { AbilityId.item_blink.ToString(), true },
                { AbilityId.item_blade_mail.ToString(), true },
                { AbilityId.item_spirit_vessel.ToString(), true },
                { AbilityId.item_urn_of_shadows.ToString(), true },
                { AbilityId.item_satanic.ToString(), true },
                { AbilityId.item_armlet.ToString(), true },
                { AbilityId.item_mjollnir.ToString(), true },
                { AbilityId.item_shivas_guard.ToString(), true },
                { AbilityId.item_dagon_5.ToString(), true },
                { AbilityId.item_veil_of_discord.ToString(), true },
                { AbilityId.item_ethereal_blade.ToString(), true },
                { AbilityId.item_rod_of_atos.ToString(), true },
                { AbilityId.item_nullifier.ToString(), true },
                { AbilityId.item_bloodthorn.ToString(), true },
                { AbilityId.item_orchid.ToString(), true },
                { AbilityId.item_sheepstick.ToString(), true },
                { AbilityId.item_abyssal_blade.ToString(), false },
                { AbilityId.item_black_king_bar.ToString(), true }
            }));

            var comboBreakerMenu = comboMenu.MenuWithTexture("Combo Breaker", "item_combo_breaker");
            ComboBreakerItem = comboBreakerMenu.Item("Cancel Important Items and Abilities", true);
            ComboBreakerItem.Item.SetTooltip("If Combo Breaker is ready then it will not use Important Items and Abilities");

            ComboKeyItem = comboMenu.Item("Combo Key", new KeyBind('D'));
            OrbwalkerItem = comboMenu.Item("Orbwalker", new StringList("Default", "Free", "Only Attack", "No Move"));
            FullFreeModeItem = comboMenu.Item("Full Free Mode", false);
            TargetItem = comboMenu.Item("Target", new StringList("Lock", "Default"));

            var linkenBreakerMenu = Factory.MenuWithTexture("Linken Breaker", "item_sphere");
            linkenBreakerMenu.Target.AddItem(new MenuItem("linkensphere", "Linkens Sphere:"));
            LinkenBreakerToggler = linkenBreakerMenu.Item("Use: ", "linkentoggler", new AbilityToggler(new Dictionary<string, bool>
            {
                { AbilityId.item_abyssal_blade.ToString(), true },
                { AbilityId.item_sheepstick.ToString(), true},
                { AbilityId.item_rod_of_atos.ToString(), true},
                { AbilityId.item_nullifier.ToString(), true },
                { AbilityId.item_bloodthorn.ToString(), true },
                { AbilityId.item_orchid.ToString(), true },
                { AbilityId.item_cyclone.ToString(), true },
                { AbilityId.item_heavens_halberd.ToString(), true },
                { AbilityId.item_force_staff.ToString(), true }
            }));

            LinkenBreakerChanger = linkenBreakerMenu.Item("Priority: ", "linkenchanger", new PriorityChanger(new List<string>
            {
                { AbilityId.item_abyssal_blade.ToString() },
                { AbilityId.item_sheepstick.ToString() },
                { AbilityId.item_rod_of_atos.ToString() },
                { AbilityId.item_nullifier.ToString() },
                { AbilityId.item_bloodthorn.ToString() },
                { AbilityId.item_orchid.ToString() },
                { AbilityId.item_cyclone.ToString() },
                { AbilityId.item_heavens_halberd.ToString() },
                { AbilityId.item_force_staff.ToString() }
            }));

            linkenBreakerMenu.Target.AddItem(new MenuItem("empty", ""));

            linkenBreakerMenu.Target.AddItem(new MenuItem("antimagespellshield", "Anti Mage Spell Shield:"));
            AntiMageBreakerToggler = linkenBreakerMenu.Item("Use: ", "antimagetoggler", new AbilityToggler(new Dictionary<string, bool>
            {
                { AbilityId.item_rod_of_atos.ToString(), true},
                { AbilityId.item_cyclone.ToString(), true },
                { AbilityId.item_force_staff.ToString(), true }
            }));

            AntiMageBreakerChanger = linkenBreakerMenu.Item("Priority: ", "antimagechanger", new PriorityChanger(new List<string>
            {
                { AbilityId.item_rod_of_atos.ToString() },
                { AbilityId.item_cyclone.ToString() },
                { AbilityId.item_force_staff.ToString() }
            }));

            UseOnlyFromRangeItem = linkenBreakerMenu.Item("Use Only From Range", false);
            UseOnlyFromRangeItem.Item.SetTooltip("Use only from the Range and do not use another Ability");

            var bladeMailMenu = Factory.MenuWithTexture("Blade Mail", "item_blade_mail");
            BladeMailItem = bladeMailMenu.Item("Cancel Combo", false);
            BladeMailItem.Item.SetTooltip("Cancel Combo if there is enemy Blade Mail");

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

            var radiusMenu = drawingMenu.Menu("Radius");
            OverwhelmingOddsRadiusItem = radiusMenu.Item("Overwhelming Odds", true);
            PressTheAttackRadiusItem = radiusMenu.Item("Press The Attack", true);
            DuelRadiusItem = radiusMenu.Item("Duel", false);
            BlinkRadiusItem = radiusMenu.Item("Blink", true);

            OrbwalkerItem.PropertyChanged += Changed;
            DrawTargetItem.PropertyChanged += Changed;
            DrawOffTargetItem.PropertyChanged += Changed;

            Changed(null, null);
        }

        private void Changed(object sender, PropertyChangedEventArgs e)
        {
            // Orbwalker Free
            if (OrbwalkerItem.Value.SelectedValue.Contains("Free"))
            {
                FullFreeModeItem.Item.ShowItem = true;
            }
            else
            {
                FullFreeModeItem.Item.ShowItem = false;
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
            DrawOffTargetItem.PropertyChanged -= Changed;
            DrawTargetItem.PropertyChanged -= Changed;
            OrbwalkerItem.PropertyChanged -= Changed;

            Factory.Dispose();
        }
    }
}

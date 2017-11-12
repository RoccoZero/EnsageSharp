using System;

using Ensage;
using Ensage.SDK.Extensions;

using SharpDX;

namespace SkywrathMagePlus
{
    internal class Renderer
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private int AlarmNumber { get; set; }

        public Renderer(Config config)
        {
            Config = config;
            Menu = config.Menu;

            Drawing.OnDraw += OnDraw;
        }

        public void Dispose()
        {
            Drawing.OnDraw -= OnDraw;
        }

        private void Text(string text, Vector2 pos, Color color)
        {
            Drawing.DrawText(text, "Arial", pos, new Vector2(21), color, FontFlags.None);
        }

        private void Texture(Vector2 pos, Vector2 size, string texture)
        {
            Drawing.DrawRect(
                pos,
                size,
                Drawing.GetTexture($"materials/ensage_ui/{ texture }.vmat"));
        }

        private void OnDraw(EventArgs args)
        {
            if (Menu.TextItem)
            {
                var setPosText = new Vector2(
                    (Config.Screen.X) - Menu.TextXItem - 10,
                    Menu.TextYItem - 20);

                var posText = new Vector2(Config.Screen.X, Config.Screen.Y * 0.65f) - setPosText;

                Text($"Combo { (Menu.ComboKeyItem ? "ON" : "OFF") }", posText, Menu.ComboKeyItem ? Color.Aqua : Color.Yellow);
                Text($"Spam Q { (Menu.SpamArcaneBoltKeyItem ? "ON" : "OFF") }", posText + new Vector2(0, 30), Menu.SpamArcaneBoltKeyItem ? Color.Aqua : Color.Yellow);
                Text($"Auto Q { (!Menu.ComboKeyItem && !Menu.SpamArcaneBoltKeyItem && Menu.AutoArcaneBoltKeyItem ? "ON" : "OFF") }",
                    posText + new Vector2(0, 60),
                    !Menu.ComboKeyItem && !Menu.SpamArcaneBoltKeyItem && Menu.AutoArcaneBoltKeyItem ? Color.Aqua : Color.Yellow);

                var i = 0;
                if (Menu.AutoComboItem)
                {
                    i += 30;
                    Text($"Auto Combo { (!Menu.ComboKeyItem ? "ON" : "OFF") }", posText + new Vector2(0, 60 + i), !Menu.ComboKeyItem ? Color.Aqua : Color.Yellow);
                }

                if (Menu.AutoDisableItem)
                {
                    i += 30;
                    Text("Auto Disable ON", posText + new Vector2(0, 60 + i), Color.Aqua);
                }

                i += 30;
                Text($"Start Combo Mute { (Menu.StartComboKeyItem ? "ON" : "OFF") }", posText + new Vector2(0, 60 + i), Menu.StartComboKeyItem ? Color.Aqua : Color.Yellow);
            }
            
            if (Menu.CalculationItem)
            {
                var setPosTexture = new Vector2(
                    Config.Screen.X - Menu.CalculationXItem - 20,
                    Menu.CalculationYItem - 110);

                var x = 0;
                foreach (var Data in Config.DamageCalculation.DamageList)
                {
                    var posTexture = new Vector2(Config.Screen.X, Config.Screen.Y * 0.65f + x) - setPosTexture;

                    var hero = Data.GetTarget;
                    var health = Data.GetHealth;

                    var ph = Math.Ceiling((float)health / hero.MaximumHealth * 100);
                    var doNotKill = DoNotKill(hero);

                    if (!hero.IsVisible)
                    {
                        Texture(posTexture + 5, new Vector2(55), $"heroes_round/{ hero.Name.Substring("npc_dota_hero_".Length) }");
                        Texture(posTexture, new Vector2(65), "other/round_percentage/frame/white");
                        Texture(posTexture, new Vector2(65), $"other/round_percentage/hp/{ Math.Min(ph, 100) }");

                        if (doNotKill != null)
                        {
                            Texture(posTexture + new Vector2(42, 45), new Vector2(20), $"modifier_textures/round/{ doNotKill }");
                        }

                        x += 80;
                        continue;
                    }

                    var damage = Data.GetDamage;
                    var readyDamage = Data.GetReadyDamage;
                    var totalDamage = Data.GetTotalDamage;

                    var maxHealth = hero.MaximumHealth + (health - hero.MaximumHealth);
                    var damagePercent = Math.Ceiling(100 - (health - Math.Max(damage, 0)) / maxHealth * 100);
                    var readyDamagePercent = Math.Ceiling(100 - (health - Math.Max(readyDamage, 0)) / maxHealth * 100);
                    var totalDamagePercent = Math.Ceiling(100 - (health - Math.Max(totalDamage, 0)) / maxHealth * 100);

                    if (damagePercent >= 100)
                    {
                        Texture(posTexture - 10, new Vector2(85), $"other/round_percentage/alert/{ Alert() }");
                    }

                    Texture(posTexture + 5, new Vector2(55), $"heroes_round/{ hero.Name.Substring("npc_dota_hero_".Length) }");

                    Texture(posTexture, new Vector2(65), "other/round_percentage/frame/white");
                    Texture(posTexture, new Vector2(65), $"other/round_percentage/no_percent_gray/{ Math.Min(totalDamagePercent, 100) }");
                    Texture(posTexture, new Vector2(65), $"other/round_percentage/no_percent_yellow/{ Math.Min(readyDamagePercent, 100) }");

                    var color = damagePercent >= 100 ? "green" : "red";
                    Texture(posTexture, new Vector2(65), $"other/round_percentage/{ color }/{ Math.Min(damagePercent, 100) }");

                    if (damagePercent >= 100)
                    {
                        Texture(posTexture, new Vector2(65), $"other/round_percentage/no_percent_gray/{ Math.Min(damagePercent - 100, 100) }");
                    }

                    if (doNotKill != null)
                    {
                        Texture(posTexture + new Vector2(42, 45), new Vector2(20), $"modifier_textures/round/{ doNotKill }");
                    }

                    x += 80;
                }
            }
        }

        private string DoNotKill(Hero hero)
        {
            var reincarnation = hero.GetAbilityById(AbilityId.skeleton_king_reincarnation);
            if (reincarnation != null && reincarnation.Cooldown == 0 && reincarnation.Level > 0)
            {
                return reincarnation.TextureName;
            }


            return null;
        }

        private string Alert()
        {
            AlarmNumber += 1;
            if (AlarmNumber < 10)
            {
                return 0.ToString();
            }
            else if (AlarmNumber < 20)
            {
                return 1.ToString();
            }
            else if (AlarmNumber < 30)
            {
                return 2.ToString();
            }
            else if (AlarmNumber < 40)
            {
                return 1.ToString();
            }
            else
            {
                AlarmNumber = 0;
            }

            return 0.ToString();
        }
    }
}

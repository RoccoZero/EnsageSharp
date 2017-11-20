using System;

using Ensage;
using Ensage.Common;
using Ensage.SDK.Extensions;

using SharpDX;

namespace SkywrathMagePlus
{
    internal class Renderer
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private Unit Owner { get; }

        private int AlarmNumber { get; set; }

        public Renderer(Config config)
        {
            Config = config;
            Menu = config.Menu;
            Owner = config.Main.Context.Owner;

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
            Drawing.DrawRect(pos, size, Drawing.GetTexture($"materials/ensage_ui/{ texture }.vmat"));
        }

        private void OnDraw(EventArgs args)
        {
            if (Menu.TextItem)
            {
                var setPosText = new Vector2(Config.Screen.X - Menu.TextXItem - 10, Menu.TextYItem - 70);
                var posText = new Vector2(Config.Screen.X, Config.Screen.Y * 0.65f) - setPosText;

                var combo = Menu.ComboKeyItem;
                Text($"Combo { (combo ? "ON" : "OFF") }", posText, combo ? Color.Aqua : Color.Yellow);

                var startCombo = Menu.StartComboKeyItem;
                Text($"Start Mute { (startCombo ? "ON" : "OFF") }", posText + new Vector2(0, 30), startCombo ? Color.Aqua : Color.Yellow);

                var ownerHealth = ((float)Owner.Health / Owner.MaximumHealth) * 100;
                var autoArcaneBolt = !combo && !Menu.SpamArcaneBoltKeyItem && Menu.AutoArcaneBoltKeyItem && Menu.AutoArcaneBoltOwnerMinHealthItem <= ownerHealth;
                Text($"Auto Q { (autoArcaneBolt ? "ON" : "OFF") }", posText + new Vector2(0, 60), autoArcaneBolt ? Color.Aqua : Color.Yellow);

                if (Menu.AutoComboItem)
                {
                    var autoCombo = !Menu.AutoComboWhenComboItem || !combo && Menu.AutoOwnerMinHealthItem <= ownerHealth;
                    Text($"Auto Combo { (autoCombo ? "ON" : "OFF") }", posText + new Vector2(0, 90), autoCombo ? Color.Aqua : Color.Yellow);
                }
            }

            var i = 0;
            foreach (var data in Config.DamageCalculation.DamageList)
            {
                var target = data.GetTarget;
                var health = data.GetHealth;
                var damage = data.GetDamage;
                var readyDamage = data.GetReadyDamage;

                if (Menu.HPBarCalculationItem)
                {
                    var hpBarPosition = HUDInfo.GetHPbarPosition(target) ;
                    if (!hpBarPosition.IsZero)
                    {
                        var hpBarSizeX = HUDInfo.GetHPBarSizeX(target);
                        var hpBarSizeY = HUDInfo.GetHpBarSizeY(target) / 1.7f;
                        var hpBarPos = hpBarPosition + new Vector2(0, hpBarSizeY * (Menu.HPBarCalculationPosItem / 70f));

                        var readyDamageBar = Math.Max(readyDamage, 0) / target.MaximumHealth;
                        if (readyDamageBar > 0)
                        {
                            var readyDamagePos = Math.Max(health - readyDamage, 0) / target.MaximumHealth;
                            var readyDamagePosition = new Vector2(hpBarPos.X + ((hpBarSizeX + readyDamageBar) * readyDamagePos), hpBarPos.Y);
                            var readyDamageSize = new Vector2(hpBarSizeX * (readyDamageBar + Math.Min(health - readyDamage, 0) / target.MaximumHealth), hpBarSizeY);
                            var readyDamageColor = ((float)health / target.MaximumHealth) - readyDamageBar > 0 ? new Color(100, 0, 0, 200) : new Color(191, 255, 0, 200);

                            Drawing.DrawRect(readyDamagePosition, readyDamageSize, readyDamageColor);
                            Drawing.DrawRect(readyDamagePosition, readyDamageSize, Color.Black, true);
                        }

                        var damageBar = Math.Max(damage, 0) / target.MaximumHealth;
                        if (damageBar > 0)
                        {
                            var damagePos = Math.Max(health - damage, 0) / target.MaximumHealth;
                            var damagePosition = new Vector2(hpBarPos.X + ((hpBarSizeX + damageBar) * damagePos), hpBarPos.Y);
                            var damageSize = new Vector2(hpBarSizeX * (damageBar + Math.Min(health - damage, 0) / target.MaximumHealth), hpBarSizeY);
                            var damageColor = ((float)health / target.MaximumHealth) - damageBar > 0 ? new Color(0, 255, 0) : Color.Aqua;

                            Drawing.DrawRect(damagePosition, damageSize, damageColor);
                            Drawing.DrawRect(damagePosition, damageSize, Color.Black, true);
                        }
                    }
                }

                if (Menu.CalculationItem)
                {
                    var setPosTexture = new Vector2(Config.Screen.X - Menu.CalculationXItem - 20, Menu.CalculationYItem - 110);
                    var posTexture = new Vector2(Config.Screen.X, Config.Screen.Y * 0.65f + i) - setPosTexture;
                    var doNotKill = DoNotKill(target);

                    Texture(posTexture + 5, new Vector2(55), $"heroes_round/{ target.Name.Substring("npc_dota_hero_".Length) }");
                    Texture(posTexture, new Vector2(65), "other/round_percentage/frame/white");

                    if (!target.IsVisible)
                    {
                        var hp = Math.Ceiling((float)health / target.MaximumHealth * 100);
                        Texture(posTexture, new Vector2(65), $"other/round_percentage/hp/{ Math.Min(hp, 100) }");

                        if (doNotKill != null)
                        {
                            Texture(posTexture + new Vector2(42, 45), new Vector2(20), $"modifier_textures/round/{ doNotKill }");
                        }

                        i += 80;
                        continue;
                    }

                    var totalDamage = data.GetTotalDamage;

                    var maxHealth = target.MaximumHealth + (health - target.MaximumHealth);
                    var damagePercent = Math.Ceiling(100 - (health - Math.Max(damage, 0)) / maxHealth * 100);
                    var readyDamagePercent = Math.Ceiling(100 - (health - Math.Max(readyDamage, 0)) / maxHealth * 100);
                    var totalDamagePercent = Math.Ceiling(100 - (health - Math.Max(totalDamage, 0)) / maxHealth * 100);

                    if (damagePercent >= 100)
                    {
                        Texture(posTexture - 10, new Vector2(85), $"other/round_percentage/alert/{ Alert() }");
                    }

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

                    i += 80;
                }
            }
        }

        private string DoNotKill(Hero target)
        {
            var comboBreaker = target.GetItemById(AbilityId.item_combo_breaker);
            if (comboBreaker != null && comboBreaker.Cooldown <= 0)
            {
                return comboBreaker.TextureName;
            }

            var reincarnation = target.GetAbilityById(AbilityId.skeleton_king_reincarnation);
            if (reincarnation != null && reincarnation.Cooldown <= 0 && reincarnation.Level > 0)
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

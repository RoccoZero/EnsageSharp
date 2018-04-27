using System;

using Ensage;
using Ensage.Common;

using SharpDX;

namespace SkywrathMagePlus
{
    internal class Renderer
    {
        private Config Config { get; }

        private MenuManager Menu { get; }

        private Unit Owner { get; }

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

            if (Menu.HPBarCalculationItem)
            {
                foreach (var data in Config.DamageCalculation.DamageList)
                {
                    var target = data.GetTarget;
                    var hpBarPosition = HUDInfo.GetHPbarPosition(target);
                    if (!hpBarPosition.IsZero)
                    {
                        var hpBarSizeX = HUDInfo.GetHPBarSizeX(target);
                        var hpBarSizeY = HUDInfo.GetHpBarSizeY(target) / 1.7f;
                        var hpBarPos = hpBarPosition + new Vector2(0, hpBarSizeY * (Menu.HPBarCalculationPosItem / 70f));

                        var health = data.GetHealth;
                        var readyDamage = data.GetReadyDamage;
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

                        var damage = data.GetDamage;
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
            }
        }
    }
}

using System;
using System.Windows.Input;

using Ensage;
using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;

using SharpDX;

using EnchantressPlus.Features;

namespace EnchantressPlus
{
    internal class Config : IDisposable
    {
        public EnchantressPlus Main { get; }

        public Vector2 Screen { get; }

        public MultiSleeper MultiSleeper { get; }

        public MenuManager Menu { get; }

        public UpdateMode UpdateMode { get; }

        public DamageCalculation DamageCalculation { get; }

        private AutoAbility AutoAbility { get; }

        public LinkenBreaker LinkenBreaker { get; }

        private AutoKillSteal AutoKillSteal { get; }

        private Mode Mode { get; }

        private Renderer Renderer { get; }

        private bool Disposed { get; set; }

        public Config(EnchantressPlus main)
        {
            Main = main;
            Screen = new Vector2(Drawing.Width - 160, Drawing.Height);
            MultiSleeper = new MultiSleeper();

            Menu = new MenuManager(this);
            
            UpdateMode = new UpdateMode(this);
            DamageCalculation = new DamageCalculation(this);
            LinkenBreaker = new LinkenBreaker(this);
            AutoKillSteal = new AutoKillSteal(this);
            AutoAbility = new AutoAbility(this);
            
            Menu.ComboKeyItem.Item.ValueChanged += ComboKeyChanged;
            var Key = KeyInterop.KeyFromVirtualKey((int)Menu.ComboKeyItem.Value.Key);
            Mode = new Mode(Main.Context, Key, this);
            Main.Context.Orbwalker.RegisterMode(Mode);

            Renderer = new Renderer(this);
        }

        private void ComboKeyChanged(object sender, OnValueChangeEventArgs e)
        {
            var keyCode = e.GetNewValue<KeyBind>().Key;

            if (keyCode == e.GetOldValue<KeyBind>().Key)
            {
                return;
            }

            var key = KeyInterop.KeyFromVirtualKey((int)keyCode);
            Mode.Key = key;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                Renderer.Dispose();
                Main.Context.Orbwalker.UnregisterMode(Mode);
                Menu.ComboKeyItem.Item.ValueChanged -= ComboKeyChanged;
                AutoAbility.Dispose();
                AutoKillSteal.Dispose();
                DamageCalculation.Dispose();
                UpdateMode.Dispose();
                Main.Context.Particle.Dispose();
                Menu.Dispose();
            }

            Disposed = true;
        }
    }
}

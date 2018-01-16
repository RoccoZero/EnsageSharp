using System;
using System.Windows.Input;

using Ensage;
using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;

using SharpDX;

using LegionCommanderPlus.Features;

namespace LegionCommanderPlus
{
    internal class Config : IDisposable
    {
        public LegionCommanderPlus Main { get; }

        public Vector2 Screen { get; }

        public MultiSleeper MultiSleeper { get; }

        public MenuManager Menu { get; }

        public Extensions Extensions { get; }

        public UpdateMode UpdateMode { get; }

        public LinkenBreaker LinkenBreaker { get; }

        private Mode Mode { get; }

        private bool Disposed { get; set; }

        public Config(LegionCommanderPlus main)
        {
            Main = main;

            ActivatePlugins();
            Screen = new Vector2(Drawing.Width - 160, Drawing.Height);
            MultiSleeper = new MultiSleeper();

            Menu = new MenuManager(this);
            Extensions = new Extensions(this);

            UpdateMode = new UpdateMode(this);
            LinkenBreaker = new LinkenBreaker(this);

            Menu.ComboKeyItem.Item.ValueChanged += ComboKeyChanged;
            var key = KeyInterop.KeyFromVirtualKey((int)Menu.ComboKeyItem.Value.Key);
            Mode = new Mode(key, this);
            main.Context.Orbwalker.RegisterMode(Mode);
        }

        private void ActivatePlugins()
        {
            var orbwalker = Main.Context.Orbwalker;
            if (!orbwalker.IsActive)
            {
                orbwalker.Activate();
            }

            var targetSelector = Main.Context.TargetSelector;
            if (!targetSelector.IsActive)
            {
                targetSelector.Activate();
            }

            var prediction = Main.Context.Prediction;
            if (!prediction.IsActive)
            {
                prediction.Activate();
            }
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
                Main.Context.Orbwalker.UnregisterMode(Mode);
                Menu.ComboKeyItem.Item.ValueChanged -= ComboKeyChanged;

                UpdateMode.Dispose();

                Main.Context.Particle.Dispose();

                Menu.Dispose();
            }

            Disposed = true;
        }
    }
}

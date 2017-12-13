using System;

using Ensage.Common.Menu;
using Ensage.SDK.Menu;

using SharpDX;

namespace JungleScanPlus
{
    internal class Config : IDisposable
    {
        private MenuFactory Factory { get; }

        public MenuItem<bool> DrawWorldItem { get; }

        public MenuItem<Slider> RedItem { get; }

        public MenuItem<Slider> GreenItem { get; }

        public MenuItem<Slider> BlueItem { get; }


        public MenuItem<Slider> TimerItem { get; }

        private bool Disposed { get; set; }

        public Config()
        {
            Factory = MenuFactory.CreateWithTexture("JungleScanPlus", "junglescanplus");
            Factory.Target.SetFontColor(Color.Aqua);

            DrawWorldItem = Factory.Item("Draw On World", true);

            RedItem = Factory.Item("Red", new Slider(0, 0, 255));
            RedItem.Item.SetFontColor(Color.Red);

            GreenItem = Factory.Item("Green", new Slider(255, 0, 255));
            GreenItem.Item.SetFontColor(Color.Green);

            BlueItem = Factory.Item("Blue", new Slider(255, 0, 255));
            BlueItem.Item.SetFontColor(Color.Blue);

            TimerItem = Factory.Item("Timer", new Slider(6, 1, 9));
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
                Factory.Dispose();
            }

            Disposed = true;
        }
    }
}

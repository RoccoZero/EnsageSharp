using System.ComponentModel;

using Ensage;

namespace SkywrathMagePlus.Features
{
    internal class WithoutFail
    {
        private MenuManager Menu { get; }

        private SkywrathMagePlus Main { get; }

        public WithoutFail(Config config)
        {
            Menu = config.Menu;
            Main = config.Main;

            if (config.Menu.ConcussiveShotWithoutFailItem)
            {
                Player.OnExecuteOrder += OnExecuteOrder;
            }

            config.Menu.ConcussiveShotWithoutFailItem.PropertyChanged += ConcussiveShotWithoutFailChanged;
        }

        public void Dispose()
        {
            Menu.ConcussiveShotWithoutFailItem.PropertyChanged -= ConcussiveShotWithoutFailChanged;

            if (Menu.ConcussiveShotWithoutFailItem)
            {
                Player.OnExecuteOrder -= OnExecuteOrder;
            }
        }

        private void ConcussiveShotWithoutFailChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Menu.ConcussiveShotWithoutFailItem)
            {
                Player.OnExecuteOrder += OnExecuteOrder;
            }
            else
            {
                Player.OnExecuteOrder -= OnExecuteOrder;
            }
        }

        private void OnExecuteOrder(Player sender, ExecuteOrderEventArgs args)
        {
            var concussiveShot = Main.ConcussiveShot;
            if (args.OrderId == OrderId.Ability && args.Ability.Name == concussiveShot.ToString() && concussiveShot.TargetHit == null)
            {
                args.Process = false;
                Game.PrintMessage($"<font color='#FF6666'>There is no one in the radius.</font>");
            }
        }
    }
}

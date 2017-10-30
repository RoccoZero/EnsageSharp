using System.ComponentModel;

using Ensage;

namespace SkywrathMagePlus.Features
{
    internal class WithoutFail
    {
        private MenuManager Menu { get; }

        private UpdateMode UpdateMode { get; }

        public WithoutFail(Config config)
        {
            Menu = config.Menu;
            UpdateMode = config.UpdateMode;

            if (config.Menu.WWithoutFailItem)
            {
                Player.OnExecuteOrder += OnExecuteOrder;
            }

            config.Menu.WWithoutFailItem.PropertyChanged += WWithoutFailChanged;
        }

        public void Dispose()
        {
            Menu.WWithoutFailItem.PropertyChanged -= WWithoutFailChanged;

            if (Menu.WWithoutFailItem)
            {
                Player.OnExecuteOrder -= OnExecuteOrder;
            }
        }

        private void WWithoutFailChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Menu.WWithoutFailItem)
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
            if (args.OrderId == OrderId.Ability 
                && args.Ability.Name == "skywrath_mage_concussive_shot" 
                && UpdateMode.WShowTarget == null)
            {
                args.Process = false;
                Game.PrintMessage($"<font color='#FF6666'>There is no one in the radius.</font>");
            }
        }
    }
}

using Ensage;

namespace AntiOrder
{
    internal sealed class AntiOrder
    {
        public static void Main()
        {
            Player.OnExecuteOrder += OnExecuteOrder;
        }

        private static void OnExecuteOrder(Player sender, ExecuteOrderEventArgs args)
        {
            if (args.IsPlayerInput)
            {
                return;
            }

            args.Process = false;
        }
    }
}
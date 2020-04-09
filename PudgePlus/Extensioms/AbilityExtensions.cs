using System.Linq;

using Ensage;

using NLog;

namespace PudgePlus.Extensioms
{
    internal static class AbilityExtensions
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static float GetSpecialData(this Ability ability, string name, uint level = 0)
        {
            var data = ability.AbilitySpecialData.FirstOrDefault(x => x.Name == name);
            if (data == null)
            {
                Log.Error($"BrokenAbilitySpecialData => Ability: {ability.Name}, SpecialData: {name}");
                return 0;
            }

            if (data.Count == 1)
            {
                return data.Value;
            }

            if (level == 0)
            {
                level = ability.Level;
            }

            if (level == 0)
            {
                return 0;
            }

            return data.GetValue(level - 1);
        }
    }
}

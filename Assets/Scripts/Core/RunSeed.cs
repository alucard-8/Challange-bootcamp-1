using UnityEngine;

namespace Soulbreak.Core
{
    public static class RunSeed
    {
        public static int Value { get; private set; }

        public static bool IsFixed { get; private set; }

        public static void Begin()
        {
            int fixedSeed = GameSettings.Seed;
            IsFixed = fixedSeed != 0;
            Value = IsFixed ? fixedSeed : Random.Range(1, int.MaxValue);
            Random.InitState(Value);
        }
    }
}

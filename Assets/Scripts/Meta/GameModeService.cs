namespace Soulbreak.Meta
{
    // Answers "which mode are we playing?" (the selected, unlocked mode or the default) and
    // "what is this thing called in this mode?" (lexicon lookups for UI text).
    public static class GameModeService
    {
        public const string DefaultMode = "reactor";
        public const string StarMode = "star";

        private static ModeLibrary library;

        public static ModeLibrary Library
        {
            get
            {
                if (library == null) library = UnityEngine.Resources.Load<ModeLibrary>("ModeLibrary");
                return library;
            }
        }

        public static ModeProfile Current
        {
            get
            {
                var lib = Library;
                if (lib == null || lib.modes == null || lib.modes.Length == 0) return null;
                return lib.Find(MetaProgress.SelectedMode) ?? lib.Find(DefaultMode) ?? lib.modes[0];
            }
        }

        public static bool IsStar => Current != null && Current.id == StarMode;

        public static string Text(string key, string fallback)
        {
            var t = Current?.Lookup(key);
            return string.IsNullOrEmpty(t) ? fallback : t;
        }
    }
}

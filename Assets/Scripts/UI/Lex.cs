namespace Soulbreak.UI
{
    // Short alias for mode-specific wording in views ("CORE" vs "STAR", "SOULS" vs "STARDUST"...).
    public static class Lex
    {
        public static string T(string key, string fallback) => Meta.GameModeService.Text(key, fallback);
    }
}

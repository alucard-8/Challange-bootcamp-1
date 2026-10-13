namespace Soulbreak.UI
{
    public static class Lex
    {
        public static string T(string key, string fallback) => Meta.GameModeService.Text(key, fallback);
    }
}

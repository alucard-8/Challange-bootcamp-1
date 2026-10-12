using UnityEngine;

namespace Soulbreak.UI
{
    // Replaces the OS cursor with a small crosshair: four thin ticks around an open center + a tiny dot,
    // so the target under it stays visible. Hardware cursor = no lag, works in every scene and menu.
    public static class CursorStyle
    {
        private const int Size = 32;
        private static Texture2D texture;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Apply()
        {
            if (texture == null) texture = Build();
            Cursor.SetCursor(texture, new Vector2(Size / 2f, Size / 2f), CursorMode.Auto);
        }

        private static Texture2D Build()
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[Size * Size];
            var core = new Color32(210, 245, 255, 255);
            var edge = new Color32(10, 5, 25, 200);

            const int c = Size / 2, gap = 4, length = 7, half = 1;   // ticks are 2px thick

            // Outline pass first (1px bigger), then the bright core on top.
            for (int pass = 0; pass < 2; pass++)
            {
                int grow = pass == 0 ? 1 : 0;
                var col = pass == 0 ? edge : core;

                for (int d = gap - grow; d <= gap + length + grow; d++)
                for (int t = -half - grow; t < half + grow; t++)
                {
                    Set(px, c + d, c + t, col);   // right
                    Set(px, c - d - 1, c + t, col);   // left
                    Set(px, c + t, c + d, col);   // up
                    Set(px, c + t, c - d - 1, col);   // down
                }

                for (int x = -1 - grow; x < 1 + grow; x++)
                for (int y = -1 - grow; y < 1 + grow; y++)
                    Set(px, c + x, c + y, col);   // center dot
            }

            tex.SetPixels32(px);
            tex.Apply();
            return tex;
        }

        private static void Set(Color32[] px, int x, int y, Color32 col)
        {
            if (x < 0 || y < 0 || x >= Size || y >= Size) return;
            px[y * Size + x] = col;
        }
    }
}

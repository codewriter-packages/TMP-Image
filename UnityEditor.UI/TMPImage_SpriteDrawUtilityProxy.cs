using UnityEngine;

namespace UnityEditor.UI
{
    public static class TMPImage_SpriteDrawUtilityProxy
    {
        public static void DrawSprite(Texture tex, Rect drawArea, Rect outer, Rect uv, Color color)
        {
            SpriteDrawUtility.DrawSprite(tex, drawArea, outer, uv, color);
        }
    }
}
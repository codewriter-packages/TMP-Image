namespace TMPro {
    using System.Collections.Generic;

    public static class TMPImage_TMP_SpriteAssetProxy {
        public static Dictionary<uint, int> GetGlyphIndexLookup(TMP_SpriteAsset spriteAsset) {
            if (spriteAsset.m_GlyphIndexLookup == null) {
                spriteAsset.UpdateLookupTables();
            }

            return spriteAsset.m_GlyphIndexLookup;
        }
    }
}
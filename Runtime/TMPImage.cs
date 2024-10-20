using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.TextCore;
using UnityEngine.UI;

namespace CodeWriter.UI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("UI/TMP Image", 13)]
    public class TMPImage : MaskableGraphic
    {
        [SerializeField] private string m_SpriteName = string.Empty;
        [SerializeField] private bool m_PreserveAspect = false;

        [NonSerialized] private TMP_SpriteAsset m_CurrentSpriteAsset;

        [Preserve]
        public TMPImage()
        {
            useLegacyMeshGeneration = false;
        }

        [PublicAPI]
        public string spriteName
        {
            get => m_SpriteName;
            set
            {
                if (m_SpriteName == value)
                {
                    return;
                }

                m_SpriteName = value ?? string.Empty;

                SetVerticesDirty();
                SetMaterialDirty();
            }
        }

        public bool preserveAspect
        {
            get => m_PreserveAspect;
            set
            {
                if (m_PreserveAspect == value)
                {
                    return;
                }

                m_PreserveAspect = value;

                SetVerticesDirty();
            }
        }

        public Rect uvRect
        {
            get
            {
                if (TrySearchAtlasWithUv(ref m_CurrentSpriteAsset, spriteName, out _, out var uv))
                {
                    return uv;
                }

                return new Rect(0, 0, 1, 1);
            }
        }

        public override Texture mainTexture
        {
            get
            {
                if (TrySearchAtlasWithUv(ref m_CurrentSpriteAsset, spriteName, out var texture, out _))
                {
                    return texture;
                }

                return s_WhiteTexture;
            }
        }

        public override void SetNativeSize()
        {
            var tex = mainTexture;
            if (tex != null && tex != s_WhiteTexture)
            {
                var w = Mathf.RoundToInt(tex.width * uvRect.width);
                var h = Mathf.RoundToInt(tex.height * uvRect.height);
                rectTransform.anchorMax = rectTransform.anchorMin;
                rectTransform.sizeDelta = new Vector2(w, h);
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (!TrySearchAtlasWithUv(ref m_CurrentSpriteAsset, spriteName, out var texture, out var uv))
            {
                return;
            }

            var r = GetPixelAdjustedRect();

            if (preserveAspect)
            {
                PreserveSpriteAspectRatio(ref r, uv.size);
            }

            var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
            var scaleX = texture.width * texture.texelSize.x;
            var scaleY = texture.height * texture.texelSize.y;
            {
                var color32 = (Color32) color;
                vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(uv.xMin * scaleX, uv.yMin * scaleY));
                vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(uv.xMin * scaleX, uv.yMax * scaleY));
                vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(uv.xMax * scaleX, uv.yMax * scaleY));
                vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(uv.xMax * scaleX, uv.yMin * scaleY));

                vh.AddTriangle(0, 1, 2);
                vh.AddTriangle(2, 3, 0);
            }
        }

        protected override void OnDidApplyAnimationProperties()
        {
            SetMaterialDirty();
            SetVerticesDirty();
            SetRaycastDirty();
        }

        private void PreserveSpriteAspectRatio(ref Rect rect, Vector2 spriteSize)
        {
            var spriteRatio = spriteSize.x / spriteSize.y;
            var rectRatio = rect.width / rect.height;

            if (spriteRatio > rectRatio)
            {
                var oldHeight = rect.height;
                rect.height = rect.width * (1.0f / spriteRatio);
                rect.y += (oldHeight - rect.height) * rectTransform.pivot.y;
            }
            else
            {
                var oldWidth = rect.width;
                rect.width = rect.height * spriteRatio;
                rect.x += (oldWidth - rect.width) * rectTransform.pivot.x;
            }
        }

        private static bool TrySearchAtlasWithUv(ref TMP_SpriteAsset currentSpriteAsset, string spriteName,
            out Texture texture, out Rect uvRect)
        {
            var spriteNameHash = TMP_TextParsingUtilities.GetHashCodeCaseSensitive(spriteName);

            currentSpriteAsset = TMP_SpriteAsset.SearchForSpriteByHashCode(currentSpriteAsset,
                spriteNameHash, true, out var spriteIndex);

            if (spriteIndex == -1)
            {
                currentSpriteAsset = TMP_SpriteAsset.SearchForSpriteByHashCode(TMP_Settings.defaultSpriteAsset,
                    spriteNameHash, true, out spriteIndex);
            }

            if (spriteIndex == -1 || currentSpriteAsset == null || currentSpriteAsset.spriteGlyphTable == null)
            {
                texture = null;
                uvRect = default;
                return false;
            }

            var spriteChar = currentSpriteAsset.spriteCharacterTable[spriteIndex];
            var spriteGlyph = currentSpriteAsset.spriteGlyphTable[(int) spriteChar.glyphIndex];

            texture = currentSpriteAsset.spriteSheet;
            uvRect = CalculateUvRect(spriteGlyph.glyphRect, texture);
            return texture != null;
        }

        private static Rect CalculateUvRect(GlyphRect r, Texture t)
        {
            var s = new Vector2(t.width, t.height);
            return new Rect(r.x / s.x, r.y / s.y, r.width / s.x, r.height / s.y);
        }
    }
}
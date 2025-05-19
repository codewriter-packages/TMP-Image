using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace CodeWriter.UI
{
    [CustomEditor(typeof(TMPImage), true)]
    [CanEditMultipleObjects]
    public class TmpImageEditor : GraphicEditor
    {
        private SerializedProperty m_SpriteName;
        private SerializedProperty m_PreserveAspect;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_SpriteName = serializedObject.FindProperty("m_SpriteName");
            m_PreserveAspect = serializedObject.FindProperty("m_PreserveAspect");

            SetShowNativeSize(true);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_SpriteName);

            AppearanceControlsGUI();
            RaycastControlsGUI();
            MaskableControlsGUI();
            EditorGUILayout.PropertyField(m_PreserveAspect);
            SetShowNativeSize(false);
            NativeSizeButtonGUI();

            serializedObject.ApplyModifiedProperties();
        }

        void SetShowNativeSize(bool instant)
        {
            base.SetShowNativeSize(true, instant);
        }

        private static Rect Outer(TMPImage tmpImage)
        {
            var outer = tmpImage.uvRect;
            outer.xMin *= tmpImage.rectTransform.rect.width;
            outer.xMax *= tmpImage.rectTransform.rect.width;
            outer.yMin *= tmpImage.rectTransform.rect.height;
            outer.yMax *= tmpImage.rectTransform.rect.height;
            return outer;
        }

        public override bool HasPreviewGUI()
        {
            var tmpImage = target as TMPImage;
            if (tmpImage == null)
            {
                return false;
            }

            return true;
        }

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            var tmpImage = target as TMPImage;
            if (tmpImage == null)
            {
                return;
            }

            var tex = tmpImage.mainTexture;
            if (tex == null)
            {
                return;
            }

            var outer = Outer(tmpImage);
            TMPImage_SpriteDrawUtilityProxy.DrawSprite(tex, rect, outer, tmpImage.uvRect,
                tmpImage.canvasRenderer.GetColor());
        }

        public override string GetInfoString()
        {
            var tmpImage = target as TMPImage;
            if (tmpImage == null)
            {
                return string.Empty;
            }

            var text = string.Format("TMPImage Size: {0}x{1}",
                Mathf.RoundToInt(Mathf.Abs(tmpImage.rectTransform.rect.width)),
                Mathf.RoundToInt(Mathf.Abs(tmpImage.rectTransform.rect.height)));

            return text;
        }
    }
}
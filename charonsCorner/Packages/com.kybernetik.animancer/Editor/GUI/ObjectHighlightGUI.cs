// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only]
    /// Allows any object to be highlighted in the GUI like with
    /// <see cref="EditorGUIUtility.PingObject(Object)"/>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/ObjectHighlightGUI
    /// 
    public static class ObjectHighlightGUI
    {
        /************************************************************************************************************************/

        /// <summary>The highlight will start by expanding then contracting over this duration.</summary>
        public const double ExpandDuration = 0.5f;

        /// <summary>After the <see cref="ExpandDuration"/> the highlight will fade out over this duration.</summary>
        public const double LingerDuration = 2;

        /// <summary>The size that the highlight expands to.</summary>
        public const float HighlightSize = 5;

        /// <summary>The colour used to highlight the pinged object.</summary>
        public static readonly Color HighlightColor = EditorGUIUtility.isProSkin
            ? new(0.8f, 0.6f, 0.2f, 0.4f)
            : new(1, 0.75f, 0.25f, 0.4f);

        /// <summary><see cref="EditorApplication.timeSinceStartup"/></summary>
        public static double CurrentTime => EditorApplication.timeSinceStartup;

        /************************************************************************************************************************/

        /// <summary>The object currently being highlighted.</summary>
        public static object Target { get; private set; }

        /// <summary>The time when the highlight was started.</summary>
        public static double StartTime { get; private set; }

        /************************************************************************************************************************/

        /// <summary>Sets the target object to start highlighting it.</summary>
        public static void Highlight(object target)
        {
            if (target is Object unityObject)
            {
                EditorGUIUtility.PingObject(unityObject);
                return;
            }

            Target = target;
            StartTime = CurrentTime;
        }

        /************************************************************************************************************************/

        /// <summary>Draws the highlight if the given `target` is the current <see cref="Target"/>.</summary>
        public static void Draw(Rect area, object target)
        {
            if (Target != target ||
                Event.current.type != EventType.Repaint)
                return;

            var elapsedTime = CurrentTime - StartTime;
            if (elapsedTime < ExpandDuration + LingerDuration)
            {
                if (elapsedTime < ExpandDuration)
                {
                    var size = HighlightSize * Mathf.Sin((float)(elapsedTime / ExpandDuration * Mathf.PI));

                    area.x -= size;
                    area.y -= size;
                    area.width += size * 2;
                    area.height += size * 2;
                }

                elapsedTime /= ExpandDuration + LingerDuration;

                var color = HighlightColor;
                color.a *= (float)(1 - elapsedTime);

                EditorGUI.DrawRect(area, color);

                InternalEditorUtility.RepaintAllViews();
            }
            else
            {
                Target = null;
            }
        }

        /************************************************************************************************************************/
    }
}

#endif


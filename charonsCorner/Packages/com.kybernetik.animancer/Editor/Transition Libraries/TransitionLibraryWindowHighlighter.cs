// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using Animancer.TransitionLibraries;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor.TransitionLibraries
{
    /// <summary>[Editor-Only]
    /// An <see cref="EditorWindow"/> for configuring <see cref="TransitionLibraryAsset"/>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.TransitionLibraries/TransitionLibraryWindowHighlighter
    public class TransitionLibraryWindowHighlighter
    {
        /************************************************************************************************************************/

        private static readonly Color
            SelectionHighlightColor = new(0.5f, 0.5f, 1, 0.1f),
            HoverHighlightColor = new(0.5f, 1, 0.5f, 0.1f);

        /************************************************************************************************************************/

        /// <summary>The current <see cref="Event.type"/>.</summary>
        public EventType EventType { get; private set; }

        /// <summary>The the mouse currently over the highlighter area.</summary>
        public bool IsMouseOver { get; private set; }

        /// <summary>The the hover highlight currently visible.</summary>
        public bool DidHoverHighlight { get; private set; }

        /************************************************************************************************************************/

        /// <summary>Gathers the details of the <see cref="Event.current"/>.</summary>
        public void BeginGUI(Rect area)
        {
            var currentEvent = Event.current;
            EventType = currentEvent.type;
            IsMouseOver = area.Contains(currentEvent.mousePosition);
            DidHoverHighlight = false;
        }

        /************************************************************************************************************************/

        /// <summary>Repaints the `window` if necessary.</summary>
        public void EndGUI(TransitionLibraryWindow window)
        {
            if (DidHoverHighlight && window != EditorWindow.mouseOverWindow)
            {
                DidHoverHighlight = false;
                window.Repaint();
            }
            else if (EventType == EventType.MouseMove)
            {
                window.Repaint();
            }
        }

        /************************************************************************************************************************/

        /// <summary>Draws highlights for the `area`.</summary>
        public void DrawHighlightGUI(Rect area, bool selected, bool hover)
        {
            if (selected)
            {
                EditorGUI.DrawRect(area, SelectionHighlightColor);
            }

            if (hover)
            {
                DidHoverHighlight = true;
                EditorGUI.DrawRect(area, HoverHighlightColor);
            }
        }

        /************************************************************************************************************************/
    }
}

#endif


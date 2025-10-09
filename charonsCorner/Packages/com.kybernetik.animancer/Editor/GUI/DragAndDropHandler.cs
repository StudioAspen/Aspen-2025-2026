// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only]
    /// Delegate for validating and responding to <see cref="DragAndDrop"/> operations.
    /// </summary>
    /// <remarks>
    /// 
    /// <strong>Example:</strong>
    /// <code>
    /// private DragAndDropHandler&lt;AnimationClip&gt; _AnimationDropHandler;
    /// 
    /// void OnGUI(Rect area)
    /// {
    ///     _AnimationDropHandler ??= (clip, isDrop) =>
    ///     {
    ///         if (clip.legacy)// Reject legacy animations
    ///             return false;
    ///             
    ///         if (isDrop)// Only act when dropping.
    ///             Debug.Log(clip + " was dropped");
    ///     
    ///         return true;// Drag or drop is accepted.
    ///     };
    ///     
    ///     _AnimationDropHandler.Handle(area);
    /// }
    /// </code></remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/DragAndDropHandler_1
    public delegate bool DragAndDropHandler<T>(
        T dragging,
        bool isDrop)
        where T : class;

    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerGUI
    public static partial class AnimancerGUI
    {
        /************************************************************************************************************************/

        /// <summary>Handles the current event.</summary>
        /// <remarks>See <see cref="DragAndDropHandler{T}"/> for a usage example.</remarks>
        public static bool Handle<T>(
            this DragAndDropHandler<T> handler,
            Rect area,
            DragAndDropVisualMode mode = DragAndDropVisualMode.Link)
            where T : class
        {
            var currentEvent = Event.current;

            bool isDrop;
            switch (currentEvent.type)
            {
                case EventType.DragUpdated:
                    isDrop = false;
                    break;

                case EventType.DragPerform:
                    isDrop = true;
                    break;

                default:
                    return false;
            }

            if (!area.Contains(currentEvent.mousePosition))
                return false;

            return handler.Handle(DragAndDrop.objectReferences, isDrop, mode);
        }

        /************************************************************************************************************************/

        /// <summary>Handles the current event.</summary>
        /// <remarks>See <see cref="DragAndDropHandler{T}"/> for a usage example.</remarks>
        public static bool Handle<T>(
            this DragAndDropHandler<T> handler,
            IEnumerable dragging,
            bool isDrop,
            DragAndDropVisualMode mode = DragAndDropVisualMode.Link)
            where T : class
        {
            if (dragging == null)
                return false;

            var droppedAny = false;

            foreach (var obj in dragging)
            {
                if (obj is not T t ||
                    !handler(t, isDrop))
                    continue;

                Deselect();
                Event.current.Use();

                if (isDrop)
                {
                    droppedAny = true;
                }
                else
                {
                    DragAndDrop.visualMode = mode;
                    return true;
                }
            }

            if (!droppedAny)
                return false;

            DragAndDrop.AcceptDrag();
            return true;
        }

        /************************************************************************************************************************/
    }
}

#endif


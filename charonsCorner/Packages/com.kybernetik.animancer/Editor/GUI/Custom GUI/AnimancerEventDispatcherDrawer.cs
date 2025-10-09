// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] A custom GUI for an <see cref="AnimancerEvent.Dispatcher"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerEventDispatcherDrawer
    [CustomGUI(typeof(AnimancerEvent.Dispatcher))]
    public class AnimancerEventDispatcherDrawer : CustomGUI<AnimancerEvent.Dispatcher>
    {
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void DoGUI()
        {
            var state = Value.State;
            var events = state?.SharedEvents;
            if (events == null)
            {
                EditorGUILayout.LabelField("Event Dispatcher", "Null");
                return;
            }

            var targetPath = state != null
                ? state.GetPath()
                : "Null";

            var eventSequenceDrawer = EventSequenceDrawer.Get(events);
            var area = AnimancerGUI.LayoutRect(eventSequenceDrawer.CalculateHeight(events));
            using (var label = PooledGUIContent.Acquire("Event Dispatcher"))
            using (var summary = PooledGUIContent.Acquire(targetPath))
                eventSequenceDrawer.DoGUI(ref area, events, label, summary);

            if (eventSequenceDrawer.IsExpanded && state != null)
            {
                EditorGUI.indentLevel++;

                var enabled = GUI.enabled;
                GUI.enabled = false;
                EditorGUILayout.Toggle("Has Owned Events", state.HasOwnedEvents);
                GUI.enabled = enabled;

                EditorGUI.indentLevel--;
            }
        }

        /************************************************************************************************************************/
    }
}

#endif


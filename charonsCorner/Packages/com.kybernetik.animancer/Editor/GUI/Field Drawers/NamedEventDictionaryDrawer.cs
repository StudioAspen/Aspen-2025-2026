// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR && UNITY_IMGUI

using System;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Draws the Inspector GUI for an <see cref="NamedEventDictionary"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/NamedEventDictionaryDrawer
    /// 
    public static class NamedEventDictionaryDrawer
    {
        /************************************************************************************************************************/

        private const string
            KeyPrefix = AnimancerGraphDrawer.KeyPrefix;

        private static readonly BoolPref
            AreEventsExpanded = new(KeyPrefix + nameof(AreEventsExpanded), false);

        /************************************************************************************************************************/

        /// <summary>Draws the <see cref="AnimancerGraph.Events"/>.</summary>
        public static void DoEventsGUI(AnimancerGraph graph)
        {
            if (!graph.HasEvents)
                return;

            EditorGUI.indentLevel++;

            var events = graph.Events;

            AreEventsExpanded.Value = AnimancerGUI.DoLabelFoldoutFieldGUI(
                "Events",
                events.Count.ToStringCached(),
                AreEventsExpanded);

            if (AreEventsExpanded)
            {
                EditorGUI.indentLevel++;

                var sortedEvents = ListPool.Acquire<StringReference>();
                sortedEvents.AddRange(events.Keys);
                sortedEvents.Sort();

                foreach (var item in sortedEvents)
                    DoEventGUI(item, events[item]);

                ListPool.Release(sortedEvents);

                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }

        /************************************************************************************************************************/

        /// <summary>Draws an event.</summary>
        public static void DoEventGUI(string name, Action action)
        {
            var gui = CustomGUIFactory.GetOrCreateForObject(action);
            if (gui == null)
            {
                EditorGUILayout.LabelField(name, action.ToStringDetailed());
                return;
            }

            gui.SetLabel(name);
            gui.DoGUI();
        }

        /************************************************************************************************************************/
    }
}

#endif

